using System;
namespace Kurisu.Framework.React
{
    public abstract class OperatorObservable<T> : IObservable<T>
    {
        public IDisposable Subscribe(Action<T> observer)
        {
            var subscription = new SingleAssignmentDisposable();
            subscription.Disposable = SubscribeCore(observer, subscription);
            return subscription;
        }

        protected abstract IDisposable SubscribeCore(Action<T> observer, IDisposable cancel);
    }
    public abstract class OperatorObserver<TSource, TResult> : IDisposable
    {
        protected internal readonly Action<TResult> observer;
        private readonly IDisposable cancel;
        public OperatorObserver(Action<TResult> observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }
        public abstract void OnNext(TSource value);
        public void Dispose()
        {
            cancel?.Dispose();
        }
    }
}