using System;
namespace Kurisu.Framework.React
{
    internal class CreateObservable<T> : IObservable<T>
    {
        private readonly Func<Action<T>, IDisposable> subscribe;
        public CreateObservable(Func<Action<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            return subscribe(observer);
        }
    }
    internal class CreateObservable<T, TState> : IObservable<T>
    {
        private readonly TState state;
        private readonly Func<TState, Action<T>, IDisposable> subscribe;

        public CreateObservable(TState state, Func<TState, Action<T>, IDisposable> subscribe)
        {
            this.state = state;
            this.subscribe = subscribe;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            return subscribe(state, observer);
        }
    }
}