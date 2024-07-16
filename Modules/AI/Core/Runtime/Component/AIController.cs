using UnityEngine;
using System.Collections.Generic;
using Kurisu.AkiBT;
using Kurisu.Framework.Tasks;
namespace Kurisu.Framework.AI
{
    [RequireComponent(typeof(BlackBoardComponent))]
    public abstract class AIController : Controller
    {
        [SerializeField]
        private BehaviorTask[] behaviorTasks;
        protected Dictionary<string, TaskBase> TaskMap { get; } = new();
        public bool IsAIEnabled { get; protected set; }
        public BlackBoard BlackBoard { get; private set; }
        protected virtual void Awake()
        {
            BlackBoard = GetComponent<BlackBoardComponent>().GetBlackBoard();
        }
        protected virtual void Start()
        {
            SetupBehaviorTree();
        }
        public sealed override bool IsBot()
        {
            return true;
        }
        public IAIPawn GetAIPawn()
        {
            return GetActor() as IAIPawn;
        }
        public TPawn GetPawn<TPawn>() where TPawn : Actor, IAIPawn
        {
            return GetActor() as TPawn;
        }
        private void SetupBehaviorTree()
        {
            foreach (var task in behaviorTasks)
            {
                AddTask(task);
            }
        }
        protected override void OnDestroy()
        {
            foreach (var task in TaskMap.Values)
            {
                task.Stop();
                task.Dispose();
            }
            base.OnDestroy();
        }
        public void EnableAI()
        {
            IsAIEnabled = true;
            foreach (var task in TaskMap.Values)
            {
                if (((IAITask)task).IsStartOnEnabled())
                    task.Start();
            }
        }
        public void DisableAI()
        {
            IsAIEnabled = false;
            foreach (var task in TaskMap.Values)
            {
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
        public TaskBase GetTask(string taskID)
        {
            return TaskMap[taskID];
        }
        public void AddTask<T>(T task) where T : TaskBase, IAITask
        {
            string id = task.GetTaskID();
            if (TaskMap.ContainsKey(id))
            {
                Debug.LogWarning($"Already contained task with same id: {id}");
                return;
            }
            task.SetController(this);
            task.Acquire();
            TaskMap.Add(id, task);
            TaskRunner.RegisterTask(task);
            if (IsAIEnabled && task.IsStartOnEnabled())
            {
                task.Start();
            }
        }
        public IEnumerable<TaskBase> GetAllTasks()
        {
            return TaskMap.Values;
        }
    }
}

