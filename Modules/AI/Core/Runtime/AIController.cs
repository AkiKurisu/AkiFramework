using UnityEngine;
using System.Collections.Generic;
using System;
using Kurisu.AkiBT;
namespace Kurisu.Framework.AI
{
    [RequireComponent(typeof(BlackBoardComponent))]
    public abstract class AIController : MonoBehaviour
    {
        [SerializeField]
        private BehaviorTask[] behaviorTasks;
        protected Dictionary<string, IAITask> TaskMap { get; } = new();
        public bool IsAIEnabled { get; protected set; }
        public virtual Transform Transform => transform;
        public virtual GameObject Object => gameObject;
        public BlackBoard BlackBoard { get; private set; }
        protected virtual void Awake()
        {
            BlackBoard = GetComponent<BlackBoardComponent>().GetBlackBoard();
        }
        protected virtual void Start()
        {
            SetupBehaviorTree();
        }
        public abstract IAIPawn GetAIPawn();
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
    /// AI Controller for <see cref="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AIController<T> : AIController where T : Actor, IAIPawn
    {
        public T Pawn { get; protected set; }
        public sealed override IAIPawn GetAIPawn()
        {
            return Pawn;
        }
    }
}

