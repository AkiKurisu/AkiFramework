using System;
namespace Kurisu.Framework.React
{
    public static partial class Observable
    {
        /// <summary>
        /// Create anonymous observable. Observer has exception durability. This is recommended for make operator and event like generator. 
        /// </summary>
        public static IObservable<T> Create<T>(Func<Action<T>, IDisposable> subscribe) where T : Delegate
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));

            return new CreateObservable<T>(subscribe);
        }
        /// <summary>
        /// Create anonymous observable. Observer has exception durability. This is recommended for make operator and event like generator. 
        /// </summary>
        public static IObservable<T> CreateWithState<T, TState>(TState state, Func<TState, Action<T>, IDisposable> subscribe)
        {
            if (subscribe == null) throw new ArgumentNullException(nameof(subscribe));

            return new CreateObservable<T, TState>(state, subscribe);
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
        /// <summary>
        /// Empty Observable. 
        /// </summary>
        public static IObservable<T> Empty<T>()
        {
            return new EmptyObservable<T>();
        }
    }
}