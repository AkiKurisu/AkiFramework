using System;
using System.Collections.Generic;
using System.Linq;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
namespace Kurisu.Framework.Resource
{
    /// <summary>
    /// Loading and cache specific asset
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class ResourceCache<TAsset> : IDisposable where TAsset : UnityEngine.Object
    {
        private readonly Dictionary<string, ResourceHandle<TAsset>> internalHandles = new();
        private readonly Dictionary<string, TAsset> cacheMap = new();
        private readonly Dictionary<string, int> versionMap = new();
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
            //Create a new resource load call, also track it's handle
            internalHandle = ResourceSystem.AsyncLoadAsset<TAsset>(address, (asset) =>
            {
                cacheMap.Add(address, asset);
                callBack?.Invoke(asset);
            });
            internalHandles.Add(address, internalHandle);
            return internalHandle;
        }
        public void Dispose()
        {
            foreach (var handle in internalHandles.Values)
            {
                ResourceSystem.ReleaseAsset(handle);
            }
        }
        public IEnumerable<string> GetCacheAssetsAddress() => cacheMap.Keys;
        public void UpdateVersion() => ++Version;
        public void ReleaseAssetsWithVersion(int version)
        {
            versionMap.Where(p => p.Value == version).Select(p => p.Key).ToList().ForEach(ads =>
            {
                ResourceSystem.ReleaseAsset(internalHandles[ads]);
                cacheMap.Remove(ads);
                internalHandles.Remove(ads);
                versionMap.Remove(ads);
            });
        }
    }
}