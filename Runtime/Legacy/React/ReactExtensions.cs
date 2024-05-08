using System;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework
{
    public static class ReactExtensions
    {
        /// <summary>
        /// Release unRegister handle when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IDisposable Add(this IDisposable handle, GameObject gameObject)
        {
            gameObject.GetUnRegister().AddDisposable(handle);
            return handle;
        }
        /// <summary>
        /// Release unRegister handle managed by a unRegister
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IDisposable Add(this IDisposable handle, IUnRegister unRegister)
        {
            unRegister.AddDisposable(handle);
            return handle;
        }
        /// <summary>
        /// Get or create an UnRegister from GameObject, listening OnDestroy event
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static GameObjectOnDestroyUnRegister GetUnRegister(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<GameObjectOnDestroyUnRegister>(out var trigger))
            {
                trigger = gameObject.AddComponent<GameObjectOnDestroyUnRegister>();
            }
            return trigger;
        }
        public static void SubscribeOnce(this IAkiEvent<Action> akiEvent, Action action)
        {
            action += () => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T>(this IAkiEvent<Action<T>> akiEvent, Action<T> action)
        {
            action += (a) => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T, K>(this IAkiEvent<Action<T, K>> akiEvent, Action<T, K> action)
        {
            action += (a, b) => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void SubscribeOnce<T, K, F>(this IAkiEvent<Action<T, K, F>> akiEvent, Action<T, K, F> action)
        {
            action += (a, b, c) => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
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
            return new CallBackDisposableHandle(() => unityEvent.RemoveListener(action));
        }
        public static IDisposable Subscribe<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.AddListener(action);
            return new CallBackDisposableHandle(() => unityEvent.RemoveListener(action));
        }
        public static void SubscribeWithUnRegister(this UnityEvent unityEvent, UnityAction action, IUnRegister unRegister)
        {
            unityEvent.Subscribe(action).Add(unRegister);
        }
        public static void SubscribeWithUnRegister(this UnityEvent unityEvent, UnityAction action, GameObject gameObject)
        {
            unityEvent.Subscribe(action).Add(gameObject);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, IUnRegister unRegister)
        {
            unityEvent.Subscribe(action).Add(unRegister);
        }
        public static void SubscribeWithUnRegister<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, GameObject gameObject)
        {
            unityEvent.Subscribe(action).Add(gameObject);
        }
        #endregion
    }
}
