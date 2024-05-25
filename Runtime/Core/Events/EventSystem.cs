using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Events
{
    public class EventSystem : MonoEventCoordinator
    {
        private class MonoEventHandler : BehaviourCallbackEventHandler
        {
            private readonly MonoDispatchType monoDispatchType;
            public MonoEventHandler(MonoEventCoordinator eventCoordinator, MonoDispatchType monoDispatchType) : base(eventCoordinator)
            {
                this.monoDispatchType = monoDispatchType;
                EventCoordinator = eventCoordinator;
            }
            public MonoEventCoordinator EventCoordinator { get; }
            public sealed override void SendEvent(EventBase e)
            {
                e.Target = this;
                EventCoordinator.Dispatch(e, DispatchMode.Default, monoDispatchType);
            }
            public sealed override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                EventCoordinator.Dispatch(e, dispatchMode, monoDispatchType);
            }
        }
        private static EventSystem instance;
        public static EventSystem Instance => instance != null ? instance : GetInstance();
        private readonly Dictionary<MonoDispatchType, CallbackEventHandler> eventHandlers = new();
        /// <summary>
        /// Default callback handler, equal to <see cref="UpdateHandler"/>
        /// </summary>
        /// <value></value>
        public static CallbackEventHandler EventHandler => UpdateHandler;
        /// <summary>
        /// Callback handler for events execute on update
        /// </summary>
        /// <returns></returns>
        public static CallbackEventHandler UpdateHandler => Instance.GetEventHandler(MonoDispatchType.Update);
        /// <summary>
        /// Callback handler for events execute on fixedUpdate
        /// </summary>
        /// <value></value>
        public static CallbackEventHandler FixedUpdateHandler => Instance.GetEventHandler(MonoDispatchType.FixedUpdate);
        /// <summary>
        /// Callback handler for events execute on lateUpdate
        /// </summary>
        /// <value></value>
        public static CallbackEventHandler LateUpdateHandler => Instance.GetEventHandler(MonoDispatchType.LateUpdate);
        private static EventSystem GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif
            if (instance == null)
            {
                EventSystem managerInScene = FindObjectOfType<EventSystem>();
                if (managerInScene != null)
                {
                    instance = managerInScene;
                }
                else
                {
                    GameObject managerObject = new() { name = nameof(EventSystem) };
                    instance = managerObject.AddComponent<EventSystem>();
                }
            }
            return instance;
        }
        protected override void Awake()
        {
            base.Awake();
            eventHandlers[MonoDispatchType.Update] = new MonoEventHandler(this, MonoDispatchType.Update);
            eventHandlers[MonoDispatchType.FixedUpdate] = new MonoEventHandler(this, MonoDispatchType.FixedUpdate);
            eventHandlers[MonoDispatchType.LateUpdate] = new MonoEventHandler(this, MonoDispatchType.LateUpdate);
        }
        public CallbackEventHandler GetEventHandler(MonoDispatchType monoDispatchType)
        {
            return eventHandlers[monoDispatchType];
        }
    }
}
