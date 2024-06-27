using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kurisu.Framework.Pool;
using Kurisu.Framework.React;
using Kurisu.Framework.Schedulers;
using R3;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Resource
{
    // TODO: Add fx preloading
    public static class FXSystem
    {
        /// <summary>
        /// Play particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        public static void PlayFX(string address, Transform parent)
        {
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
            GameObjectPoolManager.ReleasePool(PooledParticleSystem.GetFullPath(address));
        }
        /// <summary>
        /// Async instantiate pooled particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Transform parent)
        {
            return await PooledParticleSystem.Get(address, parent);
        }
        /// <summary>
        /// Async instantiate pooled particle system by address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="useLocalPosition"></param>
        /// <returns></returns>
        public static async UniTask<PooledParticleSystem> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            var pooledFX = await PooledParticleSystem.Get(address, parent);
            if (useLocalPosition)
                pooledFX.GameObject.transform.SetLocalPositionAndRotation(position, rotation);
            else
                pooledFX.GameObject.transform.SetPositionAndRotation(position, rotation);
            return pooledFX;
        }
        /// <summary>
        /// Instantiate pooled particle system by prefab, optimized version of <see cref="Object.Instantiate(Object, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static PooledParticleSystem Instantiate(GameObject prefab, Transform parent)
        {
            return PooledParticleSystem.Get(prefab, parent);
        }
        /// <summary>
        /// Instantiate pooled particle system by prefab, optimized version of <see cref="Object.Instantiate(Object, Vector3, Quaternion, Transform)"/> 
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <param name="useLocalPosition">Whether use local position instead of world position, default is false</param>
        /// <returns></returns>
        public static PooledParticleSystem Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool useLocalPosition = false)
        {
            var pooledFX = PooledParticleSystem.Get(prefab, parent);
            if (useLocalPosition)
                pooledFX.GameObject.transform.SetLocalPositionAndRotation(position, rotation);
            else
                pooledFX.GameObject.transform.SetPositionAndRotation(position, rotation);
            return pooledFX;
        }
        private static async UniTask PlayFXAsync(string address, Vector3 position, Quaternion rotation, Transform parent, bool useLocalPosition)
        {
            var pooledFX = await InstantiateAsync(address, position, rotation, parent, useLocalPosition);
            pooledFX.Play();
        }
        public sealed class PooledParticleSystem : PooledComponent<PooledParticleSystem, ParticleSystem>
        {
            public static string GetFullPath(string address)
            {
                return $"FX {address}";
            }
            public static string GetFullPath(GameObject prefab)
            {
                // append instance id since prefabs may have same name
                return $"FX {prefab.name} {prefab.GetInstanceID()}";
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
            public static PooledParticleSystem Get(GameObject prefab, Transform parent)
            {
                var pooledParticleSystem = pool.Get();
                string fullPath = GetFullPath(prefab);
                pooledParticleSystem.Name = fullPath;
                var fxObject = GameObjectPoolManager.Get(fullPath, parent, createEmptyIfNotExist: false);
                if (!fxObject)
                {
                    fxObject = Object.Instantiate(prefab, parent);
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
            public void Play(bool releaseOnEnd = true)
            {
                if (releaseOnEnd && !Component.main.loop)
                {
                    // Push particle system to pool manager after particle system end
                    Destroy(GetDuration());
                }
                if (Component.isPlaying) Component.Stop();
                Component.Play();
            }
            /// <summary>
            /// Get accurate duration compared to main.duration
            /// </summary>
            /// <returns></returns>
            public float GetDuration()
            {
                var particles = GameObject.GetComponentsInChildren<ParticleSystem>();
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
