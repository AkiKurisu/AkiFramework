using System;
using UnityEngine.Events;
using Kurisu.Framework.Events;
namespace Kurisu.Framework.React
{
    public static class ObservableExtensions
    {
        #region UnityEvents
        /// <summary>
        /// React version for <see cref="UnityEvent.AddListener"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IDisposable Subscribe(this UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.AddListener(action);
            return Disposable.Create(() => unityEvent.RemoveListener(action));
        }
        /// <summary>
        /// React version for <see cref="UnityEvent{T}.AddListener"/>
        /// </summary>
        /// <param name="unityEvent"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDisposable Subscribe<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.AddListener(action);
            return Disposable.Create(() => unityEvent.RemoveListener(action));
        }
        public static IDisposable SubscribeOnce<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            action += (a) => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
            return Disposable.Create(() => unityEvent.RemoveListener(action)); ;
        }
        public static IDisposable SubscribeOnce(this UnityEvent unityEvent, UnityAction action)
        {
            action += () => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
            return Disposable.Create(() => unityEvent.RemoveListener(action)); ;
        }
        #endregion
        #region Events
        /// <summary>
        /// React version for <see cref="CallbackEventHandler.RegisterCallback"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="callback"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static IDisposable Subscribe<TEventType>(this CallbackEventHandler handler, EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback(callback);
            return Disposable.Create(() => handler.UnregisterCallback(callback));
        }
        #endregion
    }
}
