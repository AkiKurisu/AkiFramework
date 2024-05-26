# React

A simpler rx solution for my framework, simplified from `UniRx`.

## Extensions

### React for Events

React style Event subscription.

```C#

public class ReactEventExample : MonoBehaviour
{
    private void Awake()
    {
        EventSystem.EventHandler.AsObservable<MyCustomEvent>().Subscribe((e) =>
        {
            Debug.Log(e.Message);
            using var ce2 = MyCustom2Event.GetPooled("World");
            EventSystem.EventHandler.SendEvent(ce2);
        }).AddTo(gameObject);
        EventSystem.EventHandler.AsObservable<MyCustom2Event>().Subscribe((e) =>
        {
            Debug.Log(e.Message);
        }).AddTo(gameObject);
    }
    private void Start()
    {
        using var ce1 = MyCustomEvent.GetPooled("Hello");
        EventSystem.EventHandler.SendEvent(ce1);
    }
}
```


### React for ReactiveProperty

```C#
public class ReactiveValueExample : MonoBehaviour
{
    private readonly ReactiveVector3 reactiveVector = new(default);
    private readonly ReactiveBool reactiveBool = new(default);
    private void Awake()
    {
        reactiveVector.Subscribe(e => Debug.Log($"Vector: {e.PreviousValue} => {e.NewValue}", gameObject))
                    .AddTo(gameObject);
        reactiveBool.Subscribe(e => Debug.Log($"Bool: {e.PreviousValue} => {e.NewValue}", gameObject))
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