using UnityEngine;
namespace Kurisu.Framework.Events
{
    public class EventSystem : MonoEventCoordinator
    {
#pragma warning disable IDE1006
        private sealed class _CallbackEventHandler : CallbackEventHandler, IBehaviourScope
#pragma warning restore IDE1006
        {
            public sealed override bool IsCompositeRoot => true;
            private readonly EventSystem eventCoordinator;
            public sealed override IEventCoordinator Root => eventCoordinator;
            public MonoBehaviour Behaviour { get; }
            public _CallbackEventHandler(EventSystem eventCoordinator)
            {
                Behaviour = eventCoordinator;
                this.eventCoordinator = eventCoordinator;
            }
            public override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                eventCoordinator.Dispatch(e, dispatchMode, MonoDispatchType.Update);
            }
            public void SendEvent(EventBase e, DispatchMode dispatchMode, MonoDispatchType monoDispatchType)
            {
                e.Target = this;
                eventCoordinator.Dispatch(e, dispatchMode, monoDispatchType);
            }
        }
        private static EventSystem instance;
        public static EventSystem Instance => instance != null ? instance : GetInstance();
        private CallbackEventHandler eventHandler;
        /// <summary>
        /// Get event system <see cref="CallbackEventHandler"/>
        /// </summary>
        public static CallbackEventHandler EventHandler => Instance.eventHandler;
        private static EventSystem GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif
            if (instance == null)
            {
                GameObject managerObject = new() { name = nameof(EventSystem) };
                instance = managerObject.AddComponent<EventSystem>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
        protected override void Awake()
        {
            base.Awake();
            eventHandler = new _CallbackEventHandler(this);
        }
        /// <summary>
        /// Adds an event handler to the instance. If the event handler has already been registered for the same phase (either TrickleDown or BubbleUp) then this method has no effect.
        /// </summary>
        /// <param name="callback">The event handler to add.</param>
        public static void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            EventHandler.RegisterCallback(callback, useTrickleDown);
        }
        /// <summary>
        /// Remove callback from the instance.
        /// </summary>
        /// <param name="callback">The callback to remove. If this callback was never registered, nothing happens.</param>
        /// <param name="useTrickleDown">Set this parameter to true to remove the callback from the TrickleDown phase. Set this parameter to false to remove the callback from the BubbleUp phase.</param>
        public static void UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            if (!instance) return;
            EventHandler.UnregisterCallback(callback, useTrickleDown);
        }
        /// <summary>
        /// Sends an event to the event handler.
        /// </summary>
        /// <param name="e">The event to send.</param>
        /// <param name="dispatchMode">The event dispatch mode.</param>
        /// <param name="monoDispatchType"></param>
        public static void SendEvent(EventBase eventBase, DispatchMode dispatchMode = DispatchMode.Default, MonoDispatchType monoDispatchType = MonoDispatchType.Update)
        {
            (EventHandler as _CallbackEventHandler).SendEvent(eventBase, dispatchMode, monoDispatchType);
        }

        public sealed override CallbackEventHandler GetCallbackEventHandler()
        {
            return eventHandler;
        }
    }
}