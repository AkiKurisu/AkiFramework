using Kurisu.Framework.Events;
using UnityEngine;
namespace Kurisu.Framework.Example
{
    public class EventCallbackExample : MonoBehaviour
    {
        private class MyCustomEvent : EventBase<MyCustomEvent>
        {
            public string Message { get; private set; }
            protected override void Init()
            {
                base.Init();
                Message = string.Empty;
            }
            public static MyCustomEvent GetPooled(string message)
            {
                var ce = GetPooled();
                ce.Message = message;
                return ce;
            }
        }
        private class MyCustom2Event : EventBase<MyCustom2Event>
        {
            public string Message { get; private set; }
            protected override void Init()
            {
                base.Init();
                Message = string.Empty;
            }
            public static MyCustom2Event GetPooled(string message)
            {
                var ce = GetPooled();
                ce.Message = message;
                return ce;
            }
        }
        private void Awake()
        {
            EventSystem.Instance.EventHandler.RegisterCallback<MyCustomEvent>((e) =>
            {
                Debug.Log(e.Message);
                var ce2 = MyCustom2Event.GetPooled("World");
                EventSystem.Instance.EventHandler.SendEvent(ce2);
            });
            EventSystem.Instance.EventHandler.RegisterCallback<MyCustom2Event>((e) =>
            {
                Debug.Log(e.Message);
            });
        }
        private void Start()
        {
            var ce1 = MyCustomEvent.GetPooled("Hello");
            EventSystem.Instance.EventHandler.SendEvent(ce1);
        }
    }
}
