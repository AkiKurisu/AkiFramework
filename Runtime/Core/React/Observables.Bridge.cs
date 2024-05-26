using UnityEngine.Events;
using Kurisu.Framework.Events;
using System;
namespace Kurisu.Framework.React
{
    public static partial class Observable
    {
        #region UnityEvents
        /// <summary>
        /// Create Observable for <see cref="UnityEvent"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <returns></returns>
        public static IObservable<Unit> AsObservable(this UnityEvent unityEvent)
        {
            return FromEvent(h => new UnityAction(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h));
        }
        /// <summary>
        /// Create Observable for <see cref="UnityEvent{T}"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> AsObservable<T>(this UnityEvent<T> unityEvent)
        {
            return FromEvent<UnityAction<T>, T>(h => new UnityAction<T>(h), h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h));
        }
        /// <summary>
        /// Create Observable for <see cref="UnityEvent{T0,T1}"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public static IObservable<Tuple<T0, T1>> AsObservable<T0, T1>(this UnityEvent<T0, T1> unityEvent)
        {
            return FromEvent<UnityAction<T0, T1>, Tuple<T0, T1>>(h =>
            {
                return new UnityAction<T0, T1>((t0, t1) =>
                {
                    h(Tuple.Create(t0, t1));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h));
        }
        /// <summary>
        /// Create Observable for <see cref="UnityEvent{T0,T1,T2}"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static IObservable<Tuple<T0, T1, T2>> AsObservable<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent)
        {
            return FromEvent<UnityAction<T0, T1, T2>, Tuple<T0, T1, T2>>(h =>
            {
                return new UnityAction<T0, T1, T2>((t0, t1, t2) =>
                {
                    h(Tuple.Create(t0, t1, t2));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h));
        }
        /// <summary>
        /// Create Observable for <see cref="UnityEvent{T0,T1,T2,T3}"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static IObservable<Tuple<T0, T1, T2, T3>> AsObservable<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent)
        {
            return FromEvent<UnityAction<T0, T1, T2, T3>, Tuple<T0, T1, T2, T3>>(h =>
            {
                return new UnityAction<T0, T1, T2, T3>((t0, t1, t2, t3) =>
                {
                    h(Tuple.Create(t0, t1, t2, t3));
                });
            }, h => unityEvent.AddListener(h), h => unityEvent.RemoveListener(h));
        }
        #endregion
        #region AF Events
        /// <summary>
        /// Create Observable for <see cref="CallbackEventHandler"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trickleDown"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static IObservable<TEventType> AsObservable<TEventType>(this CallbackEventHandler handler, TrickleDown trickleDown = TrickleDown.NoTrickleDown)
        where TEventType : EventBase<TEventType>, new()
        {
            return FromEvent<EventCallback<TEventType>, TEventType>(h => new EventCallback<TEventType>(h),
            //frame:5 => Skip this, FromEventObservable and FromEvent
            h => handler.RegisterCallback(h, trickleDown, 5), h => handler.UnregisterCallback(h, trickleDown));
        }
        /// <summary>
        /// Create Observable for <see cref="CallbackEventHandler"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="trickleDown"></param>
        /// <param name="skipFrame">Skip frames for debugger</param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static IObservable<TEventType> AsObservable<TEventType>(this CallbackEventHandler handler, TrickleDown trickleDown = TrickleDown.NoTrickleDown, int skipFrame = 5)
        where TEventType : EventBase<TEventType>, new()
        {
            return FromEvent<EventCallback<TEventType>, TEventType>(h => new EventCallback<TEventType>(h),
            h => handler.RegisterCallback(h, trickleDown, skipFrame), h => handler.UnregisterCallback(h, trickleDown));
        }
        #endregion
    }
}
