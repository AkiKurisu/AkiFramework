using System;
using UnityEngine;
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
            PoolManager.Instance.PushObject(obj);
        }
        public static void ObjectPushPool(this IPooled obj, string overrideName)
        {
            PoolManager.Instance.PushObject(obj, overrideName);
        }
        public static bool IsNull(this GameObject obj)
        {
            return obj == null;
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
    }

}
