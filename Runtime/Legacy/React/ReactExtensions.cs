using System;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework
{
    public static class ReactExtensions
    {
        #region IDisposable
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
        #endregion
        #region IObservable
        public static IDisposable Subscribe<T>(this IObservable<T> observable, T action) where T : Delegate
        {
            observable.Register(action);
            return new CallBackDisposable(() => observable.Unregister(action));
        }
        public static IObservable<T> SubscribeOnce<T>(this IObservable<T> observable, T action) where T : Delegate
        {
            T combinedAction = (T)Delegate.Combine(action, (Action)(() => observable.Unregister(action)));
            observable.Register(combinedAction);
            return observable;
        }
        public static void SubscribeWithUnRegister<T>(this IObservable<T> observable, T action, IUnRegister unRegister) where T : Delegate
        {
            observable.Subscribe(action).AddTo(unRegister);
        }
        public static void SubscribeWithUnRegister<T>(this IObservable<T> observable, T action, GameObject gameObject) where T : Delegate
        {
            observable.Subscribe(action).AddTo(gameObject);
        }
        public static void SubscribeWithUnRegister<T>(this IObservable<T> observable, T action, Component component) where T : Delegate
        {
            observable.Subscribe(action).AddTo(component);
        }
        #endregion
        #region UnityEvents
        public static UnityEvent<T> SubscribeOnce<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            action += (a) => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
            return unityEvent;
        }
        public static UnityEvent SubscribeOnce(this UnityEvent unityEvent, UnityAction action)
        {
            action += () => unityEvent.RemoveListener(action);
            unityEvent.AddListener(action);
            return unityEvent;
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
        public static void SubscribeWithUnRegister(this UnityEvent unityEvent, UnityAction action, Component component)
        {
            unityEvent.Subscribe(action).AddTo(component);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, IUnRegister unRegister)
        {
            unityEvent.Subscribe(action).AddTo(unRegister);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, GameObject gameObject)
        {
            unityEvent.Subscribe(action).AddTo(gameObject);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, Component component)
        {
            unityEvent.Subscribe(action).AddTo(component);
        }
        #endregion
    }
}
