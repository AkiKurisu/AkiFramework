using UnityEngine;
namespace Kurisu.Framework.Events
{
    /// <summary>
    /// CallbackEventHandler of having behaviour lifetime scope.
    /// </summary>
    public abstract class BehaviourCallbackEventHandler : CallbackEventHandler
    {
        internal protected Behaviour AttachedBehaviour { get; set; }
        public BehaviourCallbackEventHandler(Behaviour attachedBehaviour) : base()
        {
            AttachedBehaviour = attachedBehaviour;
        }
        public bool IsActiveAndEnabled => AttachedBehaviour.isActiveAndEnabled;
    }
    /// <summary>
    /// Interface for classes capable of having callbacks to handle events.
    /// </summary>
    public abstract class CallbackEventHandler : IEventHandler
    {
        private EventCallbackRegistry m_CallbackRegistry;

        /// <summary>
        /// Adds an event handler to the instance. If the event handler has already been registered for the same phase (either TrickleDown or BubbleUp) then this method has no effect.
        /// </summary>
        /// <param name="callback">The event handler to add.</param>
        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, default);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback);
#endif
            AddEventCategories<TEventType>();
        }

        private void AddEventCategories<TEventType>() where TEventType : EventBase<TEventType>, new()
        {
            //TODO: Encode event categories
        }

        /// <summary>
        /// Adds an event handler to the instance. If the event handler has already been registered for the same phase then this method has no effect.
        /// </summary>
        /// <param name="callback">The event handler to add.</param>
        /// <param name="userArgs">Data to pass to the callback.</param>
        internal void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, userArgs, default);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback);
#endif
            AddEventCategories<TEventType>();
        }

        internal void RegisterCallback<TEventType>(EventCallback<TEventType> callback, InvokePolicy invokePolicy) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, invokePolicy);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback);
#endif
            AddEventCategories<TEventType>();
        }

        /// <summary>
        /// Remove callback from the instance.
        /// </summary>
        /// <param name="callback">The callback to remove. If this callback was never registered, nothing happens.</param>
        public void UnregisterCallback<TEventType>(EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry?.UnregisterCallback(callback);
#if UNITY_EDITOR
            GlobalCallbackRegistry.UnregisterListeners<TEventType>(this, callback);
#endif
        }

        /// <summary>
        /// Remove callback from the instance.
        /// </summary>
        /// <param name="callback">The callback to remove. If this callback was never registered, nothing happens.</param>
        internal void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry?.UnregisterCallback(callback);
#if UNITY_EDITOR
            GlobalCallbackRegistry.UnregisterListeners<TEventType>(this, callback);
#endif
        }

        internal bool TryGetUserArgs<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback, out TCallbackArgs userData) where TEventType : EventBase<TEventType>, new()
        {
            userData = default;

            if (m_CallbackRegistry != null)
            {
                return m_CallbackRegistry.TryGetUserArgs(callback, out userData);
            }

            return false;
        }

        /// <summary>
        /// Sends an event to the event handler.
        /// </summary>
        /// <param name="e">The event to send.</param>
        public abstract void SendEvent(EventBase e);

        public abstract void SendEvent(EventBase e, DispatchMode dispatchMode);

        internal protected void HandleEventAtTargetPhase(EventBase evt)
        {
            evt.CurrentTarget = evt.Target;
            evt.PropagationPhase = PropagationPhase.AtTarget;
            HandleEventAtCurrentTargetAndPhase(evt);
            evt.PropagationPhase = PropagationPhase.DefaultActionAtTarget;
            HandleEventAtCurrentTargetAndPhase(evt);
        }

        internal protected void HandleEventAtTargetAndDefaultPhase(EventBase evt)
        {
            HandleEventAtTargetPhase(evt);
            evt.PropagationPhase = PropagationPhase.DefaultAction;
            HandleEventAtCurrentTargetAndPhase(evt);
        }

        internal protected void HandleEventAtCurrentTargetAndPhase(EventBase evt)
        {
            if (evt == null)
                return;

            switch (evt.PropagationPhase)
            {
                case PropagationPhase.AtTarget:
                    {
                        if (!evt.IsPropagationStopped)
                        {
                            m_CallbackRegistry?.InvokeCallbacks(evt, PropagationPhase.AtTarget);
                        }
                    }
                    break;
                case PropagationPhase.DefaultActionAtTarget:
                    {
                        if (!evt.IsDefaultPrevented)
                        {
                            using (new EventDebuggerLogExecuteDefaultAction(evt))
                            {
                                if (evt.SkipDisabledElements && this is BehaviourCallbackEventHandler me && !me.IsActiveAndEnabled)
                                    ExecuteDefaultActionDisabledAtTarget(evt);
                                else
                                    ExecuteDefaultActionAtTarget(evt);
                            }
                        }
                        break;
                    }

                case PropagationPhase.DefaultAction:
                    {
                        if (!evt.IsDefaultPrevented)
                        {
                            using (new EventDebuggerLogExecuteDefaultAction(evt))
                            {
                                if (evt.SkipDisabledElements && this is BehaviourCallbackEventHandler me && !me.IsActiveAndEnabled)
                                    ExecuteDefaultActionDisabled(evt);
                                else
                                    ExecuteDefaultAction(evt);
                            }
                        }
                        break;
                    }
            }
        }


        void IEventHandler.HandleEvent(EventBase evt)
        {
            HandleEventAtCurrentTargetAndPhase(evt);
        }


        /// <summary>
        /// Executes logic after the callbacks registered on the event target have executed,
        /// unless the event is marked to prevent its default behaviour.
        /// <see cref="EventBase.PreventDefault"/>.
        /// </summary>
        /// <param name="evt">The event instance.</param>
        protected virtual void ExecuteDefaultActionAtTarget(EventBase evt) { }

        /// <summary>
        /// Executes logic after the callbacks registered on the event target have executed,
        /// unless the event has been marked to prevent its default behaviour.
        /// <see cref="EventBase.PreventDefault"/>.
        /// </summary>
        /// <remarks>
        /// This method is designed to be overridden by subclasses. Use it to implement event handling without
        /// registering callbacks which guarantees precedences of callbacks registered by users of the subclass.
        /// Unlike <see cref="ExecuteDefaultActionAtTarget"/>, this method is called after the callbacks registered
        /// on the element
        /// </remarks>
        /// <param name="evt">The event instance.</param>
        protected virtual void ExecuteDefaultAction(EventBase evt) { }

        protected virtual void ExecuteDefaultActionDisabledAtTarget(EventBase evt) { }

        protected virtual void ExecuteDefaultActionDisabled(EventBase evt) { }
    }
}
