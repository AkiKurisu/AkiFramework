using System.Collections.Generic;
using System;
namespace Kurisu.Framework.React
{
    public static class Disposable
    {
        public static IDisposable Create(Action disposeAction)
        {
            return new AnonymousDisposable(disposeAction);
        }
    }
    /// <summary>
    /// Anonymous callBack implement of <see cref="IDisposable"/>, will invoke callback on dispose
    /// </summary>
    internal class AnonymousDisposable : IDisposable
    {
        private bool isDisposed;
        private readonly Action onDispose;
        public AnonymousDisposable(Action onDispose)
        {
            this.onDispose = onDispose;
        }
        public void Dispose()
        {
            if (!isDisposed)
            {
                onDispose();
                isDisposed = true;
            }
        }
    }
    /// <summary>
    /// Composite implement of <see cref="IDisposable"/> and <see cref="IUnRegister"/>, will dispose inner disposable children on dispose
    /// </summary>
    public class CompositeDisposable : IUnRegister, IDisposable
    {
        private readonly HashSet<IDisposable> disposables = new();

        public void Add(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void Remove(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
}