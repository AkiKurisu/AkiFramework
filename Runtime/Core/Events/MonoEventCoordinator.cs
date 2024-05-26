using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Events
{
    public enum MonoDispatchType
    {
        Update,
        FixedUpdate = 1,
        LateUpdate = 2,
    }

    /// <summary>
    /// MonoBehaviour based EventCoordinator that can be enabled and disabled, and can be tracked by the debugger
    /// </summary>
    public abstract class MonoEventCoordinator : MonoBehaviour, IEventCoordinator
    {
        public virtual EventDispatcher EventDispatcher { get; protected set; }
        public MonoDispatchType DispatchStatus { get; private set; }
        public virtual CallbackEventHandler RootEventHandler { get; protected set; }
        private readonly HashSet<ICoordinatorDebugger> m_Debuggers = new();
        private readonly Queue<EventBase> updateQueue = new();
        private readonly Queue<EventBase> lateUpdateQueue = new();
        private readonly Queue<EventBase> fixedUpdateQueue = new();
        protected virtual void Awake()
        {
            EventDispatcher = EventDispatcher.CreateDefault();
        }
        protected virtual void Update()
        {
            DispatchStatus = MonoDispatchType.Update;
            DrainQueue(DispatchStatus);
            EventDispatcher.PushDispatcherContext();
            EventDispatcher.PopDispatcherContext();
        }
        protected virtual void FixedUpdate()
        {
            DispatchStatus = MonoDispatchType.FixedUpdate;
            DrainQueue(DispatchStatus);
            EventDispatcher.PushDispatcherContext();
            EventDispatcher.PopDispatcherContext();
        }
        protected virtual void LateUpdate()
        {
            DispatchStatus = MonoDispatchType.LateUpdate;
            DrainQueue(DispatchStatus);
            EventDispatcher.PushDispatcherContext();
            EventDispatcher.PopDispatcherContext();
        }
        protected virtual void OnDestroy()
        {
            DetachAllDebuggers();
        }
        public void Dispatch(EventBase evt, DispatchMode dispatchMode, MonoDispatchType monoDispatchType)
        {
            if (dispatchMode == DispatchMode.Immediate || monoDispatchType == DispatchStatus)
            {
                EventDispatcher.Dispatch(evt, this, dispatchMode);
                Refresh();
            }
            else
            {
                //Acquire to ensure not released
                evt.Acquire();
                GetDispatchQueue(monoDispatchType).Enqueue(evt);
            }
        }
        private void DrainQueue(MonoDispatchType monoDispatchType)
        {
            var queue = GetDispatchQueue(monoDispatchType);
            foreach (var evt in queue)
            {
                try
                {
                    EventDispatcher.Dispatch(evt, this, DispatchMode.Queued);
                }
                finally
                {
                    // Balance the Acquire when the event was put in queue.
                    evt.Dispose();
                }
            }
            queue.Clear();
            Refresh();
        }
        private Queue<EventBase> GetDispatchQueue(MonoDispatchType monoDispatchType)
        {
            return monoDispatchType switch
            {
                MonoDispatchType.Update => updateQueue,
                MonoDispatchType.FixedUpdate => fixedUpdateQueue,
                MonoDispatchType.LateUpdate => lateUpdateQueue,
                _ => throw new ArgumentOutOfRangeException(nameof(monoDispatchType)),
            };
        }
        internal void AttachDebugger(ICoordinatorDebugger debugger)
        {
            if (debugger != null && m_Debuggers.Add(debugger))
            {
                debugger.CoordinatorDebug = this;
            }
        }
        internal void DetachDebugger(ICoordinatorDebugger debugger)
        {
            if (debugger != null)
            {
                debugger.CoordinatorDebug = null;
                m_Debuggers.Remove(debugger);
            }
        }
        internal void DetachAllDebuggers()
        {
            foreach (var debugger in m_Debuggers)
            {
                debugger.CoordinatorDebug = null;
                debugger.Disconnect();
            }
        }
        internal IEnumerable<ICoordinatorDebugger> GetAttachedDebuggers()
        {
            return m_Debuggers;
        }
        public void Refresh()
        {
            foreach (var debugger in m_Debuggers)
            {
                debugger.Refresh();
            }
        }
        public bool InterceptEvent(EventBase ev)
        {
            bool intercepted = false;
            foreach (var debugger in m_Debuggers)
            {
                intercepted |= debugger.InterceptEvent(ev);
            }
            return intercepted;
        }

        public void PostProcessEvent(EventBase ev)
        {
            foreach (var debugger in m_Debuggers)
            {
                debugger.PostProcessEvent(ev);
            }
        }
    }
}