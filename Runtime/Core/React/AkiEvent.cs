using System;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Event for AkiFramework's react
    /// </summary>
    public class AkiEvent<T> : IObservable<T>
    {
        protected Action<T> mEvent = (e) => { };
        public void Register(Action<T> observer)
        {
            mEvent += observer;
        }
        public void Unregister(Action<T> observer)
        {
            mEvent -= observer;
        }
        public IDisposable Subscribe(Action<T> observer)
        {
            Register(observer);
            return Disposable.Create(() => Unregister(observer));
        }
        public void Trigger(T t)
        {
            mEvent?.Invoke(t);
        }
    }
    /// <summary>
    /// Event for AkiFramework's react
    /// </summary>
    public class AkiEvent : AkiEvent<Unit>
    {
        public IDisposable Subscribe(Action observer)
        {
            Action<Unit> conversation = new((e) => observer());
            Register(conversation);
            return Disposable.Create(() => Unregister(conversation));
        }
        public void Trigger()
        {
            mEvent?.Invoke(Unit.Default);
        }
    }
    /// <summary>
    /// Event for AkiFramework's react
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class AkiEvent<T, K> : IObservable<Tuple<T, K>>
    {
        protected Action<Tuple<T, K>> mEvent = (e) => { };
        public IDisposable Subscribe(Action<Tuple<T, K>> observer)
        {
            Register(observer);
            return Disposable.Create(() => Unregister(observer));
        }
        public IDisposable Subscribe(Action<T, K> observer)
        {
            Action<Tuple<T, K>> conversation = new(t => observer(t.Item1, t.Item2));
            Register(conversation);
            return Disposable.Create(() => Unregister(conversation));
        }
        public void Register(Action<Tuple<T, K>> observer)
        {
            mEvent += observer;
        }
        public void Unregister(Action<Tuple<T, K>> observer)
        {
            mEvent -= observer;
        }
        public void Trigger(T t, K k)
        {
            mEvent?.Invoke(new(t, k));
        }
        public void Trigger(Tuple<T, K> item)
        {
            mEvent?.Invoke(item);
        }
    }
    /// <summary>
    /// Event for AkiFramework's react
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="S"></typeparam>
    public class AkiEvent<T, K, S> : IObservable<Tuple<T, K, S>>
    {
        protected Action<Tuple<T, K, S>> mEvent = (e) => { };
        public IDisposable Subscribe(Action<Tuple<T, K, S>> observer)
        {
            Register(observer);
            return Disposable.Create(() => Unregister(observer));
        }
        public IDisposable Subscribe(Action<T, K, S> observer)
        {
            Action<Tuple<T, K, S>> conversation = new(t => observer(t.Item1, t.Item2, t.Item3));
            Register(conversation);
            return Disposable.Create(() => Unregister(conversation));
        }
        public void Register(Action<Tuple<T, K, S>> observer)
        {
            mEvent += observer;
        }
        public void Unregister(Action<Tuple<T, K, S>> observer)
        {
            mEvent -= observer;
        }
        public void Trigger(T t, K k, S s)
        {
            mEvent?.Invoke(new(t, k, s));
        }
        public void Trigger(Tuple<T, K, S> item)
        {
            mEvent?.Invoke(item);
        }
    }
}