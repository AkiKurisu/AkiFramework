using System;
using System.Threading;
using Kurisu.Framework.Events;
using R3;
namespace Kurisu.Framework.React
{
    public static class ObservableExtensions
    {
        #region CallbackEventHandler
        /// <summary>
        /// Create Observable for <see cref="CallbackEventHandler"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static Observable<TEventType> AsObservable<TEventType>(this CallbackEventHandler handler)
where TEventType : EventBase<TEventType>, new()
        {
            return handler.AsObservable<TEventType>(TrickleDown.NoTrickleDown, 8);
        }
        /// <summary>
        /// Create Observable for <see cref="CallbackEventHandler"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trickleDown"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static Observable<TEventType> AsObservable<TEventType>(this CallbackEventHandler handler, TrickleDown trickleDown)
        where TEventType : EventBase<TEventType>, new()
        {
            return handler.AsObservable<TEventType>(trickleDown, 8);
        }
        /// <summary>
        /// Create Observable for <see cref="CallbackEventHandler"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trickleDown"></param>
        /// <param name="skipFrame">Skip frames for debugger</param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static Observable<TEventType> AsObservable<TEventType>(this CallbackEventHandler handler, TrickleDown trickleDown, int skipFrame)
        where TEventType : EventBase<TEventType>, new()
        {
            CancellationToken cancellationToken = default;
            if (handler is IBehaviourScope behaviourScope && behaviourScope.AttachedBehaviour)
                cancellationToken = behaviourScope.AttachedBehaviour.destroyCancellationToken;
            return new FromEventHandler<TEventType>(static h => new(h),
            h => handler.RegisterCallback(h, trickleDown, skipFrame), h => handler.UnregisterCallback(h, trickleDown), cancellationToken);
        }
        #endregion
        #region IReadonlyReactiveProperty<T>
        /// <summary>
        /// Create Observable for <see cref="IReadonlyReactiveProperty{T}"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trickleDown"></param>
        /// <param name="skipFrame">Skip frames for debugger</param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static Observable<ChangeEvent<T>> ValueChangeAsObservable<T>(this IReadonlyReactiveProperty<T> handler)
        {
            CancellationToken cancellationToken = default;
            if (handler is IBehaviourScope behaviourScope && behaviourScope.AttachedBehaviour)
                cancellationToken = behaviourScope.AttachedBehaviour.destroyCancellationToken;
            return new FromEventHandler<ChangeEvent<T>>(static h => new(h),
            h => handler.RegisterValueChangeCallback(h), h => handler.UnregisterValueChangeCallback(h), cancellationToken);
        }
        #endregion
        /// <summary>
        /// Subscribe <see cref="Observable{TEventType}"/> and finally dispose event, better performance for <see cref="EventBase"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static IDisposable SubscribeSafe<TEventType>(this Observable<TEventType> source, EventCallback<TEventType> onNext) where TEventType : EventBase<TEventType>, new()
        {
            var action = new Action<TEventType>(OnNext);
            void OnNext(TEventType evt)
            {
                onNext(evt);
                evt.Dispose();
            }
            return source.Subscribe(action);
        }
    }
}
