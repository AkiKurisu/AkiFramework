using System.Collections.Generic;
using UnityEngine;
using static Kurisu.Framework.Events.EventBase;
namespace Kurisu.Framework.Events
{
    public class EventSystem : MonoEventCoordinator
    {
        private sealed class RootCallBackEventHandler : CallbackEventHandler, IBehaviourScope
        {
            public sealed override bool IsCompositeRoot => true;
            private readonly EventSystem eventCoordinator;
            public sealed override IEventCoordinator Root => eventCoordinator;

            public MonoBehaviour AttachedBehaviour { get; }

            public RootCallBackEventHandler(EventSystem eventCoordinator)
            {
                AttachedBehaviour = eventCoordinator;
                this.eventCoordinator = eventCoordinator;
            }
            public override void SendEvent(EventBase e)
            {
                e.Target = this;
                eventCoordinator.Dispatch(e, DispatchMode.Default, MonoDispatchType.Update);
            }
            public override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                eventCoordinator.Dispatch(e, dispatchMode, MonoDispatchType.Update);
            }
        }
        private class MonoCallBackEventHandler : CallbackEventHandler, IBehaviourScope
        {
            public sealed override bool IsCompositeRoot => false;
            protected readonly MonoDispatchType monoDispatchType;
            protected readonly MonoEventCoordinator eventCoordinator;
            public sealed override IEventCoordinator Root => eventCoordinator;
            public MonoBehaviour AttachedBehaviour { get; }
            public MonoCallBackEventHandler(MonoEventCoordinator eventCoordinator, MonoDispatchType monoDispatchType, CallbackEventHandler parent)
            {
                AttachedBehaviour = eventCoordinator;
                this.monoDispatchType = monoDispatchType;
                this.eventCoordinator = eventCoordinator;
                Parent = parent;
            }

            public override void SendEvent(EventBase e)
            {
                e.Target = this;
                e.Propagation |= EventPropagation.Bubbles;
                eventCoordinator.Dispatch(e, DispatchMode.Default, monoDispatchType);
            }
            public override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                e.Propagation |= EventPropagation.Bubbles;
                eventCoordinator.Dispatch(e, dispatchMode, monoDispatchType);
            }
        }
        private static EventSystem instance;
        public static EventSystem Instance => instance != null ? instance : GetInstance();
        private readonly Dictionary<MonoDispatchType, CallbackEventHandler> eventHandlers = new();
        public override CallbackEventHandler RootEventHandler { get; protected set; }
        /// <summary>
        /// Get <see cref="CallbackEventHandler"/> for events dispatch on all frame (Update, FixedUpdate and LateUpdate), use <see cref="TrickleDown"/> to register calBack on children handlers.
        /// </summary>
        /// <remarks>
        /// Events send from this can not be received by children, to receive specified dispatch type events, sending of events should also use specified eventHandler.
        /// </remarks>
        public static CallbackEventHandler EventHandler => Instance.RootEventHandler;
        /// <summary>
        /// Get <see cref="CallbackEventHandler"/> for events dispatch on update
        /// </summary>
        /// <returns></returns>
        public static CallbackEventHandler UpdateHandler => Instance.GetEventHandler(MonoDispatchType.Update);
        /// <summary>
        /// Get <see cref="CallbackEventHandler"/> for events dispatch on fixedUpdate
        /// </summary>
        /// <value></value>
        public static CallbackEventHandler FixedUpdateHandler => Instance.GetEventHandler(MonoDispatchType.FixedUpdate);
        /// <summary>
        /// Get <see cref="CallbackEventHandler"/> for events dispatch on lateUpdate
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
            var root = new RootCallBackEventHandler(this);
            RootEventHandler = root;
            //Set parent to bubble up
            eventHandlers[MonoDispatchType.Update] = new MonoCallBackEventHandler(this, MonoDispatchType.Update, RootEventHandler);
            eventHandlers[MonoDispatchType.FixedUpdate] = new MonoCallBackEventHandler(this, MonoDispatchType.FixedUpdate, RootEventHandler);
            eventHandlers[MonoDispatchType.LateUpdate] = new MonoCallBackEventHandler(this, MonoDispatchType.LateUpdate, RootEventHandler);
        }
        public CallbackEventHandler GetEventHandler(MonoDispatchType monoDispatchType)
        {
            return eventHandlers[monoDispatchType];
        }
    }
}
