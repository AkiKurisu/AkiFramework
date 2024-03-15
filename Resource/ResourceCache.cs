using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public bool TryGetAsset(string address, out TAsset asset)
        {
            return cacheMap.TryGetValue(address, out asset);
        }
        public async Task<TAsset> LoadAssetAsync(string address)
        {
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                asset = await LoadNewAssetAsync(address).Task;
            }
            return asset;
        }
        public TAsset LoadAsset(string address)
        {
            if (!cacheMap.TryGetValue(address, out TAsset asset))
            {
                asset = LoadNewAssetAsync(address).WaitForCompletion();
            }
            return asset;
        }
        public ResourceHandle<TAsset> LoadNewAssetAsync(string address, Action<TAsset> callBack = null)
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

    }
}