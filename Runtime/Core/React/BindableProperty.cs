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

    public class BindableProperty<T> : AkiEvent<T>, IBindableProperty<T>, IDisposable
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
                SetValue(value);
            }
        }
        protected virtual void OnValueChanged(T newValue) { }
        public void SetValue(T newValue)
        {
            if (newValue == null && mValue == null) return;
            mValue = newValue;
            OnValueChanged(newValue);
            mEvent?.Invoke(newValue);
        }
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
            action(mValue);
            return disposable;
        }

        public virtual void Dispose()
        {
            Release();
        }
    }
}