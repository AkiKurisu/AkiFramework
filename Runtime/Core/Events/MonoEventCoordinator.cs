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
        //Currently use different dispatcher which means should register and unregister event callBack from specified dispatcher.
        //TODO: Maybe only need one dispatcher and use strategy to change where event should be dispatched on
        public virtual EventDispatcher UpdateDispatcher { get; protected set; }
        public virtual EventDispatcher LateUpdateDispatcher { get; protected set; }
        public virtual EventDispatcher FixedUpdateDispatcher { get; protected set; }
        private readonly HashSet<ICoordinatorDebugger> m_Debuggers = new();
        protected virtual void Awake()
        {
            UpdateDispatcher = EventDispatcher.CreateDefault();
            LateUpdateDispatcher = EventDispatcher.CreateDefault();
            FixedUpdateDispatcher = EventDispatcher.CreateDefault();
        }
        protected virtual void Update()
        {
            UpdateDispatcher.PushDispatcherContext();
            UpdateDispatcher.PopDispatcherContext();
        }
        protected virtual void FixedUpdate()
        {
            LateUpdateDispatcher.PushDispatcherContext();
            LateUpdateDispatcher.PopDispatcherContext();
        }
        protected virtual void LateUpdate()
        {
            FixedUpdateDispatcher.PushDispatcherContext();
            FixedUpdateDispatcher.PopDispatcherContext();
        }
        protected virtual void OnDestroy()
        {
            DetachAllDebuggers();
        }
        public EventDispatcher GetDispatcher(MonoDispatchType frameDispatchType)
        {
            return frameDispatchType switch
            {
                MonoDispatchType.Update => UpdateDispatcher,
                MonoDispatchType.FixedUpdate => FixedUpdateDispatcher,
                MonoDispatchType.LateUpdate => LateUpdateDispatcher,
                _ => throw new ArgumentOutOfRangeException(nameof(frameDispatchType)),
            };
        }
        public void Dispatch(EventBase evt, DispatchMode dispatchMode, MonoDispatchType frameDispatchType = MonoDispatchType.Update)
        {
            GetDispatcher(frameDispatchType).Dispatch(evt, this, dispatchMode);
            Refresh();
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