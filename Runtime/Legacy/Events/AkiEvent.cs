using System;
namespace Kurisu.Framework
{
    public interface IObservable<T> where T : Delegate
    {
        void Register(T onEvent);
        void Unregister(T onEvent);
    }
    /// <summary>
    /// A light-weight replacement of C# event
    /// </summary>
    public class AkiEvent : IObservable<Action>
    {
        private Action mOnEvent = () => { };

        public void Register(Action onEvent)
        {
            mOnEvent += onEvent;
        }

        public void Unregister(Action onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger()
        {
            mOnEvent?.Invoke();
        }
    }
    /// <summary>
    /// A light-weight replacement of C# event
    /// </summary>
    public class AkiEvent<T> : IObservable<Action<T>>
    {
        private Action<T> mOnEvent = e => { };

        public void Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void Unregister(Action<T> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t)
        {
            mOnEvent?.Invoke(t);
        }
    }
    /// <summary>
    /// A light-weight replacement of C# event
    /// </summary>
    public class AkiEvent<T, K> : IObservable<Action<T, K>>
    {
        private Action<T, K> mOnEvent = (t, k) => { };

        public void Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void Unregister(Action<T, K> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k)
        {
            mOnEvent?.Invoke(t, k);
        }
    }
    /// <summary>
    /// A light-weight replacement of C# event
    /// </summary>
    public class AkiEvent<T, K, S> : IObservable<Action<T, K, S>>
    {
        private Action<T, K, S> mOnEvent = (t, k, s) => { };

        public void Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void Unregister(Action<T, K, S> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k, S s)
        {
            mOnEvent?.Invoke(t, k, s);
        }
    }
}