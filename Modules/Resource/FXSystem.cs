using Cysharp.Threading.Tasks;
using Kurisu.Framework.Pool;
using Kurisu.Framework.React;
using Kurisu.Framework.Schedulers;
using R3;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework.Resource
{
    // TODO: Add fx preloading
    public static class FXSystem
    {
        /// <summary>
        /// Play particle system
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        public static void PlayFX(string address, Transform parent)
        {
            PlayFXAsync(address, parent).Forget();
        }
        /// <summary>
        /// Play particle system with offset
        /// </summary>
        /// <param name="address"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        public static void PlayFX(string address, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            PlayFXAsync(address, position, rotation, parent).Forget();
        }
        /// <summary>
        /// Release particle system
        /// </summary>
        /// <param name="address"></param>
        public static void ReleaseFX(string address)
        {
            GameObjectPoolManager.ReleasePool(PooledParticleSystem.GetFullPath(address));
        }
        private static async UniTask PlayFXAsync(string address, Transform parent)
        {
            var pooledFX = await PooledParticleSystem.Get(address, parent);
            pooledFX.Play();
        }
        private static async UniTask PlayFXAsync(string address, Vector3 position, Quaternion rotation, Transform parent)
        {
            var pooledFX = await PooledParticleSystem.Get(address, parent);
            pooledFX.GameObject.transform.SetPositionAndRotation(position, rotation);
            pooledFX.Play();
        }
        private sealed class PooledParticleSystem : PooledComponent<PooledParticleSystem, ParticleSystem>
        {
            public static string GetFullPath(string address)
            {
                return $"FX {address}";
            }
            public new static async UniTask<PooledParticleSystem> Get(string address, Transform parent)
            {
                var pooledParticleSystem = pool.Get();
                string fullPath = GetFullPath(address);
                pooledParticleSystem.Name = fullPath;
                var fxObject = GameObjectPoolManager.Get(fullPath, parent, createEmptyIfNotExist: false);
                if (!fxObject)
                {
                    var handle = ResourceSystem.AsyncInstantiate(address, parent);
                    fxObject = await handle;
                    // decrease ref count when pool manager release root
                    _ = handle.AddTo(fxObject);
                }
                pooledParticleSystem.GameObject = fxObject;
                pooledParticleSystem.Init();
                return pooledParticleSystem;
            }
            protected sealed override void Init()
            {
                LocalInit();
            }
            private void LocalInit()
            {
                IsDisposed = false;
                disposableBag = new();
                // Allow add a pivot
                Component = GameObject.GetComponentInChildren<ParticleSystem>();
                Assert.IsNotNull(Component);
            }
            public void Play()
            {
                if (!Component.main.loop)
                {
                    // Push particle system to pool manager after particle system end
                    Scheduler.Delay(Component.main.duration, Dispose).AddTo(this);
                }
                Component.Play();
            }
        }
    }
}
