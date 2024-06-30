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
        public class DisposableBag : IDisposable
        {
            private IDisposable[] items;
            private bool isDisposed;
            private int count;
            public DisposableBag() { }
            public DisposableBag(int capacity)
            {
                isDisposed = false;
                count = 0;
                items = new IDisposable[capacity];
            }
            public void Add(IDisposable item)
            {
                if (isDisposed)
                {
                    item.Dispose();
                    return;
                }

                if (items == null)
                {
                    items = new IDisposable[4];
                }
                else if (count == items.Length)
                {
                    Array.Resize(ref items, count * 2);
                }

                items[count++] = item;
            }
            public void Clear()
            {
                if (items != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        items[i]?.Dispose();
                    }

                    items = null;
                    count = 0;
                }
                isDisposed = false;
            }
            public void Dispose()
            {
                Clear();
                isDisposed = true;
            }
        }
        public PooledGameObject() { }
        private readonly static _ObjectPool<PooledGameObject> pool = new(() => new());
        public GameObject GameObject { get; protected set; }
        public Transform Transform { get; protected set; }
        public bool IsDisposed { get; protected set; }
        protected readonly DisposableBag disposableBag = new();
        public PooledKey Name { get; protected set; }
        public static void SetMaxSize(int size)
        {
            pool.MaxSize = size;
        }
        /// <summary>
        /// Get or create empty pooled gameObject by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static PooledGameObject Get(string address, Transform parent = null)
        {
            var pooledObject = pool.Get();
            pooledObject.Name = new(address);
            pooledObject.GameObject = GameObjectPoolManager.Get(pooledObject.Name, out _, parent);
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
            IsDisposed = false;
            Transform = GameObject.transform;
            disposableBag.Clear();
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
        private static readonly PooledKey componentName;
        static PooledComponent()
        {
            componentName = new(typeof(T).FullName);
        }
        internal readonly static _ObjectPool<T> pool = new(() => new());
        public new static void SetMaxSize(int size)
        {
            pool.MaxSize = size;
        }
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
        private const string Prefix = "Prefab";
        private static IPooledMetaData metaData;
        public static PooledKey GetPooledKey(GameObject prefab)
        {
            // append instance id since prefabs may have same name
            return new PooledKey(Prefix, prefab.GetInstanceID());
        }
        /// <summary>
        /// Instantiate pooled component by prefab, optimized version of <see cref="Object.Instantiate(Object, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T Instantiate(GameObject prefab, Transform parent = null)
        {
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.BeginSample(nameof(Instantiate));
#endif
            var pooledComponent = pool.Get();
            PooledKey key = GetPooledKey(prefab);
            pooledComponent.Name = key;
            var @object = GameObjectPoolManager.Get(key, out metaData, parent, createEmptyIfNotExist: false);
            if (!@object)
            {
                @object = Object.Instantiate(prefab, parent);
            }
            pooledComponent.Cache = metaData as ComponentCache;
            pooledComponent.GameObject = @object;
            pooledComponent.Init();
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.EndSample();
#endif
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
            disposableBag.Clear();
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
    /// <summary>
    /// Struct based pooled key without allocation
    /// </summary>
    public readonly struct PooledKey
    {
        public readonly string key;
        public readonly string subKey;
        public readonly int id;
        public PooledKey(string key)
        {
            this.key = key;
            subKey = null;
            id = 0;
        }
        public PooledKey(string key, string subKey)
        {
            this.key = key;
            this.subKey = subKey;
            id = 0;
        }
        public PooledKey(string key, int id)
        {
            this.key = key;
            subKey = null;
            this.id = id;
        }
        public readonly bool IsNull()
        {
            if (string.IsNullOrEmpty(key)) return true;
            bool isNull = true;
            isNull &= !string.IsNullOrEmpty(subKey);
            isNull &= id != 0;
            return isNull;
        }
        public readonly override string ToString()
        {
            if (IsNull()) return string.Empty;
            if (string.IsNullOrEmpty(subKey))
            {
                if (id != 0)
                    return $"{key} {id}";
                return key;
            }
            if (id != 0)
                return $"{key} {subKey} {id}";
            return $"{key} {subKey}";
        }
        public class Comparer : IEqualityComparer<PooledKey>
        {
            public bool Equals(PooledKey x, PooledKey y)
            {
                return x.id == y.id && x.key == y.key && x.subKey == y.subKey;
            }

            public int GetHashCode(PooledKey key)
            {
                return HashCode.Combine(key.id, key.key, key.subKey);
            }
        }
    }
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
        private readonly Dictionary<PooledKey, GameObjectPool> poolDic = new(new PooledKey.Comparer());
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
        public static GameObject Get(PooledKey address, out IPooledMetaData pooledMetaData, Transform parent = null, bool createEmptyIfNotExist = true)
        {
            GameObject obj = null;
            pooledMetaData = null;
            if (Instance.poolDic.TryGetValue(address, out GameObjectPool poolData) && poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent, out pooledMetaData);
            }
            else if (createEmptyIfNotExist)
            {
                obj = new GameObject(address.ToString());
            }
            return obj;
        }
        /// <summary>
        /// Release gameObject to pool
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="address"></param>
        /// <param name="pooledMetaData"></param>
        public static void Release(GameObject obj, PooledKey address = default, IPooledMetaData pooledMetaData = null)
        {
            if (address.IsNull())
                address = new(obj.name);
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
        public static void ReleasePool(PooledKey address)
        {
            if (Instance.poolDic.TryGetValue(address, out var pool))
            {
                Destroy(pool.fatherObj);
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
            public readonly PooledKey key;
            public readonly GameObject fatherObj;
            public readonly Queue<GameObject> poolQueue = new();
            private readonly Dictionary<GameObject, IPooledMetaData> metaData = new();
            public GameObjectPool(PooledKey address, Transform poolRoot)
            {
                fatherObj = new GameObject((key = address).ToString());
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