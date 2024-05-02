using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework
{
    public interface IPooled { }
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new() { name = "PoolManager" };
                    instance = managerObject.AddComponent<PoolManager>();
                }
                return instance;
            }
        }
        private static PoolManager instance;
        public Dictionary<string, GameObjectPool> gameObjectPoolDic = new();
        public Dictionary<string, ObjectPool> objectPoolDic = new();
        private void OnDestroy()
        {
            if (instance == this) instance = null;
            Clear();
        }
        #region GameObject
        public GameObject GetGameObject(string assetName, Transform parent = null)
        {
            GameObject obj = null;
            if (gameObjectPoolDic.TryGetValue(assetName, out GameObjectPool poolData) && poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent);
            }
            return obj;
        }
        public void PushGameObject(GameObject obj, string overrideName = null)
        {
            string name = overrideName ?? obj.name;
            if (gameObjectPoolDic.TryGetValue(name, out GameObjectPool poolData))
            {
                poolData.PushObj(obj);
            }
            else
            {
                gameObjectPoolDic.Add(name, new GameObjectPool(obj, gameObject));
            }
        }

        #endregion

        #region C# object
        public T GetObject<T>() where T : class, IPooled, new()
        {
            T obj;
            if (CheckObjectCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)objectPoolDic[name].Get();
                return obj;
            }
            else
            {
                return new T();
            }
        }
        public T GetScriptableObject<T>(string objectName) where T : ScriptableObject
        {
            T obj;
            if (CheckObjectCache(objectName))
            {
                obj = (T)objectPoolDic[objectName].Get();
                return obj;
            }
            else
            {
                return null;
            }
        }

        public void PushObject(object obj)
        {
            string name = obj.GetType().FullName;
            if (objectPoolDic.ContainsKey(name))
            {
                objectPoolDic[name].Push(obj);
            }
            else
            {
                objectPoolDic.Add(name, new ObjectPool(obj));
            }
        }
        public void PushObject(object obj, string overrideName)
        {
            if (objectPoolDic.ContainsKey(overrideName))
            {
                objectPoolDic[overrideName].Push(obj);
            }
            else
            {
                objectPoolDic.Add(overrideName, new ObjectPool(obj));
            }
        }


        private bool CheckObjectCache<T>()
        {
            string name = typeof(T).FullName;
            return objectPoolDic.ContainsKey(name) && objectPoolDic[name].poolQueue.Count > 0;
        }
        private bool CheckObjectCache(string objectName)
        {
            return objectPoolDic.ContainsKey(objectName) && objectPoolDic[objectName].poolQueue.Count > 0;
        }

        #endregion


        #region Release
        public void Clear(bool clearGameObject = true, bool clearCObject = true)
        {
            if (clearGameObject)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
                gameObjectPoolDic.Clear();
            }

            if (clearCObject)
            {
                objectPoolDic.Clear();
            }
        }

        public void ClearAllGameObject()
        {
            Clear(true, false);
        }
        public void ClearGameObject(string prefabName)
        {
            GameObject go = transform.Find(prefabName).gameObject;
            if (go)
            {
                Destroy(go);
                gameObjectPoolDic.Remove(prefabName);

            }

        }
        public void ClearGameObject(GameObject prefab)
        {
            ClearGameObject(prefab.name);
        }

        public void ClearAllObject()
        {
            Clear(false, true);
        }
        public void ClearObject<T>()
        {
            objectPoolDic.Remove(typeof(T).FullName);
        }
        public void ClearObject(Type type)
        {
            objectPoolDic.Remove(type.FullName);
        }
        #endregion

    }
}