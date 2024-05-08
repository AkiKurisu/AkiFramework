using System;
namespace Kurisu.Framework
{
    public interface IAkiEvent { }
    public interface IAkiEvent<T> : IAkiEvent where T : Delegate
    {
        void Register(T onEvent);
        void UnRegister(T onEvent);
    }

    public class AkiEvent : IAkiEvent<Action>
    {
        private Action mOnEvent = () => { };

        public void Register(Action onEvent)
        {
            mOnEvent += onEvent;
        }

        public void UnRegister(Action onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger()
        {
            mOnEvent?.Invoke();
        }
    }
    public class AkiEvent<T> : IAkiEvent<Action<T>>
    {
        private Action<T> mOnEvent = e => { };

        public void Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void UnRegister(Action<T> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t)
        {
            mOnEvent?.Invoke(t);
        }
    }

    public class AkiEvent<T, K> : IAkiEvent<Action<T, K>>
    {
        private Action<T, K> mOnEvent = (t, k) => { };

        public void Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void UnRegister(Action<T, K> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k)
        {
            mOnEvent?.Invoke(t, k);
        }
    }

    public class AkiEvent<T, K, S> : IAkiEvent<Action<T, K, S>>
    {
        private Action<T, K, S> mOnEvent = (t, k, s) => { };

        public void Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
        }

        public void UnRegister(Action<T, K, S> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k, S s)
        {
            mOnEvent?.Invoke(t, k, s);
        }
    }
}