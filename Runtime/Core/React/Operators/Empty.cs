using System;
namespace Kurisu.Framework.React
{
    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Singleton = new();

        private EmptyDisposable()
        {

        }

        public void Dispose()
        {
        }
    }
    internal class EmptyObservable<T> : IObservable<T>
    {
        public IDisposable Subscribe(Action<T> observer)
        {
            return EmptyDisposable.Singleton;
        }
    }
}