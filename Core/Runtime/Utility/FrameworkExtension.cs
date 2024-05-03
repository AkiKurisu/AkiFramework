using System;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework
{
    public static class FrameworkExtension
    {
        public static void GameObjectPushPool(this GameObject go, string overrideName = null)
        {
            PoolManager.Instance.PushGameObject(go, overrideName);
        }
        public static void GameObjectPushPool(this Component com, string overrideName = null)
        {
            GameObjectPushPool(com.gameObject, overrideName);
        }
        public static void ObjectPushPool(this IPooled obj)
        {
            PoolManager.Instance.ReleaseObject(obj);
        }
        public static void ObjectPushPool(this IPooled obj, string overrideName)
        {
            PoolManager.Instance.ReleaseObject(obj, overrideName);
        }
        public static void RegisterOnce(this IAkiEvent<Action> akiEvent, Action action)
        {
            action += () => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void RegisterOnce<T>(this IAkiEvent<Action<T>> akiEvent, Action<T> action)
        {
            action += (a) => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void RegisterOnce<T, K>(this IAkiEvent<Action<T, K>> akiEvent, Action<T, K> action)
        {
            action += (a, b) => akiEvent.UnRegister(action);
            akiEvent.Register(action);
        }
        public static void RegisterOnce<T, K, F>(this IAkiEvent<Action<T, K, F>> akiEvent, Action<T, K, F> action)
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
        public static void Subscribe(this UnityEvent unityEvent, UnityAction action, IUnRegister unRegister)
        {
            unityEvent.AddListener(action);
            unRegister.AddUnRegisterHandle(new UnRegisterCallBackHandle(() => unityEvent.RemoveListener(action)));
        }
        public static void Subscribe(this UnityEvent unityEvent, UnityAction action, GameObject gameObject)
        {
            unityEvent.AddListener(action);
            gameObject.GetUnRegister().AddUnRegisterHandle(new UnRegisterCallBackHandle(() => unityEvent.RemoveListener(action)));
        }
        public static void Subscribe<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, IUnRegister unRegister)
        {
            unityEvent.AddListener(action);
            unRegister.AddUnRegisterHandle(new UnRegisterCallBackHandle(() => unityEvent.RemoveListener(action)));
        }
        public static void Subscribe<T>(this UnityEvent<T> unityEvent, UnityAction<T> action, GameObject gameObject)
        {
            unityEvent.AddListener(action);
            gameObject.GetUnRegister().AddUnRegisterHandle(new UnRegisterCallBackHandle(() => unityEvent.RemoveListener(action)));
        }
        #endregion
        /// <summary>
        /// Release unRegister handle when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegisterHandle AttachUnRegister(this IUnRegisterHandle handle, GameObject gameObject)
        {
            gameObject.GetUnRegister().AddUnRegisterHandle(handle);
            return handle;
        }
        /// <summary>
        /// Release unRegister handle managed by a unRegister
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegisterHandle AttachUnRegister(this IUnRegisterHandle handle, IUnRegister unRegister)
        {
            unRegister.AddUnRegisterHandle(handle);
            return handle;
        }
        public static UnRegisterOnDestroyTrigger GetUnRegister(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<UnRegisterOnDestroyTrigger>(out var trigger))
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }
            return trigger;
        }
    }

}
