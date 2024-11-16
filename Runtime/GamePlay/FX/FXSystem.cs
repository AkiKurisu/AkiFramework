using Cysharp.Threading.Tasks;
using Kurisu.Framework.Pool;
using Kurisu.Framework.React;
using Kurisu.Framework.Resource;
using R3;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.FX
{
    // TODO: Add fx preloading
    public static class FXSystem
    {
        /// <summary>
        /// Validate asset location before loading, throw <see cref="InvalidResourceRequestException"/> if not exist
        /// </summary>
        /// <value></value>
        public static bool AddressSafeCheck { get; set; } = false;
        /// <summary>
        /// Play particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        public static void PlayFX(string address, Transform parent)
        {
            if (AddressSafeCheck)
                ResourceSystem.CheckAsset<GameObject>(address);
            PlayFXAsync(address, Vector3.zero, Quaternion.identity, parent, true).Forget();
        }
        /// <summary>
        /// Play particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="useLocalPosition"></param>
        public static void PlayFX(string address, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            if (AddressSafeCheck)
                ResourceSystem.CheckAsset<GameObject>(address);
            PlayFXAsync(address, position, rotation, parent, useLocalPosition).Forget();
        }
        /// <summary>
        /// Play particle system by prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        public static void PlayFX(GameObject prefab, Transform parent)
        {
            Instantiate(prefab, Vector3.zero, Quaternion.identity, parent, true).Play();
        }
        /// <summary>
        /// Play particle system by prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="useLocalPosition"></param>
        public static void PlayFX(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            Instantiate(prefab, position, rotation, parent, useLocalPosition).Play();
        }
        /// <summary>
        /// Release particle system
        /// </summary>
        /// <param name="address"></param>
        public static void ReleaseFX(string address)
        {
            GameObjectPoolManager.ReleasePool(PooledParticleSystem.GetPooledKey(address));
        }
        /// <summary>
        /// Async instantiate pooled particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Transform parent)
        {
            if (AddressSafeCheck)
                await ResourceSystem.CheckAssetAsync<GameObject>(address);
            return await PooledParticleSystem.InstantiateAsync(address, parent);
        }
        /// <summary>
        /// Async instantiate pooled particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent">The parent attached to. If parent exists, it will use prefab's scale as local scale instead of lossy scale</param>
        /// <param name="useLocalPosition">Whether use local position instead of world position, default is false</param>
        /// <returns></returns>
        public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            return await PooledParticleSystem.InstantiateAsync(address, position, rotation, parent, useLocalPosition);
        }
        /// <summary>
        /// Instantiate pooled particle system by prefab, optimized version of <see cref="Object.Instantiate(Object, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static PooledParticleSystem Instantiate(GameObject prefab, Transform parent)
        {
            return PooledParticleSystem.Instantiate(prefab, parent);
        }
        /// <summary>
        /// Instantiate pooled particle system by prefab, optimized version of <see cref="Object.Instantiate(Object, Vector3, Quaternion, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent">The parent attached to. If parent exists, it will use prefab's scale as local scale instead of lossy scale</param>
        /// <param name="useLocalPosition">Whether use local position instead of world position, default is false</param>
        /// <returns></returns>
        public static PooledParticleSystem Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            return PooledParticleSystem.Instantiate(prefab, position, rotation, parent, useLocalPosition);
        }
        private static async UniTask PlayFXAsync(string address, Vector3 position, Quaternion rotation, Transform parent, bool useLocalPosition)
        {
            var pooledFX = await InstantiateAsync(address, position, rotation, parent, useLocalPosition);
            pooledFX.Play();
        }
        public sealed class PooledParticleSystem : PooledComponent<PooledParticleSystem, ParticleSystem>
        {
            public new class ComponentCache : PooledComponent<PooledParticleSystem, ParticleSystem>.ComponentCache
            {
                /// <summary>
                /// Particle system total duration
                /// </summary>
                public float duration;
            }
            private const string key = "FX";
            public static PoolKey GetPooledKey(string address)
            {
                // append prefix since different type UObjects can have same address
                return new PoolKey(key, address);
            }
            public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Transform parent)
            {
                var pooledParticleSystem = pool.Get();
                PoolKey key = GetPooledKey(address);
                pooledParticleSystem.PoolKey = key;
                var fxObject = GameObjectPoolManager.Get(key, out var metaData, parent, createEmptyIfNotExist: false);
                if (!fxObject)
                {
                    var handle = ResourceSystem.InstantiateAsync(address, parent);
                    fxObject = await handle;
                    // decrease ref count when pool manager release root
                    _ = handle.AddTo(fxObject);
                }
                pooledParticleSystem.GameObject = fxObject;
                pooledParticleSystem.Cache = metaData as ComponentCache;
                pooledParticleSystem.Init();
                return pooledParticleSystem;
            }
            public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
            {
                var pooledFX = await InstantiateAsync(address, parent);
                if (useLocalPosition)
                    pooledFX.GameObject.transform.SetLocalPositionAndRotation(position, rotation);
                else
                    pooledFX.GameObject.transform.SetPositionAndRotation(position, rotation);
                return pooledFX;
            }
            protected sealed override void Init()
            {
                IsDisposed = false;
                InitDisposables();
                Transform = GameObject.transform;
                Cache ??= new ComponentCache();
                if (!Cache.component)
                {
                    var particles = GameObject.GetComponentsInChildren<ParticleSystem>();
                    Assert.IsTrue(particles.Length > 0);
                    Cache.component = particles[0];
                    ((ComponentCache)Cache).duration = particles.GetDuration();
                }
                Assert.IsNotNull(Component);
            }
            public void Play(bool releaseOnEnd = true)
            {
                if (releaseOnEnd && !Component.main.loop)
                {
                    // Push particle system to pool manager after particle system end
                    Destroy(((ComponentCache)Cache).duration);
                }
                if (Component.isPlaying) Component.Stop();
                Component.Play();
            }
            public void Stop(bool release = true)
            {
                Component.Stop();
                if (release) Dispose();
            }
        }
    }
    // Reference: https://blog.csdn.net/ls9512/article/details/103815387
    public static class ParticleSystemExtension
    {
        /// <summary>
        /// Get accurate duration compared to main.duration
        /// </summary>
        /// <param name="particles"></param>
        /// <returns></returns>
        public static float GetDuration(this ParticleSystem[] particles)
        {
            var duration = -1f;
            for (var i = 0; i < particles.Length; i++)
            {
                var ps = particles[i];
                var time = ps.GetDuration(false);
                if (time > duration)
                {
                    duration = time;
                }
            }
            return duration;
        }
        private static float GetMaxValue(this ParticleSystem.MinMaxCurve minMaxCurve)
        {
            switch (minMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return minMaxCurve.curve.GetMaxValue();
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMax;
                case ParticleSystemCurveMode.TwoCurves:
                    var ret1 = minMaxCurve.curveMin.GetMaxValue();
                    var ret2 = minMaxCurve.curveMax.GetMaxValue();
                    return ret1 > ret2 ? ret1 : ret2;
            }
            return -1f;
        }
        private static float GetMinValue(this ParticleSystem.MinMaxCurve minMaxCurve)
        {
            switch (minMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return minMaxCurve.curve.GetMinValue();
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMin;
                case ParticleSystemCurveMode.TwoCurves:
                    var ret1 = minMaxCurve.curveMin.GetMinValue();
                    var ret2 = minMaxCurve.curveMax.GetMinValue();
                    return ret1 < ret2 ? ret1 : ret2;
            }
            return -1f;
        }
        private static float GetMaxValue(this AnimationCurve curve)
        {
            var ret = float.MinValue;
            var frames = curve.keys;
            for (var i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                var value = frame.value;
                if (value > ret)
                {
                    ret = value;
                }
            }

            return ret;
        }
        private static float GetMinValue(this AnimationCurve curve)
        {
            var ret = float.MaxValue;
            var frames = curve.keys;
            for (var i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                var value = frame.value;
                if (value < ret)
                {
                    ret = value;
                }
            }
            return ret;
        }
        public static float GetDuration(this ParticleSystem particle, bool allowLoop = false)
        {
            if (!particle.emission.enabled) return 0f;
            if (particle.main.loop && !allowLoop)
            {
                return -1f;
            }
            if (particle.emission.rateOverTime.GetMinValue() <= 0)
            {
                return particle.main.startDelay.GetMaxValue() + particle.main.startLifetime.GetMaxValue();
            }
            else
            {
                return particle.main.startDelay.GetMaxValue() + Mathf.Max(particle.main.duration, particle.main.startLifetime.GetMaxValue());
            }
        }
    }
}
