using System;
using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.Pool;
using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    public enum TaskStatus
    {
        /// <summary>
        /// Task is enabled to run and can be updated
        /// </summary>
        Running,
        /// <summary>
        /// Task is paused and will be ignored
        /// </summary>
        Paused,
        /// <summary>
        /// Task is completed and can not be updated any longer
        /// </summary>
        Completed,
        /// <summary>
        /// Task is not completed but is stopped externally
        /// </summary>
        Stopped
    }
    public class TaskCompleteEvent : EventBase<TaskCompleteEvent>
    {
        public TaskBase Task { get; private set; }
        public readonly List<TaskBase> Listeners = new();
        public static TaskCompleteEvent GetPooled(TaskBase task)
        {
            var evt = GetPooled();
            evt.Task = task;
            evt.Listeners.Clear();
            return evt;
        }
        public void AddListenerTask(TaskBase taskBase)
        {
            Listeners.Add(taskBase);
        }
    }
    /// <summary>
    /// Base class for framework task
    /// </summary>
    public abstract class TaskBase : IDisposable
    {
        protected TaskStatus mStatus;
        public virtual TaskStatus GetStatus()
        {
            return mStatus;
        }
        public abstract string GetTaskID();

        protected virtual void Init()
        {
            mStatus = TaskStatus.Stopped;
        }

        public virtual void Stop()
        {
            mStatus = TaskStatus.Stopped;
        }
        public virtual void Start()
        {
            mStatus = TaskStatus.Running;
        }
        public virtual void Pause()
        {
            mStatus = TaskStatus.Paused;
        }

        public virtual void Tick()
        {

        }
        protected virtual void Reset()
        {
            mStatus = TaskStatus.Stopped;
        }
        public virtual void Dispose()
        {
            if (completeEvent != null)
            {
                completeEvent.Dispose();
                completeEvent = null;
            }
        }
        public virtual void Acquire()
        {

        }
        private TaskCompleteEvent completeEvent;
        private int prerequisiteCount;
        internal void PostComplete()
        {
            if (completeEvent == null) return;
            EventSystem.EventHandler.SendEvent(completeEvent);
            completeEvent.Dispose();
            completeEvent = null;
        }
        internal void ReleasePrerequistite()
        {
            prerequisiteCount--;
        }
        internal bool HasPrerequistites()
        {
            return prerequisiteCount > 0;
        }
        private TaskCompleteEvent GetCompleteEvent()
        {
            completeEvent ??= TaskCompleteEvent.GetPooled(this);
            return completeEvent;
        }
        public void RegisterPrerequisite(TaskBase taskBase)
        {
            taskBase.GetCompleteEvent().AddListenerTask(this);
            prerequisiteCount++;
        }
        public virtual string GetTaskName()
        {
            return GetType().Name;
        }
    }
    public abstract class PooledTaskBase<T> : TaskBase where T : PooledTaskBase<T>, new()
    {
        private int m_RefCount;
        private bool pooled;
        private static readonly _ObjectPool<T> s_Pool = new(() => new T());
        private static readonly string defaultName;
        static PooledTaskBase()
        {
            defaultName = typeof(T).Name;
        }
        protected PooledTaskBase() : base()
        {
            m_RefCount = 0;
        }
        public sealed override void Dispose()
        {
            if (--m_RefCount == 0)
            {
                base.Dispose();
                ReleasePooled((T)this);
            }
        }
        private static void ReleasePooled(T evt)
        {
            if (evt.pooled)
            {
                evt.Reset();
                s_Pool.Release(evt);
                evt.pooled = false;
            }
        }
        public static T GetPooled()
        {
            T t = s_Pool.Get();
            t.Init();
            t.pooled = true;
            return t;
        }
        protected override void Init()
        {
            base.Init();
            if (m_RefCount != 0)
            {
                Debug.LogWarning($"Task improperly released, reference count {m_RefCount}.");
                m_RefCount = 0;
            }
        }
        public override void Acquire()
        {
            m_RefCount++;
        }
        public override string GetTaskID()
        {
            return defaultName;
        }
    }
}
