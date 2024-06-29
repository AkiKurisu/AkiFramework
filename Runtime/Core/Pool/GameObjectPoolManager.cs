using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kurisu.Framework.React;
using Kurisu.Framework.Schedulers;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Pool
{
    /// <summary>
    /// Wrapper for pooling gameObject
    /// </summary>
    public class PooledGameObject : IDisposable, IUnRegister
    {
        public PooledGameObject() { }
        private readonly static _ObjectPool<PooledGameObject> pool = new(() => new());
        public GameObject GameObject { get; protected set; }
        public Transform Transform { get; protected set; }
        public bool IsDisposed { get; protected set; }
        protected DisposableBag disposableBag;
        public string Name { get; protected set; }

        /// <summary>
        /// Get or create empty pooled gameObject by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static PooledGameObject Get(string address, Transform parent = null)
        {
            var pooledObject = pool.Get();
            pooledObject.Name = address;
            pooledObject.GameObject = GameObjectPoolManager.Get(address, out _, parent);
            pooledObject.Init();
            return pooledObject;
        }

        /// <summary>
        /// Should not use AddTo(GameObject gameObject) since gameObject will not be destroyed until pool manager cleanup.
        /// </summary>
        /// <param name="disposable"></param>
        /// <remarks>
        /// Implement of <see cref="IUnRegister"/> to manage <see cref="IDisposable"/> in pooling scope.
        /// </remarks>
        void IUnRegister.Add(IDisposable disposable)
        {
            disposableBag.Add(disposable);
        }
        protected virtual void Init()
        {
            LocalInit();
        }
        private void LocalInit()
        {
            Transform = GameObject.transform;
            disposableBag = new();
        }
        public virtual void Dispose()
        {
            if (IsDisposed) return;
            disposableBag.Dispose();
            if (GameObjectPoolManager.IsInstantiated)
                GameObjectPoolManager.Release(GameObject, Name);
            IsDisposed = true;
            pool.Release(this);
        }
        public void Destroy(float t = 0f)
        {
            if (t >= 0f)
                Scheduler.Delay(t, Dispose).AddTo(this);
            else
                Dispose();
        }
    }
    public class PooledComponent<T, K> : PooledGameObject where K : Component where T : PooledComponent<T, K>, new()
    {
        /// <summary>
        /// Cache component as meta data to reduce allocation
        /// </summary>
        public class ComponentCache : IPooledMetaData
        {
            public K component;
        }
        public K Component => Cache.component;
        protected ComponentCache Cache { get; set; }
        private static readonly string componentName;
        static PooledComponent()
        {
            componentName = typeof(T).FullName;
        }
        internal readonly static _ObjectPool<T> pool = new(() => new());
        /// <summary>
        /// Get or create empty pooled component
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T Get(Transform parent = null)
        {
            var pooledComponent = pool.Get();
            pooledComponent.Name = componentName;
            pooledComponent.GameObject = GameObjectPoolManager.Get(componentName, out var metaData, parent);
            pooledComponent.Cache = metaData as ComponentCache;
            pooledComponent.Init();
            return pooledComponent;
        }
        public static string GetFullPath(GameObject prefab)
        {
            // append instance id since prefabs may have same name
            return $"{prefab.name} {prefab.GetInstanceID()}";
        }
        /// <summary>
        /// Instantiate pooled component by prefab, optimized version of <see cref="Object.Instantiate(Object, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T Instantiate(GameObject prefab, Transform parent = null)
        {
            var pooledComponent = pool.Get();
            string fullPath = GetFullPath(prefab);
            pooledComponent.Name = fullPath;
            var @object = GameObjectPoolManager.Get(fullPath, out var metaData, parent, createEmptyIfNotExist: false);
            if (!@object)
            {
                @object = Object.Instantiate(prefab, parent);
            }
            pooledComponent.Cache = metaData as ComponentCache;
            pooledComponent.GameObject = @object;
            pooledComponent.Init();
            return pooledComponent;
        }
        /// <summary>
        /// Instantiate pooled component by prefab, optimized version of <see cref="Object.Instantiate(Object, Vector3, Quaternion, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent">The parent attached to. If parent exists, it will use prefab's scale as local scale instead of lossy scale</param>
        /// <param name="useLocalPosition">Whether use local position instead of world position, default is false</param>
        /// <returns></returns>
        public static T Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            var pooledComponent = Instantiate(prefab, parent);
            if (useLocalPosition)
                pooledComponent.GameObject.transform.SetLocalPositionAndRotation(position, rotation);
            else
                pooledComponent.GameObject.transform.SetPositionAndRotation(position, rotation);
            return pooledComponent;
        }
        protected override void Init()
        {
            LocalInit();
        }
        private void LocalInit()
        {
            IsDisposed = false;
            disposableBag = new();
            Transform = GameObject.transform;
            Cache ??= new ComponentCache();
            if (!Cache.component)
            {
                // allocate few to get component from gameObject
                Cache.component = GameObject.GetOrAddComponent<K>();
            }
        }
        public sealed override void Dispose()
        {
            if (IsDisposed) return;
            OnDispose();
            disposableBag.Dispose();
            if (GameObjectPoolManager.IsInstantiated)
                GameObjectPoolManager.Release(GameObject, Name, Cache);
            IsDisposed = true;
            pool.Release((T)this);
        }
        protected virtual void OnDispose() { }
    }
    public interface IPooledMetaData { }
    public sealed class GameObjectPoolManager : MonoBehaviour
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
        /// <param name="pooledMetaData"></param>
        /// <param name="parent"></param>
        /// <param name="createEmptyIfNotExist"></param>
        /// <returns></returns>
        public static GameObject Get(string address, out IPooledMetaData pooledMetaData, Transform parent = null, bool createEmptyIfNotExist = true)
        {
            GameObject obj = null;
            pooledMetaData = null;
            if (Instance.poolDic.TryGetValue(address, out GameObjectPool poolData) && poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent, out pooledMetaData);
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
        /// <param name="pooledMetaData"></param>
        public static void Release(GameObject obj, string address = null, IPooledMetaData pooledMetaData = null)
        {
            address ??= obj.name;
            if (!Instance.poolDic.TryGetValue(address, out GameObjectPool poolData))
            {
                poolData = Instance.poolDic[address] = new GameObjectPool(address, Instance.transform);
            }
            poolData.PushObj(obj, pooledMetaData);
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
            public readonly GameObject fatherObj;
            public readonly Queue<GameObject> poolQueue = new();
            private readonly Dictionary<GameObject, IPooledMetaData> metaData = new();
            public GameObjectPool(string address, Transform poolRoot)
            {
                fatherObj = new GameObject(address);
                fatherObj.transform.SetParent(poolRoot);
            }
            public void PushObj(GameObject obj, IPooledMetaData pooledMetaData)
            {
                poolQueue.Enqueue(obj);
                metaData[obj] = pooledMetaData;
                obj.transform.SetParent(fatherObj.transform);
                obj.SetActive(false);
            }
            public GameObject GetObj(Transform parent, out IPooledMetaData pooledMetaData)
            {
                GameObject obj = poolQueue.Dequeue();
                if (metaData.TryGetValue(obj, out pooledMetaData)) metaData.Remove(obj);
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