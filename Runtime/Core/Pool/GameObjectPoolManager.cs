using System;
using System.Collections.Generic;
using Kurisu.Framework.React;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Kurisu.Framework.Pool
{
    /// <summary>
    /// Wrapper for pooling gameObject
    /// </summary>
    public class PooledGameObject : IDisposable, IUnRegister
    {
        public PooledGameObject() { }
        private readonly static UnityEngine.Pool.ObjectPool<PooledGameObject> pool = new(() => new());
        public GameObject GameObject { get; protected set; }
        public bool IsDisposed { get; protected set; }
        protected DisposableBag disposableBag;
        public string Name { get; protected set; }
        public static PooledGameObject Get(string address, Transform parent = null)
        {
            var pooledObject = pool.Get();
            pooledObject.Name = address;
            pooledObject.GameObject = GameObjectPoolManager.Get(address, parent);
            pooledObject.disposableBag = new();
            return pooledObject;
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
            if (IsDisposed) return;
            disposableBag.Dispose();
            if (GameObjectPoolManager.IsInstantiated)
                GameObjectPoolManager.Release(GameObject, Name);
            IsDisposed = true;
            pool.Release(this);
        }
    }
    public class PooledComponent<T, K> : PooledGameObject where K : Component where T : PooledComponent<T, K>, new()
    {
        public K Component { get; protected set; }
        private static readonly string componentName;
        static PooledComponent()
        {
            componentName = typeof(T).FullName;
        }
        protected readonly static UnityEngine.Pool.ObjectPool<T> pool = new(() => new());
        public new static T Get(string name, Transform parent = null)
        {
            var pooledComponent = pool.Get();
            pooledComponent.GameObject = GameObjectPoolManager.Get($"{componentName}.{name}", parent);
            pooledComponent.Init();
            return pooledComponent;
        }
        public static T Get(Transform parent = null)
        {
            var pooledComponent = pool.Get();
            pooledComponent.GameObject = GameObjectPoolManager.Get(componentName, parent);
            pooledComponent.Init();
            return pooledComponent;
        }
        protected virtual void Init()
        {
            LocalInit();
        }
        private void LocalInit()
        {
            IsDisposed = false;
            disposableBag = new();
            Component = GameObject.GetOrAddComponent<K>();
        }
    }
    public class GameObjectPoolManager : MonoBehaviour
    {
        private static GameObjectPoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new() { name = nameof(GameObjectPoolManager) };
                    instance = managerObject.AddComponent<GameObjectPoolManager>();
                }
                return instance;
            }
        }
        public static bool IsInstantiated => instance != null;
        private static GameObjectPoolManager instance;
        private readonly Dictionary<string, GameObjectPool> poolDic = new();
        private void OnDestroy()
        {
            if (instance == this) instance = null;
            LocalReleaseAll();
        }
        /// <summary>
        /// Get pooled gameObject
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <param name="createEmptyIfNotExist"></param>
        /// <returns></returns>
        public static GameObject Get(string address, Transform parent = null, bool createEmptyIfNotExist = true)
        {
            GameObject obj = null;
            if (Instance.poolDic.TryGetValue(address, out GameObjectPool poolData) && poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent);
            }
            else if (createEmptyIfNotExist)
            {
                obj = new GameObject(address);
            }
            return obj;
        }
        /// <summary>
        /// Release gameObject to pool
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="address"></param>
        public static void Release(GameObject obj, string address = null)
        {
            address ??= obj.name;
            if (Instance.poolDic.TryGetValue(address, out GameObjectPool poolData))
            {
                poolData.PushObj(obj);
            }
            else
            {
                Instance.poolDic.Add(address, new GameObjectPool(address, obj, Instance.gameObject));
            }
        }
        /// <summary>
        /// Release addressable gameObject pool
        /// </summary>
        /// <param name="address"></param>
        public static void ReleasePool(string address)
        {
            GameObject go = Instance.transform.Find(address).gameObject;
            if (go)
            {
                Destroy(go);
                Instance.poolDic.Remove(address);
            }
        }
        private void LocalReleaseAll()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            poolDic.Clear();
        }
        /// <summary>
        /// Release all pooled gameObjects
        /// </summary>
        public static void ReleaseAll()
        {
            Instance.LocalReleaseAll();
        }
        private class GameObjectPool
        {
            public GameObject fatherObj;
            public Queue<GameObject> poolQueue;
            public GameObjectPool(string address, GameObject obj, GameObject poolRootObj)
            {
                fatherObj = new GameObject(address);
                fatherObj.transform.SetParent(poolRootObj.transform);
                poolQueue = new Queue<GameObject>();
                PushObj(obj);
            }
            public void PushObj(GameObject obj)
            {
                poolQueue.Enqueue(obj);
                obj.transform.SetParent(fatherObj.transform);
                obj.SetActive(false);
            }
            public GameObject GetObj(Transform parent = null)
            {
                GameObject obj = poolQueue.Dequeue();
                obj.SetActive(true);
                obj.transform.SetParent(parent);
                if (parent == null)
                {
                    SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
                }
                return obj;
            }
        }
    }
}