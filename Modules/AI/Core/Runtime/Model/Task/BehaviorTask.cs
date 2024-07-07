using System;
using Kurisu.AkiBT;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.AI
{
    /// <summary>
    /// Task controlled with external state machine
    /// </summary>
    public abstract class StatusTask
    {
        public TaskStatus Status { get; private set; }
        #region  Status controlled by agent
        public void Stop()
        {
            Status = TaskStatus.Disabled;
            OnStop();
        }
        protected virtual void OnStop() { }
        public void Start()
        {
            Status = TaskStatus.Enabled;
            OnStart();
        }
        protected virtual void OnStart() { }
        public void Pause()
        {
            Status = TaskStatus.Pending;
            OnPause();
        }
        protected virtual void OnPause() { }
        #endregion
    }
    /// <summary>
    /// Task to run a behavior tree inside a agent-authority state machine.
    /// Whether behavior tree is failed or succeed will not affect task status. 
    /// </summary>
    [Serializable]
    public class BehaviorTask : StatusTask, IAITask, IBehaviorTreeContainer
    {
        [SerializeField, TaskID]
        private string taskID;
        public string TaskID => taskID;
        [SerializeField]
        private bool isPersistent;
        public bool IsPersistent => isPersistent;
        [SerializeField]
        private BehaviorTreeAsset behaviorTreeAsset;
        public BehaviorTree InstanceTree { get; private set; }
        public Object Object => host.Object;
        private AIController host;
        public void Init(AIController host)
        {
            this.host = host;
            InstanceTree = behaviorTreeAsset.GetBehaviorTree();
            InstanceTree.InitVariables();
            InstanceTree.BlackBoard.MapTo(host.BlackBoard);
            InstanceTree.Run(host.Object);
            InstanceTree.Awake();
            InstanceTree.Start();
        }
        public void Tick()
        {
            InstanceTree.Tick();
        }

        public BehaviorTree GetBehaviorTree()
        {
            // get runtime instance tree only
            return InstanceTree;
        }

        public void SetBehaviorTreeData(BehaviorTreeData behaviorTreeData)
        {
            // should not edit instance
            return;
        }
    }
}