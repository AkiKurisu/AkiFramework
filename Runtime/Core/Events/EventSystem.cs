using UnityEngine;
namespace Kurisu.Framework.Events
{
    public class EventSystem : MonoEventCoordinator
    {
        private class GlobalCallbackEventHandler : BehaviourCallbackEventHandler
        {
            public GlobalCallbackEventHandler(MonoEventCoordinator eventCoordinator) : base(eventCoordinator)
            {
                EventCoordinator = eventCoordinator;
            }
            public MonoEventCoordinator EventCoordinator { get; }
            public sealed override void SendEvent(EventBase e)
            {
                e.Target = this;
                EventCoordinator.Dispatcher.Dispatch(e, EventCoordinator, DispatchMode.Default);
                EventCoordinator.Refresh();
            }

            public sealed override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                EventCoordinator.Dispatcher.Dispatch(e, EventCoordinator, dispatchMode);
                EventCoordinator.Refresh();
            }
        }
        private static EventSystem instance;
        public static EventSystem Instance => instance != null ? instance : GetInstance();
        public CallbackEventHandler EventHandler { get; private set; }
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
            EventHandler = new GlobalCallbackEventHandler(this);
        }
    }
}
