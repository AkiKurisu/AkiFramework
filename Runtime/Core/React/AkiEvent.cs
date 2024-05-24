using System;
namespace Kurisu.Framework.React
{
    public abstract class AkiEventBase<T> : IObservable<T> where T : Delegate
    {
        public abstract void Register(T onEvent);
        public abstract void Unregister(T onEvent);
        public IDisposable Subscribe(T observer)
        {
            Register(observer);
            return Disposable.Create(() => Unregister(observer));
        }
        public IDisposable SubscribeOnce(T observer)
        {
            T combinedAction = (T)Delegate.Combine(observer, (Action)(() => Unregister(observer)));
            Register(combinedAction);
            return Disposable.Create(() => Unregister(combinedAction));
        }
    }
    public class AkiEvent : AkiEventBase<Action>
    {
        private Action mOnEvent = () => { };

        public override void Register(Action onEvent)
        {
            mOnEvent += onEvent;
        }

        public override void Unregister(Action onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger()
        {
            mOnEvent?.Invoke();
        }
    }
    public class AkiEvent<T> : AkiEventBase<Action<T>>
    {
        private Action<T> mOnEvent = e => { };

        public override void Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
        }

        public override void Unregister(Action<T> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t)
        {
            mOnEvent?.Invoke(t);
        }
    }
    public class AkiEvent<T, K> : AkiEventBase<Action<T, K>>
    {
        private Action<T, K> mOnEvent = (t, k) => { };

        public override void Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
        }

        public override void Unregister(Action<T, K> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k)
        {
            mOnEvent?.Invoke(t, k);
        }
    }
    public class AkiEvent<T, K, S> : AkiEventBase<Action<T, K, S>>
    {
        private Action<T, K, S> mOnEvent = (t, k, s) => { };

        public override void Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
        }

        public override void Unregister(Action<T, K, S> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k, S s)
        {
            mOnEvent?.Invoke(t, k, s);
        }
    }
}