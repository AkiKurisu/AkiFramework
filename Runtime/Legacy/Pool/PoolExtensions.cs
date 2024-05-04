using UnityEngine;
namespace Kurisu.Framework
{
    public static class PoolExtensions
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
    }
}
