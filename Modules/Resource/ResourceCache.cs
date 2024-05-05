using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
namespace Kurisu.Framework.Resource
{
    public class InvalidResourceRequestException : Exception
    {
        public InvalidResourceRequestException() : base() { }
        public InvalidResourceRequestException(string message) : base(message) { }
    }
    /// <summary>
    /// Loading and cache specific asset as a group and release them by control version
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class ResourceCache<TAsset> : IDisposable where TAsset : UnityEngine.Object
    {
        private readonly Dictionary<string, ResourceHandle<TAsset>> internalHandles = new();
        private readonly Dictionary<string, TAsset> cacheMap = new();
        private readonly Dictionary<string, int> versionMap = new();
        /// <summary>
        /// Validate asset location before loading, throw <see cref="InvalidResourceRequestException"/> if not exist
        /// </summary>
        /// <value></value>
        public bool SafeAddressCheck { get; set; } = false;
        public int Version { get; private set; } = 0;
#if UNITASK_SUPPORT
        public async UniTask<TAsset> LoadAssetAsync(string address)
#else
        public async Task<TAsset> LoadAssetAsync(string address)
#endif
        {
            versionMap[address] = Version;
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                asset = await LoadNewAssetAsync(address);
            }
            return asset;
        }
        public TAsset LoadAsset(string address)
        {
            versionMap[address] = Version;
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                asset = LoadNewAssetAsync(address).WaitForCompletion();
            }
            return asset;
        }
        private ResourceHandle<TAsset> LoadNewAssetAsync(string address, Action<TAsset> callBack = null)
        {
            if (internalHandles.TryGetValue(address, out var internalHandle))
            {
                if (internalHandle.IsDone())
                {
                    callBack?.Invoke(internalHandle.Result);
                    return internalHandle;
                }
                else
                {
                    internalHandle.RegisterCallBack(callBack);
                }
                return internalHandle;
            }
            if (SafeAddressCheck)
            {
                var location = Addressables.LoadResourceLocationsAsync(address, typeof(TAsset));
                location.WaitForCompletion();
                if (location.Status != AsyncOperationStatus.Succeeded || location.Result.Count == 0)
                {
                    throw new InvalidResourceRequestException($"Address {address} not valid for loading {typeof(TAsset)} asset");
                }
            }
            //Create a new resource load call, also track it's handle
            internalHandle = ResourceSystem.AsyncLoadAsset<TAsset>(address, (asset) =>
            {
                cacheMap.Add(address, asset);
                callBack?.Invoke(asset);
            });
            internalHandles.Add(address, internalHandle);
            return internalHandle;
        }
        /// <summary>
        /// Implementation of <see cref="IDisposable"/>, release all handles in cache.
        /// </summary>
        public void Dispose()
        {
            foreach (var handle in internalHandles.Values)
            {
                ResourceSystem.ReleaseAsset(handle);
            }
            internalHandles.Clear();
            cacheMap.Clear();
            versionMap.Clear();
        }
        /// <summary>
        /// Get cache addresses
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCacheKeys() => cacheMap.Keys;
        /// <summary>
        /// Update version
        /// </summary>
        public int UpdateVersion() => ++Version;
        /// <summary>
        /// Release all assets with target version
        /// </summary>
        /// <param name="version"></param>
        public void ReleaseAssetsWithVersion(int version)
        {
            versionMap.Where(p => p.Value == version).Select(p => p.Key).ToList().ForEach(ads =>
            {
                if (internalHandles.TryGetValue(ads, out var handle))
                    ResourceSystem.ReleaseAsset(handle);
                cacheMap.Remove(ads);
                internalHandles.Remove(ads);
                versionMap.Remove(ads);
            });
        }
        /// <summary>
        /// Release assets with last version and update version
        /// </summary>
        public void ReleaseAssetsAndUpdateVersion()
        {
            ReleaseAssetsWithVersion(Version);
            UpdateVersion();
        }
    }
}