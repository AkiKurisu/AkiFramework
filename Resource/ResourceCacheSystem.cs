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
        private readonly Dictionary<string, Action<TAsset>> loadQueue = new();
        protected virtual string AddressFormat(string inputAddress)
        {
            return inputAddress;
        }
        public bool TryGetAsset(string address, out TAsset asset)
        {
            return cacheMap.TryGetValue(address, out asset);
        }
        public ResourceHandle<TAsset> LoadAssetAsync(string address, Action<TAsset> callBack)
        {
            //If is loading in back ground then add callBack instead
            if (loadQueue.ContainsKey(address))
            {
                if (callBack != null) loadQueue[address] += callBack;
                return internalHandles[address];
            }
            //Enqueue new loading job
            loadQueue.Add(address, (asset) =>
            {
                loadQueue.Remove(address);
                callBack?.Invoke(asset);
            });
            //Create a new resource load call, also track it's handle
            var handle = ResourceSystem.AsyncLoadAsset<TAsset>(AddressFormat(address), (asset) =>
            {
                cacheMap.Add(address, asset);
                loadQueue[address](asset);
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