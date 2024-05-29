using System;
namespace Kurisu.Framework.React
{
    internal class FromEventObservable<TDelegate> : IObservable<Unit>
    {
        private readonly Func<Action, TDelegate> conversion;
        private readonly Action<TDelegate> addHandler;
        private readonly Action<TDelegate> removeHandler;

        public FromEventObservable(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        {
            this.conversion = conversion;
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IDisposable Subscribe(Action<Unit> observer)
        {
            var fe = new FromEvent(this, observer);
            return fe.Register() ? fe : Disposable.Empty;
        }

        private class FromEvent : IDisposable
        {
            readonly FromEventObservable<TDelegate> parent;
            readonly Action<Unit> observer;
            TDelegate handler;

            public FromEvent(FromEventObservable<TDelegate> parent, Action<Unit> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }
            public bool Register()
            {
                handler = parent.conversion(OnNext);
                try
                {
                    parent.addHandler(handler);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            private void OnNext()
            {
                observer(Unit.Default);
            }

            public void Dispose()
            {
                if (handler != null)
                {
                    parent.removeHandler(handler);
                    handler = default;
                }
            }
        }
    }
    internal class FromEventObservable<TDelegate, TEventArgs> : IObservable<TEventArgs>
    {
        readonly Func<Action<TEventArgs>, TDelegate> conversion;
        readonly Action<TDelegate> addHandler;
        readonly Action<TDelegate> removeHandler;

        public FromEventObservable(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        {
            this.conversion = conversion;
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IDisposable Subscribe(Action<TEventArgs> observer)
        {
            var fe = new FromEvent(this, observer);
            return fe.Register() ? fe : Disposable.Empty;
        }

        private class FromEvent : IDisposable
        {
            private readonly FromEventObservable<TDelegate, TEventArgs> parent;
            private TDelegate handler;
            private readonly Action<TEventArgs> observer;
            public FromEvent(FromEventObservable<TDelegate, TEventArgs> parent, Action<TEventArgs> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }
            public bool Register()
            {
                handler = parent.conversion(OnNext);
                try
                {
                    parent.addHandler(handler);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            private void OnNext(TEventArgs args)
            {
                observer(args);
            }
            public void Dispose()
            {
                if (handler != null)
                {
                    parent.removeHandler(handler);
                    handler = default;
                }
            }
        }
    }

    internal class FromEventObservable : IObservable<Unit>
    {
        private readonly Action<Action> addHandler;
        private readonly Action<Action> removeHandler;

        public FromEventObservable(Action<Action> addHandler, Action<Action> removeHandler)
        {
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IDisposable Subscribe(Action<Unit> observer)
        {
            var fe = new FromEvent(this, observer);
            return fe.Register() ? fe : Disposable.Empty;
        }

        private class FromEvent : IDisposable
        {
            private readonly FromEventObservable parent;
            private readonly Action<Unit> observer;
            private Action handler;

            public FromEvent(FromEventObservable parent, Action<Unit> observer)
            {
                this.parent = parent;
                this.observer = observer;
                handler = OnNext;
            }
            public bool Register()
            {
                try
                {
                    parent.addHandler(handler);
                }
                catch
                {
                    return false;
                }
                return true;
            }

            private void OnNext()
            {
                observer(Unit.Default);
            }
            public void Dispose()
            {
                if (handler != null)
                {
                    parent.removeHandler(handler);
                    handler = null;
                }
            }
        }
    }
}