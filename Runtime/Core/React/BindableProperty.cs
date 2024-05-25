using System;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutNotify(T newValue);
    }

    public interface IReadonlyBindableProperty<T> : IObservable<T>
    {
        T Value { get; }
        /// <summary>
        /// Subscribe value change event and immediately notify observer
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        IDisposable SubscribeWithInitValue(Action<T> action);
        void Register(Action<T> observer);
        void Unregister(Action<T> observer);
    }

    public class BindableProperty<T> : AkiEvent<T>, IBindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            mValue = defaultValue;
        }
        [SerializeField]
        protected T mValue;
        public T Value
        {
            get => mValue;
            set
            {
                if (value == null && mValue == null) return;
                mValue = value;
                OnValueChanged(value);
                mEvent?.Invoke(value);
            }
        }
        protected virtual void OnValueChanged(T newValue) { }

        public void SetValueWithoutNotify(T newValue)
        {
            mValue = newValue;
        }
        public void Notify()
        {
            mEvent?.Invoke(mValue);
        }
        public void Release()
        {
            mValue = default;
            mEvent = null;
        }

        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
        public IDisposable SubscribeWithInitValue(Action<T> action)
        {
            var disposable = Subscribe(action);
            action?.Invoke(mValue);
            return disposable;
        }
    }
}