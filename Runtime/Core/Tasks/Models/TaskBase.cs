using System;
using Kurisu.Framework.Pool;
using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    public enum TaskStatus
    {
        /// <summary>
        /// Task is enabled and can be updated
        /// </summary>
        Enabled,
        /// <summary>
        /// Task is paused and will be ignored
        /// </summary>
        Paused,
        /// <summary>
        /// Task is disabled and can not be updated
        /// </summary>
        Disabled
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

        }

        public virtual void Stop()
        {
            mStatus = TaskStatus.Disabled;
        }
        public virtual void Start()
        {
            mStatus = TaskStatus.Enabled;
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
            mStatus = TaskStatus.Disabled;
        }
        public virtual void Dispose()
        {

        }
        public virtual void Acquire()
        {

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
