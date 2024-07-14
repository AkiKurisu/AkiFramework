using UnityEngine;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Pool
{
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
        private static readonly PoolKey componentName;
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
            pooledComponent.PoolKey = componentName;
            pooledComponent.GameObject = GameObjectPoolManager.Get(componentName, out var metaData, parent);
            pooledComponent.Cache = metaData as ComponentCache;
            pooledComponent.Init();
            return pooledComponent;
        }
        private const string Prefix = "Prefab";
        private static IPooledMetaData metaData;
        public static PoolKey GetPooledKey(GameObject prefab)
        {
            // append instance id since prefabs may have same name
            return new PoolKey(Prefix, prefab.GetInstanceID());
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
            PoolKey key = GetPooledKey(prefab);
            pooledComponent.PoolKey = key;
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
            IsDisposed = false;
            InitDisposables();
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
            ReleaseDisposables();
            if (GameObjectPoolManager.IsInstantiated)
                GameObjectPoolManager.Release(GameObject, PoolKey, Cache);
            IsDisposed = true;
            pool.Release((T)this);
        }
        protected virtual void OnDispose() { }
    }
}