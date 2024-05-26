using System;
namespace Kurisu.Framework.React
{
    internal class TakeObservable<T> : OperatorObservableBase<T>
    {
        private readonly IObservable<T> source;
        private readonly int count;
        public TakeObservable(IObservable<T> source, int count)
        {
            this.source = source;
            this.count = count;
        }

        // optimize combiner

        public IObservable<T> Combine(int count)
        {
            // xs = 6
            // xs.Take(5) = 5         | xs.Take(3) = 3
            // xs.Take(5).Take(3) = 3 | xs.Take(3).Take(5) = 3

            // use minimum one
            return (this.count <= count)
                ? this
                : new TakeObservable<T>(source, count);
        }

        protected override IDisposable SubscribeCore(Action<T> observer, IDisposable disposable)
        {
            return source.Subscribe(new Take(this, observer, disposable).OnNext);
        }

        private class Take
        {
            private int rest;
            private readonly Action<T> observer;
            private readonly IDisposable cancelable;
            public Take(TakeObservable<T> parent, Action<T> observer, IDisposable cancelable)
            {
                rest = parent.count;
                this.observer = observer;
                this.cancelable = cancelable;
            }

            public void OnNext(T value)
            {
                if (rest > 0)
                {
                    rest -= 1;
                    observer(value);
                    if (rest == 0)
                    {
                        cancelable.Dispose();
                    }
                }
            }
        }

    }
}