using UnityEngine;
namespace Kurisu.Framework.Events
{
    public class EventSystem : MonoBehaviour
    {
        private class GlobalCallbackEventHandler : CallbackEventHandler, IEventCoordinator
        {
            public GlobalCallbackEventHandler(EventDispatcher dispatcher)
            {
                Dispatcher = dispatcher;
            }
            public EventDispatcher Dispatcher { get; }
            public sealed override void SendEvent(EventBase e)
            {
                e.Target = this;
                Dispatcher.Dispatch(e, this, DispatchMode.Queued);
            }
            public sealed override void SendEvent(EventBase e, DispatchMode dispatchMode)
            {
                e.Target = this;
                Dispatcher.Dispatch(e, this, dispatchMode);
            }
        }
        private static EventSystem instance;
        public static EventSystem Instance => instance != null ? instance : GetInstance();
        private EventDispatcher dispatcher;
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
        private void Awake()
        {
            dispatcher = EventDispatcher.CreateDefault();
            EventHandler = new GlobalCallbackEventHandler(dispatcher);
        }
        private void Update()
        {
            dispatcher.PushDispatcherContext();
            dispatcher.PopDispatcherContext();
        }
    }
}
