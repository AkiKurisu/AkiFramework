using System;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework
{
    public static class ReactExtensions
    {
        /// <summary>
        /// Dispose when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, GameObject gameObject) where T : IDisposable
        {
            gameObject.GetUnRegister().AddDisposable(handle);
            return handle;
        }
        /// <summary>
        /// Dispose when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, Component component) where T : IDisposable
        {
            component.gameObject.GetUnRegister().AddDisposable(handle);
            return handle;
        }
        /// <summary>
        /// Dispose managed by a unRegister
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, IUnRegister unRegister) where T : IDisposable
        {
            unRegister.AddDisposable(handle);
            return handle;
        }
        /// <summary>
        /// Get or create an UnRegister from GameObject, listening OnDestroy event
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegister GetUnRegister(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<GameObjectOnDestroyUnRegister>(out var trigger))
            {
                trigger = gameObject.AddComponent<GameObjectOnDestroyUnRegister>();
            }
            return trigger;
        }
        #region IObservable
        public static IDisposable Subscribe(this IObservable<Action> akiEvent, Action action)
        {
            akiEvent.Register(action);
            return new CallBackDisposable(() => akiEvent.Unregister(action));
        }
        public static IDisposable Subscribe<T>(this IObservable<Action<T>> akiEvent, Action<T> action)
        {
            akiEvent.Register(action);
            return new CallBackDisposable(() => akiEvent.Unregister(action));
        }
        public static IDisposable Subscribe<T, K>(this IObservable<Action<T, K>> akiEvent, Action<T, K> action)
        {
            akiEvent.Register(action);
            return new CallBackDisposable(() => akiEvent.Unregister(action));
        }
        public static IDisposable Subscribe<T, K, F>(this IObservable<Action<T, K, F>> akiEvent, Action<T, K, F> action)
        {
            akiEvent.Register(action);
            return new CallBackDisposable(() => akiEvent.Unregister(action));
        }
        public static void SubscribeOnce(this IObservable<Action> akiEvent, Action action)
        {
            action += () => akiEvent.Unregister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T>(this IObservable<Action<T>> akiEvent, Action<T> action)
        {
            action += (a) => akiEvent.Unregister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T, K>(this IObservable<Action<T, K>> akiEvent, Action<T, K> action)
        {
            action += (a, b) => akiEvent.Unregister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T, K, F>(this IObservable<Action<T, K, F>> akiEvent, Action<T, K, F> action)
        {
            action += (a, b, c) => akiEvent.Unregister(action);
            akiEvent.Register(action);
        }
        #endregion
        #region UnityEvents
        public static void SubscribeOnce<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            action += (a) => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
        }
        public static void SubscribeOnce(this UnityEvent unityEvent, UnityAction action)
        {
            action += () => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
        }
        public static IDisposable Subscribe(this UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.AddListener(action);
            return new CallBackDisposable(() => unityEvent.RemoveListener(action));
        }
        public static IDisposable Subscribe<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.AddListener(action);
            return new CallBackDisposable(() => unityEvent.RemoveListener(action));
        }
        public static void SubscribeWithUnRegister(this UnityEvent unityEvent, UnityAction action, IUnRegister unRegister)
        {
            unityEvent.Subscribe(action).AddTo(unRegister);
        }
        public static void SubscribeWithUnRegister(this UnityEvent unityEvent, UnityAction action, GameObject gameObject)
        {
            unityEvent.Subscribe(action).AddTo(gameObject);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, IUnRegister unRegister)
        {
            unityEvent.Subscribe(action).AddTo(unRegister);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, GameObject gameObject)
        {
            unityEvent.Subscribe(action).AddTo(gameObject);
        }
        #endregion
    }
}
