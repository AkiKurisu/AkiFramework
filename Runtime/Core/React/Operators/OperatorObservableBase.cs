using System;
namespace Kurisu.Framework.React
{
    public abstract class OperatorObservableBase<T> : IObservable<T>
    {
        public IDisposable Subscribe(Action<T> observer)
        {
            var subscription = new SingleAssignmentDisposable();
            subscription.Disposable = SubscribeCore(observer, subscription);
            return subscription;
        }

        protected abstract IDisposable SubscribeCore(Action<T> observer, IDisposable cancel);
    }
}