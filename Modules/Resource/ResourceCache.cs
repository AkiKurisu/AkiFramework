using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
#if AF_UNITASK_INSTALL
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
namespace Kurisu.Framework.Resource
{
    public class InvalidResourceRequestException : Exception
    {
        public string InvalidAddress { get; }
        public InvalidResourceRequestException() : base() { }
        public InvalidResourceRequestException(string address, string message) : base(message) { InvalidAddress = address; }
    }
    /// <summary>
    /// Loading and cache specific asset as a group and release them by control version
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class ResourceCache<TAsset> : IDisposable, IReadOnlyDictionary<string, TAsset> where TAsset : UnityEngine.Object
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
        public IEnumerable<string> Keys => cacheMap.Keys;
        public IEnumerable<TAsset> Values => cacheMap.Values;
        public int Count => cacheMap.Count;
        public TAsset this[string key] => cacheMap[key];

        /// <summary>
        /// Load and cache asset async
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
#if AF_UNITASK_INSTALL
        public async UniTask<TAsset> LoadAssetAsync(string address)
#else
        public async Task<TAsset> LoadAssetAsync(string address)
#endif
        {
            versionMap[address] = Version;
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                if (SafeAddressCheck)
                    await SafeCheckAsync(address);
                asset = await LoadNewAssetAsync(address);
            }
            return asset;
        }
        /// <summary>
        /// Load and cache asset in sync way which will block game, not recommend
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public TAsset LoadAsset(string address)
        {
            versionMap[address] = Version;
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                if (SafeAddressCheck)
                    SafeCheck(address);
                asset = LoadNewAssetAsync(address).WaitForCompletion();
            }
            return asset;
        }
#if AF_UNITASK_INSTALL
        private async UniTask SafeCheckAsync(string address)
#else
        private async Task SafeCheckAsync(string address)
#endif
        {
            //No need when global safe check is on
#if !AF_RESOURCES_SAFE_CHECK
            var location = Addressables.LoadResourceLocationsAsync(address, typeof(TAsset));
#if AF_UNITASK_INSTALL
            await location.ToUniTask();
#else
            await location.Task;
#endif
            if (location.Status != AsyncOperationStatus.Succeeded || location.Result.Count == 0)
            {
                throw new InvalidResourceRequestException(address, $"Address {address} not valid for loading {typeof(TAsset)} asset");
            }
#endif
        }
        private void SafeCheck(string address)
        {
            //No need when global safe check is on
#if !AF_RESOURCES_SAFE_CHECK
            var location = Addressables.LoadResourceLocationsAsync(address, typeof(TAsset));
            location.WaitForCompletion();
            if (location.Status != AsyncOperationStatus.Succeeded || location.Result.Count == 0)
            {
                throw new InvalidResourceRequestException(address, $"Address {address} not valid for loading {typeof(TAsset)} asset");
            }
#endif
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
        public bool ContainsKey(string key)
        {
            return cacheMap.ContainsKey(key);
        }
        public bool TryGetValue(string key, out TAsset value)
        {
            return cacheMap.TryGetValue(key, out value);
        }
        public IEnumerator<KeyValuePair<string, TAsset>> GetEnumerator()
        {
            return cacheMap.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return cacheMap.GetEnumerator();
        }
    }
}