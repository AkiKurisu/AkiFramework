using System;
namespace Kurisu.Framework.React
{
    public static partial class Observable
    {
        public static IObservable<T> Create<T>(Func<Action<T>, IDisposable> subscribe) where T : Delegate
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateObservable<T>(subscribe);
        }
        public static IObservable<Unit> FromEvent<TDelegate>(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        {
            return new FromEventObservable<TDelegate>(conversion, addHandler, removeHandler);
        }

        public static IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        {
            return new FromEventObservable<TDelegate, TEventArgs>(conversion, addHandler, removeHandler);
        }

        public static IObservable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler)
        {
            return new FromEventObservable(addHandler, removeHandler);
        }
    }
    internal class CreateObservable<T> : IObservable<T>
    {
        readonly Func<Action<T>, IDisposable> subscribe;
        public CreateObservable(Func<Action<T>, IDisposable> subscribe)
        {
            this.subscribe = subscribe;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            return subscribe(observer);
        }
    }
    internal class FromEventObservable<TDelegate> : IObservable<Unit>
    {
        readonly Func<Action, TDelegate> conversion;
        readonly Action<TDelegate> addHandler;
        readonly Action<TDelegate> removeHandler;

        public FromEventObservable(Func<Action, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        {
            this.conversion = conversion;
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IDisposable Subscribe(Action<Unit> observer)
        {
            return new FromEvent(this, observer);
        }

        class FromEvent : IDisposable
        {
            readonly FromEventObservable<TDelegate> parent;
            readonly Action<Unit> observer;
            TDelegate handler;

            public FromEvent(FromEventObservable<TDelegate> parent, Action<Unit> observer)
            {
                this.parent = parent;
                this.observer = observer;
                handler = parent.conversion(OnNext);
                parent.addHandler(handler);
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
            return new FromEvent(this, observer);
        }

        class FromEvent : IDisposable
        {
            readonly FromEventObservable<TDelegate, TEventArgs> parent;
            TDelegate handler;

            public FromEvent(FromEventObservable<TDelegate, TEventArgs> parent, Action<TEventArgs> observer)
            {
                this.parent = parent;
                handler = parent.conversion(observer);
                parent.addHandler(handler);
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
        readonly Action<Action> addHandler;
        readonly Action<Action> removeHandler;

        public FromEventObservable(Action<Action> addHandler, Action<Action> removeHandler)
        {
            this.addHandler = addHandler;
            this.removeHandler = removeHandler;
        }

        public IDisposable Subscribe(Action<Unit> observer)
        {
            return new FromEvent(this, observer);
        }

        class FromEvent : IDisposable
        {
            readonly FromEventObservable parent;
            readonly Action<Unit> observer;
            Action handler;

            public FromEvent(FromEventObservable parent, Action<Unit> observer)
            {
                this.parent = parent;
                this.observer = observer;
                handler = OnNext;
                parent.addHandler(handler);
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