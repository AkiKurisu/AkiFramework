using Kurisu.Framework.Events;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;
namespace Kurisu.Framework.React
{
    public interface IReadonlyReactiveValue<T>
    {
        T Value { get; }
        void UnregisterCallback(EventCallback<ChangeEvent<T>> onValueChanged);
        void RegisterCallback(EventCallback<ChangeEvent<T>> onValueChanged);
    }
    public abstract class ReactiveValue<T> : CallbackEventHandler, INotifyValueChanged<T>, IReadonlyReactiveValue<T>, IBehaviourScope
    {
        protected T _value;
        /// <summary>
        /// Since event system are initialized by MonoBehaviour lifetime scope, ReactiveValue should also be constructed in Awake() or Start()
        /// </summary>
        private void ConstructorSafeCheck()
        {
#if !REACT_DISABLE_SAFE_CHECK
            try
            {
                bool constructValid = Application.isPlaying;
            }
            catch
            {
                throw;
            }
#endif
        }
        /// <summary>
        /// Constructor with defining its owner behaviour
        /// </summary>
        /// <param name="initValue"></param>
        /// <param name="attachedBehaviour"></param>
        public ReactiveValue(T initValue, Behaviour attachedBehaviour)
        {
            ConstructorSafeCheck();
            AttachedBehaviour = attachedBehaviour;
            _value = initValue;
        }
        /// <summary>
        /// Constructor with anonymous owner 
        /// </summary>
        /// <param name="initValue"></param>
        /// <param name="attachedBehaviour"></param>
        public ReactiveValue(T initValue)
        {
            ConstructorSafeCheck();
            AttachedBehaviour = EventSystem.Instance;
            _value = initValue;
        }
        /// <summary>
        /// Constructor with anonymous owner and default value
        /// </summary>
        public ReactiveValue()
        {
            ConstructorSafeCheck();
            AttachedBehaviour = EventSystem.Instance;
            _value = default;
        }
        public virtual T Value
        {
            get => _value;
            set
            {
                if (!value.Equals(_value))
                {
                    T previewsValue = value;
                    _value = value;
                    SendEvent(ChangeEvent<T>.GetPooled(previewsValue, value));
                }
            }
        }
        [JsonIgnore]
        public Behaviour AttachedBehaviour { get; protected set; }
        /// <summary>
        /// Whether to call <see cref="EventBase.StopPropagation"/> when attached behaviour is inactive or disable, default is true.
        /// </summary>
        /// <value></value>
        public bool StopPropagationWhenDisabled { get; set; } = true;
        public override void SendEvent(EventBase e)
        {
            e.Target = this;
            if (StopPropagationWhenDisabled && !AttachedBehaviour.isActiveAndEnabled) e.StopPropagation();
            EventSystem.Instance.Dispatcher.Dispatch(e, EventSystem.Instance, DispatchMode.Default);
            EventSystem.Instance.Refresh();
            e.Dispose();
        }
        public override void SendEvent(EventBase e, DispatchMode dispatchMode)
        {
            e.Target = this;
            if (StopPropagationWhenDisabled && !AttachedBehaviour.isActiveAndEnabled) e.StopPropagation();
            EventSystem.Instance.Dispatcher.Dispatch(e, EventSystem.Instance, DispatchMode.Default);
            EventSystem.Instance.Refresh();
            e.Dispose();
        }

        public void SetValueWithoutNotify(T newValue)
        {
            _value = newValue;
        }

        public void UnregisterCallback(EventCallback<ChangeEvent<T>> onValueChanged)
        {
            base.UnregisterCallback(onValueChanged);
        }
        public void RegisterCallback(EventCallback<ChangeEvent<T>> onValueChanged)
        {
            base.RegisterCallback(onValueChanged);
        }
        public void AttachBehaviour(Behaviour behaviour)
        {
            AttachedBehaviour = behaviour != null ? behaviour : EventSystem.Instance;
        }
    }
    public class ReactiveBool : ReactiveValue<bool>
    {
        public ReactiveBool() : base() { }
        public ReactiveBool(bool initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveBool(bool initValue) : base(initValue)
        {
        }
    }
    public class ReactiveInt : ReactiveValue<int>
    {
        public ReactiveInt() : base() { }
        public ReactiveInt(int initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveInt(int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveUint : ReactiveValue<uint>
    {
        public ReactiveUint() : base() { }
        public ReactiveUint(uint initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveUint(uint initValue) : base(initValue)
        {
        }
    }
    public class ReactiveDouble : ReactiveValue<double>
    {
        public ReactiveDouble() : base() { }
        public ReactiveDouble(double initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveDouble(double initValue) : base(initValue)
        {
        }
    }
    public class ReactiveLong : ReactiveValue<long>
    {
        public ReactiveLong() : base() { }
        public ReactiveLong(long initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveLong(long initValue) : base(initValue)
        {
        }
    }
    public class ReactiveFloat : ReactiveValue<float>
    {
        public ReactiveFloat() : base() { }
        public ReactiveFloat(float initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveFloat(float initValue) : base(initValue)
        {
        }
    }
    public class ReactiveString : ReactiveValue<string>
    {
        public ReactiveString() : base() { }
        public ReactiveString(string initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveString(string initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector2 : ReactiveValue<Vector2>
    {
        [JsonConverter(typeof(VectorConverter))]
        public override Vector2 Value
        {
            get => base.Value;
            set => base.Value = value;
        }
        public ReactiveVector2() : base() { }
        public ReactiveVector2(Vector2 initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector2(Vector2 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector3 : ReactiveValue<Vector3>
    {
        [JsonConverter(typeof(VectorConverter))]
        public override Vector3 Value
        {
            get => base.Value;
            set => base.Value = value;
        }
        public ReactiveVector3() : base() { }
        public ReactiveVector3(Vector3 initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector3(Vector3 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector2Int : ReactiveValue<Vector2Int>
    {
        [JsonConverter(typeof(VectorConverter))]
        public override Vector2Int Value
        {
            get => base.Value;
            set => base.Value = value;
        }
        public ReactiveVector2Int() : base() { }
        public ReactiveVector2Int(Vector2Int initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector2Int(Vector2Int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveVector3Int : ReactiveValue<Vector3Int>
    {
        [JsonConverter(typeof(VectorConverter))]
        public override Vector3Int Value
        {
            get => base.Value;
            set => base.Value = value;
        }
        public ReactiveVector3Int() : base() { }
        public ReactiveVector3Int(Vector3Int initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveVector3Int(Vector3Int initValue) : base(initValue)
        {
        }
    }
    public class ReactiveColor32 : ReactiveValue<Color32>
    {
        public ReactiveColor32() : base() { }
        public ReactiveColor32(Color32 initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveColor32(Color32 initValue) : base(initValue)
        {
        }
    }
    public class ReactiveObject : ReactiveValue<Object>
    {
        public ReactiveObject() : base() { }
        public ReactiveObject(Object initValue, Behaviour attachedBehaviour) : base(initValue, attachedBehaviour)
        {
        }
        public ReactiveObject(Object initValue) : base(initValue)
        {
        }
    }
}