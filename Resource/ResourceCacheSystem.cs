using System;
using System.Collections.Generic;
namespace Kurisu.Framework.Resource
{
    /// <summary>
    /// System to loading and cache specific asset
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class ResourceCacheSystem<TAsset> : IDisposable where TAsset : UnityEngine.Object
    {
        private readonly Dictionary<string, ResourceHandle<TAsset>> internalHandles = new();
        private readonly Dictionary<string, TAsset> cacheMap = new();
        public bool TryGetAsset(string address, out TAsset asset)
        {
            return cacheMap.TryGetValue(address, out asset);
        }
        public ResourceHandle<TAsset> LoadAssetAsync(string address, Action<TAsset> callBack = null)
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
            }
            //Create a new resource load call, also track it's handle
            var handle = ResourceSystem.AsyncLoadAsset<TAsset>(address, (asset) =>
            {
                cacheMap.Add(address, asset);
                callBack?.Invoke(asset);
            });
            internalHandles.Add(address, handle);
            return handle;
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