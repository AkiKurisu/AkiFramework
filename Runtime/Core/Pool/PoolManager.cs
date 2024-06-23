using System;
using System.Collections.Generic;
using Kurisu.Framework.React;
using R3;
using UnityEngine;
namespace Kurisu.Framework.Pool
{

    /// <summary>
    /// Wrapper for pooling gameObject
    /// </summary>
    public class PooledGameObject : IDisposable, IUnRegister
    {
        public readonly GameObject gameObject;
        private bool isDisposed;
        private readonly DisposableBag disposableBag;
        public PooledGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            disposableBag = new();
        }
        public static PooledGameObject Get(string name, Transform parent = null)
        {
            return new PooledGameObject(PoolManager.Instance.GetGameObject(name, parent));
        }

        /// <summary>
        /// Should not use AddTo(GameObject gameObject) since gameObject will not be destroyed until pool manager cleanup
        /// </summary>
        /// <param name="disposable"></param>
        public void Add(IDisposable disposable)
        {
            disposableBag.Add(disposable);
        }

        public void Dispose()
        {
            if (isDisposed) return;
            disposableBag.Dispose();
            if (PoolManager.IsInstantiated)
                PoolManager.Instance.ReleaseGameObject(gameObject);
            isDisposed = true;
        }

    }
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new() { name = nameof(PoolManager) };
                    instance = managerObject.AddComponent<PoolManager>();
                }
                return instance;
            }
        }
        public static bool IsInstantiated => instance != null;
        private static PoolManager instance;
        private readonly Dictionary<string, GameObjectPool> gameObjectPoolDic = new();
        private void OnDestroy()
        {
            if (instance == this) instance = null;
            Clear();
        }
        public GameObject GetGameObject(string assetName, Transform parent = null)
        {
            GameObject obj;
            if (gameObjectPoolDic.TryGetValue(assetName, out GameObjectPool poolData) && poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent);
            }
            else
            {
                obj = new GameObject(assetName);
            }
            return obj;
        }
        public void ReleaseGameObject(GameObject obj, string overrideName = null)
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

        public void Clear()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            gameObjectPoolDic.Clear();
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

    }
}