using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Events
{
    public interface IReadonlyReactiveProperty<T>
    {
        T Value { get; }
        void UnregisterValueChangeCallback(EventCallback<ChangeEvent<T>> onValueChanged);
        void RegisterValueChangeCallback(EventCallback<ChangeEvent<T>> onValueChanged);
    }
    /// <summary>
    /// Property broker using AkiFramework's Events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReactiveProperty<T> : CallbackEventHandler, INotifyValueChanged<T>, IReadonlyReactiveProperty<T>, IBehaviourScope
    {
        protected T _value;
        /// <summary>
        /// Constructor with defining its owner behaviour
        /// </summary>
        /// <param name="initValue"></param>
        /// <param name="attachedBehaviour"></param>
        public ReactiveProperty(T initValue, MonoBehaviour attachedBehaviour)
        {
            AttachBehaviour(attachedBehaviour);
            _value = initValue;
        }
        /// <summary>
        /// Constructor with anonymous owner 
        /// </summary>
        /// <param name="initValue"></param>
        /// <param name="attachedBehaviour"></param>
        public ReactiveProperty(T initValue)
        {
            _value = initValue;
        }
        /// <summary>
        /// Constructor with anonymous owner and default value
        /// </summary>
        public ReactiveProperty()
        {
            _value = default;
        }
        public virtual T Value
        {
            get => _value;
            set
            {
                SetValue(value);
            }
        }
        private MonoBehaviour attachedBehaviour;
        [JsonIgnore]
        public MonoBehaviour AttachedBehaviour
        {
            get
            {
                if (attachedBehaviour == null) return attachedBehaviour = EventSystem.Instance;
                return attachedBehaviour;
            }
            set
            {
                attachedBehaviour = value;
            }
        }
        private MonoEventCoordinator attachedCoordinator;
        [JsonIgnore]
        public MonoEventCoordinator AttachedCoordinator
        {
            get
            {
                if (attachedCoordinator == null) return attachedCoordinator = EventSystem.Instance;
                return attachedCoordinator;
            }
            set
            {
                attachedCoordinator = value;
            }
        }
        public sealed override IEventCoordinator Root => AttachedCoordinator;
        /// <summary>
        /// Whether to call <see cref="EventBase.StopPropagation"/> when attached behaviour is inactive or disable, default is true.
        /// </summary>
        /// <value></value>
        public bool StopPropagationWhenDisabled { get; set; } = true;
        /// <summary>
        /// Specify during which phases the event handler is executed.
        /// </summary>
        /// <value></value>
        public TrickleDown TrickleDown { get; set; } = TrickleDown.NoTrickleDown;
        /// <summary>
        /// When the change event is executed
        /// </summary>
        /// <value></value>
        public MonoDispatchType MonoDispatchType { get; set; }
        public override void SendEvent(EventBase e)
        {
            e.Target = this;
            if (StopPropagationWhenDisabled && !AttachedBehaviour.isActiveAndEnabled) e.StopPropagation();
            AttachedCoordinator.Dispatch(e, DispatchMode.Default, MonoDispatchType);
        }
        public override void SendEvent(EventBase e, DispatchMode dispatchMode)
        {
            e.Target = this;
            if (StopPropagationWhenDisabled && !AttachedBehaviour.isActiveAndEnabled) e.StopPropagation();
            AttachedCoordinator.Dispatch(e, dispatchMode, MonoDispatchType);
        }
        public virtual void SetValue(T newValue)
        {
            if (_value.Equals(newValue)) return;
            T previewsValue = _value;
            _value = newValue;
            using var evt = ChangeEvent<T>.GetPooled(previewsValue, newValue);
            SendEvent(evt);
        }
        public virtual void SetValueWithoutNotify(T newValue)
        {
            if (_value.Equals(newValue)) return;
            _value = newValue;
        }

        public void UnregisterValueChangeCallback(EventCallback<ChangeEvent<T>> onValueChanged)
        {
            UnregisterCallback(onValueChanged, TrickleDown);
        }
        public void RegisterValueChangeCallback(EventCallback<ChangeEvent<T>> onValueChanged)
        {
            RegisterCallback(onValueChanged, TrickleDown);
        }
        /// <summary>
        /// Attach this reactive value to a behaviour
        /// </summary>
        /// <param name="behaviour"></param>
        public void AttachBehaviour(MonoBehaviour behaviour)
        {
            AttachedBehaviour = behaviour;
            AttachedCoordinator = behaviour as MonoEventCoordinator;
        }
    }
    public class ReactiveBool : ReactiveProperty<bool>
    {
        public ReactiveBool() : base() { }
        public ReactiveBool(bool initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveBool(bool initValue) : base(initValue)
        {
        }
    }
    public class ReactiveInt : ReactiveProperty<int>
    {
        public ReactiveInt() : base() { }
        public ReactiveInt(int initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveInt(int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveUint : ReactiveProperty<uint>
    {
        public ReactiveUint() : base() { }
        public ReactiveUint(uint initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveUint(uint initValue) : base(initValue)
        {
        }
    }
    public class ReactiveDouble : ReactiveProperty<double>
    {
        public ReactiveDouble() : base() { }
        public ReactiveDouble(double initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveDouble(double initValue) : base(initValue)
        {
        }
    }
    public class ReactiveLong : ReactiveProperty<long>
    {
        public ReactiveLong() : base() { }
        public ReactiveLong(long initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveLong(long initValue) : base(initValue)
        {
        }
    }
    public class ReactiveFloat : ReactiveProperty<float>
    {
        public ReactiveFloat() : base() { }
        public ReactiveFloat(float initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveFloat(float initValue) : base(initValue)
        {
        }
    }
    public class ReactiveString : ReactiveProperty<string>
    {
        public ReactiveString() : base() { }
        public ReactiveString(string initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveString(string initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector2 : ReactiveProperty<Vector2>
    {
        public ReactiveVector2() : base() { }
        public ReactiveVector2(Vector2 initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector2(Vector2 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector3 : ReactiveProperty<Vector3>
    {
        public ReactiveVector3() : base() { }
        public ReactiveVector3(Vector3 initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector3(Vector3 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector2Int : ReactiveProperty<Vector2Int>
    {
        public ReactiveVector2Int() : base() { }
        public ReactiveVector2Int(Vector2Int initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector2Int(Vector2Int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector3Int : ReactiveProperty<Vector3Int>
    {
        public ReactiveVector3Int() : base() { }
        public ReactiveVector3Int(Vector3Int initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector3Int(Vector3Int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveColor32 : ReactiveProperty<Color32>
    {
        public ReactiveColor32() : base() { }
        public ReactiveColor32(Color32 initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveColor32(Color32 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveObject : ReactiveProperty<Object>
    {
        public ReactiveObject() : base() { }
        public ReactiveObject(Object initValue, MonoBehaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveObject(Object initValue) : base(initValue)
        {
        }
    }
}