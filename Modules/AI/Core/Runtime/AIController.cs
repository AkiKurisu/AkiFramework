using UnityEngine;
using System.Collections.Generic;
using System;
using Kurisu.AkiBT;
using Kurisu.Framework;
namespace Kurisu.Framework.AI
{
    [RequireComponent(typeof(BlackBoardComponent))]
    public abstract class AIController : Actor
    {
        [SerializeField]
        private BehaviorTask[] behaviorTasks;
        protected Dictionary<string, IAITask> TaskMap { get; } = new();
        public bool IsAIEnabled { get; protected set; }
        public virtual Transform Transform => transform;
        public virtual GameObject Object => gameObject;
        public BlackBoard BlackBoard { get; private set; }
        public abstract IAIContext Context { get; }
        protected virtual void Awake()
        {
            BlackBoard = GetComponent<BlackBoardComponent>().GetBlackBoard();
        }
        protected virtual void Start()
        {
            SetupBehaviorTree();
        }
        private void SetupBehaviorTree()
        {
            foreach (var task in behaviorTasks)
            {
                AddTask(task);
            }
        }
        protected virtual void Update()
        {
            if (!IsAIEnabled) return;
            TickTasks();
            OnUpdate();
        }
        protected virtual void OnDestroy()
        {
            foreach (var task in TaskMap.Values)
            {
                if (task is IDisposable disposable)
                    disposable.Dispose();
            }
        }
        protected void TickTasks()
        {
            foreach (var task in TaskMap.Values)
            {
                if (task.Status == TaskStatus.Enabled) task.Tick();
            }
        }
        protected virtual void OnUpdate() { }
        public void EnableAI()
        {
            IsAIEnabled = true;
            foreach (var task in TaskMap.Values)
            {
                if (task.IsPersistent || task.Status == TaskStatus.Pending)
                    task.Start();
            }
        }
        public void DisableAI()
        {
            IsAIEnabled = false;
            foreach (var task in TaskMap.Values)
            {
                //Pend running tasks
                if (task.Status == TaskStatus.Enabled)
                    task.Pause();
            }
        }
        protected virtual void OnEnable()
        {
            EnableAI();
        }
        protected virtual void OnDisable()
        {
            DisableAI();
        }
        public IAITask GetTask(string taskID)
        {
            return TaskMap[taskID];
        }
        public void AddTask(IAITask task)
        {
            if (!task.IsPersistent && TaskMap.ContainsKey(task.TaskID))
            {
                Debug.LogWarning($"Already contained task with same id: {task.TaskID}");
                return;
            }
            task.Init(this);
            TaskMap.Add(task.TaskID, task);
            if (task.IsPersistent && IsAIEnabled)
            {
                task.Start();
            }
        }
        public IEnumerable<IAITask> GetAllTasks()
        {
            return TaskMap.Values;
        }
    }
    /// <summary>
    /// AI Agent for custom context (eg. Data model, GamePlay components)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AIController<T> : AIController where T : IAIContext
    {
        public sealed override IAIContext Context => TContext;
        public abstract T TContext { get; }
    }
}

