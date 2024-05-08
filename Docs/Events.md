# Events

Support dynamic and contextual event handling, ported from `UnityEngine.UIElements`.

### Event System

Event System is an implementation for global event purpose.

```C#
// Interface for your custom event
// Debugger will notify it and group them
public interface ICustomEvent { }

public class MyCustomEvent : EventBase<MyCustomEvent>, ICustomEvent
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

public class MyCustom2Event : EventBase<MyCustom2Event>, ICustomEvent
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

public class EventSystemExample : MonoBehaviour
{
    private void Awake()
    {
        EventSystem.Instance.EventHandler.Subscribe<MyCustomEvent>((e) =>
        {
            Debug.Log(e.Message);
            using var ce2 = MyCustom2Event.GetPooled("World");
            EventSystem.Instance.EventHandler.SendEvent(ce2);
        }).AddTo(gameObject);
        EventSystem.Instance.EventHandler.Subscribe<MyCustom2Event>((e) =>
        {
            Debug.Log(e.Message);
        }).AddTo(gameObject);
    }
    private void Start()
    {
        using var ce1 = MyCustomEvent.GetPooled("Hello");
        EventSystem.Instance.EventHandler.SendEvent(ce1);
    }
}
```
### Debugger

Events can be tracked in a debugger `Windows/AkiFramework/Event Debugger`.

![Debugger](./Images/debugger.png)


### Record

Record events state and resend to target event handler.

Recommend to install `jillejr.newtonsoft.json-for-unity.converters` to solve serialization problem with `Newtonsoft.Json`.

### React

```C#
public class ReactiveValueExample : MonoBehaviour
{
    private readonly ReactiveVector3 reactiveVector = new(default);
    private readonly ReactiveBool reactiveBool = new(default);
    private void Awake()
    {
        reactiveVector.SubscribeValueChange(e => Debug.Log($"Vector: {e.PreviousValue} => {e.NewValue}", gameObject))
                    .AddTo(gameObject);
        reactiveBool.SubscribeValueChange(e => Debug.Log($"Bool: {e.PreviousValue} => {e.NewValue}", gameObject))
                    .AddTo(gameObject);
    }
    private void Start()
    {        
        // Will send event
        reactiveVector.Value += Vector3.one;
        reactiveBool.Value = !reactiveBool.Value;
        // Not send event
        reactiveBool.SetValueWithoutNotify(false);
    }
}
```