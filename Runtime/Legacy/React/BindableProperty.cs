using System;
namespace Kurisu.Framework
{
    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutNotify(T newValue);
    }

    public interface IReadonlyBindableProperty<T> : IAkiEvent<Action<T>>
    {
        T Value { get; }
        void RegisterWithInitValue(Action<T> action);
    }

    public class BindableProperty<T> : IBindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            mValue = defaultValue;
        }

        protected T mValue;
        private Action<T> mOnValueChanged = (v) => { };
        public T Value
        {
            get => GetValue();
            set
            {
                if (value == null && mValue == null) return;
                // if (value != null && value.Equals(mValue)) return;

                SetValue(value);
                mOnValueChanged?.Invoke(value);
            }
        }

        protected virtual void SetValue(T newValue)
        {
            mValue = newValue;
        }

        protected virtual T GetValue()
        {
            return mValue;
        }

        public void SetValueWithoutNotify(T newValue)
        {
            mValue = newValue;
        }
        public void Notify()
        {
            mOnValueChanged?.Invoke(mValue);
        }
        public void Release()
        {
            mValue = default;
            mOnValueChanged = null;
        }
        public void Register(Action<T> onValueChanged)
        {
            mOnValueChanged += onValueChanged;
        }

        public void RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(mValue);
        }

        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            mOnValueChanged -= onValueChanged;
        }
    }
}