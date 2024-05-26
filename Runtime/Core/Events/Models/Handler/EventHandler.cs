using UnityEngine;
namespace Kurisu.Framework.Events
{
    /// <summary>
    /// CallbackEventHandler of having behaviour lifetime scope.
    /// </summary>
    public abstract class BehaviourCallbackEventHandler : CallbackEventHandler, IBehaviourScope
    {
        public Behaviour AttachedBehaviour { get; set; }
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
        public virtual bool IsCompositeRoot => false;
        public abstract IEventCoordinator Root { get; }
        /// <summary>
        /// Get and set parent callBack handler
        /// </summary>
        /// <value></value>
        public CallbackEventHandler Parent { get; protected set; } = null;
        /// <summary>
        /// Get and set children callBack handlers
        /// </summary>
        /// <value></value>
        public CallbackEventHandler[] Children { get; protected set; } = null;
        private EventCallbackRegistry m_CallbackRegistry;

        /// <summary>
        /// Adds an event handler to the instance. If the event handler has already been registered for the same phase (either TrickleDown or BubbleUp) then this method has no effect.
        /// </summary>
        /// <param name="callback">The event handler to add.</param>
        public void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, useTrickleDown, default);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback, useTrickleDown);
#endif
            // AddEventCategories<TEventType>();
        }
        internal void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown, int skipFrame = 2) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, useTrickleDown, default);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback, useTrickleDown, skipFrame);
#endif
            // AddEventCategories<TEventType>();
        }

        //TODO: Encode event categories
        // private void AddEventCategories<TEventType>() where TEventType : EventBase<TEventType>, new()
        // {

        // }

        /// <summary>
        /// Adds an event handler to the instance. If the event handler has already been registered for the same phase then this method has no effect.
        /// </summary>
        /// <param name="callback">The event handler to add.</param>
        /// <param name="userArgs">Data to pass to the callback.</param>
        public void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, userArgs, useTrickleDown, default);
#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback, useTrickleDown);
#endif
            // AddEventCategories<TEventType>();
        }

        internal void RegisterCallback<TEventType>(EventCallback<TEventType> callback, InvokePolicy invokePolicy, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry ??= new EventCallbackRegistry();

            m_CallbackRegistry.RegisterCallback(callback, useTrickleDown, invokePolicy);

#if UNITY_EDITOR
            GlobalCallbackRegistry.RegisterListeners<TEventType>(this, callback, useTrickleDown);
#endif

            // AddEventCategories<TEventType>();
        }

        /// <summary>
        /// Remove callback from the instance.
        /// </summary>
        /// <param name="callback">The callback to remove. If this callback was never registered, nothing happens.</param>
        /// <param name="useTrickleDown">Set this parameter to true to remove the callback from the TrickleDown phase. Set this parameter to false to remove the callback from the BubbleUp phase.</param>
        public void UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry?.UnregisterCallback(callback, useTrickleDown);

#if UNITY_EDITOR
            GlobalCallbackRegistry.UnregisterListeners<TEventType>(this, callback);
#endif
        }

        /// <summary>
        /// Remove callback from the instance.
        /// </summary>
        /// <param name="callback">The callback to remove. If this callback was never registered, nothing happens.</param>
        /// <param name="useTrickleDown">Set this parameter to true to remove the callback from the TrickleDown phase. Set this parameter to false to remove the callback from the BubbleUp phase.</param>
        public void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            m_CallbackRegistry?.UnregisterCallback(callback, useTrickleDown);

#if UNITY_EDITOR
            GlobalCallbackRegistry.UnregisterListeners<TEventType>(this, callback);
#endif
        }

        internal bool TryGetUserArgs<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback, TrickleDown useTrickleDown, out TCallbackArgs userData) where TEventType : EventBase<TEventType>, new()
        {
            userData = default;

            if (m_CallbackRegistry != null)
            {
                return m_CallbackRegistry.TryGetUserArgs(callback, useTrickleDown, out userData);
            }

            return false;
        }

        /// <summary>
        /// Sends an event to the event handler.
        /// </summary>
        /// <param name="e">The event to send.</param>
        public abstract void SendEvent(EventBase e);

        public abstract void SendEvent(EventBase e, DispatchMode dispatchMode);

        public void HandleEventAtTargetPhase(EventBase evt)
        {
            evt.CurrentTarget = evt.Target;
            evt.PropagationPhase = PropagationPhase.AtTarget;
            HandleEventAtCurrentTargetAndPhase(evt);
            evt.PropagationPhase = PropagationPhase.DefaultActionAtTarget;
            HandleEventAtCurrentTargetAndPhase(evt);
        }

        public void HandleEventAtTargetAndDefaultPhase(EventBase evt)
        {
            HandleEventAtTargetPhase(evt);
            evt.PropagationPhase = PropagationPhase.DefaultAction;
            HandleEventAtCurrentTargetAndPhase(evt);
        }

        public void HandleEventAtCurrentTargetAndPhase(EventBase evt)
        {
            if (evt == null)
                return;

            switch (evt.PropagationPhase)
            {
                case PropagationPhase.TrickleDown:
                case PropagationPhase.BubbleUp:
                    {
                        if (!evt.IsPropagationStopped)
                        {
                            m_CallbackRegistry?.InvokeCallbacks(evt, evt.PropagationPhase);
                        }
                        break;
                    }
                case PropagationPhase.AtTarget:
                    {
                        //We make sure we invoke callbacks from the TrickleDownPhase before the BubbleUp ones when we are directly at target
                        if (!evt.IsPropagationStopped)
                        {
                            m_CallbackRegistry?.InvokeCallbacks(evt, PropagationPhase.TrickleDown);
                        }
                        if (!evt.IsPropagationStopped)
                        {
                            m_CallbackRegistry?.InvokeCallbacks(evt, PropagationPhase.BubbleUp);
                        }
                    }
                    break;
                case PropagationPhase.DefaultActionAtTarget:
                    {
                        if (!evt.IsDefaultPrevented)
                        {
                            using (new EventDebuggerLogExecuteDefaultAction(evt))
                            {
                                if (evt.SkipDisabledElements && this is IBehaviourScope bs && !bs.AttachedBehaviour.isActiveAndEnabled)
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
                                if (evt.SkipDisabledElements && this is IBehaviourScope bs && !bs.AttachedBehaviour.isActiveAndEnabled)
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
        /// Returns true if event handlers, for the event propagation TrickleDown phase, are attached to this object.
        /// </summary>
        /// <returns>True if object has event handlers for the TrickleDown phase.</returns>
        public bool HasTrickleDownHandlers()
        {
            return m_CallbackRegistry != null && m_CallbackRegistry.HasTrickleDownHandlers();
        }

        /// <summary>
        /// Return true if event handlers for the event propagation BubbleUp phase have been attached on this object.
        /// </summary>
        /// <returns>True if object has event handlers for the BubbleUp phase.</returns>
        public bool HasBubbleUpHandlers()
        {
            return m_CallbackRegistry != null && m_CallbackRegistry.HasBubbleHandlers();
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
