using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Kurisu.Framework.Tasks;
using System.Collections;
namespace Kurisu.Framework.Resource
{
    /// <summary>
    /// Simple system to load resource from address and label using addressable
    /// </summary>
    public class ResourceSystem
    {
        /// <summary>
        /// Options for merging the results of requests.
        /// If keys (A, B) mapped to results ([1,2,4],[3,4,5])...
        ///  - UseFirst (or None) takes the results from the first key
        ///  -- [1,2,4]
        ///  - Union takes results of each key and collects items that matched any key.
        ///  -- [1,2,3,4,5]
        ///  - Intersection takes results of each key, and collects items that matched every key.
        ///  -- [4]
        /// </summary>
        public enum MergeMode
        {
            /// <summary>
            /// Use to indicate that no merge should occur. The first set of results will be used.
            /// </summary>
            None = 0,

            /// <summary>
            /// Use to indicate that the merge should take the first set of results.
            /// </summary>
            UseFirst = 0,

            /// <summary>
            /// Use to indicate that the merge should take the union of the results.
            /// </summary>
            Union,

            /// <summary>
            /// Use to indicate that the merge should take the intersection of the results.
            /// </summary>
            Intersection
        }
        #region  Asset Load
        public static ResourceHandle<T> AsyncLoadAsset<T>(string address, Action<T> action, IUnRegisterHandle unRegisterHandle)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            if (action != null)
                handle.Completed += (h) => action.Invoke(h.Result);
            var resourceHandle = CreateHandle(handle);
            if (unRegisterHandle != null)
                resourceHandle.GetUnRegister().AttachUnRegister(unRegisterHandle);
            return resourceHandle;
        }
        public static ResourceHandle<T> AsyncLoadAsset<T>(string address, Action<T> action = null, GameObject bindObject = null)
        {

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            if (action != null)
                handle.Completed += (h) => action.Invoke(h.Result);

            var resourceHandle = CreateHandle(handle);
            if (bindObject != null)
                resourceHandle.GetUnRegister().AttachUnRegister(bindObject);
            return resourceHandle;
        }
        #endregion
        #region Instantiate
        public static ResourceHandle<GameObject> AsyncInstantiate(string address, Transform parent, Action<GameObject> action = null, GameObject bindObject = null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);
            var resourceHandle = CreateHandle(handle);
            if (action != null)
                handle.Completed += (h) => { instanceIDMap.Add(h.Result.GetInstanceID(), resourceHandle.handleID); action.Invoke(h.Result); };
            if (bindObject != null)
                new CustomUnRegister(() => ReleaseInstance(resourceHandle.Result)).AttachUnRegister(bindObject);
            return resourceHandle;
        }
        #endregion
        #region Release
        public static void ReleaseAsset(ResourceHandle handle)
        {
            if (handle.InternalHandle.Equals(null) || handle.InternalHandle.Result != null)
                Addressables.Release(handle.InternalHandle);
            internalHandleMap.Remove(handle.handleID);
        }

        public static void ReleaseInstance(GameObject obj)
        {
            if (instanceIDMap.TryGetValue(obj.GetInstanceID(), out int handleID))
            {
                internalHandleMap.Remove(handleID);
            }
            if (obj != null)
                Addressables.ReleaseInstance(obj);
        }
        #endregion
        #region  Multi Assets Load
        public static ResourceAsyncSequence GetAsyncSequence()
        {
            var sequence = PoolManager.Instance.GetObject<ResourceAsyncSequence>();
            sequence.Reset();
            //Check sequence is completed if no asset was added
            Timer.Schedule(sequence.Check);
            return sequence;
        }
        public static ResourceHandle<IList<T>> AsyncLoadAssets<T>(object key, Action<IList<T>> action, GameObject bindObject = null)
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key, null);
            if (action != null)
                handle.Completed += (h) => action.Invoke(h.Result);
            var resourceHandle = CreateHandle(handle);
            if (bindObject != null)
                resourceHandle.GetUnRegister().AttachUnRegister(bindObject);
            return resourceHandle;
        }
        public static ResourceHandle<IList<T>> AsyncLoadAssets<T>(object key, Action<IList<T>> action, IUnRegisterHandle unRegisterHandle)
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key, null);
            if (action != null)
                handle.Completed += (h) => action.Invoke(h.Result);
            var resourceHandle = CreateHandle(handle);
            if (unRegisterHandle != null)
                resourceHandle.GetUnRegister().AttachUnRegister(unRegisterHandle);
            return resourceHandle;
        }
        public static ResourceHandle<IList<T>> AsyncLoadAssets<T>(IEnumerable key, MergeMode mode, Action<IList<T>> action, IUnRegisterHandle unRegisterHandle)
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(key, null, (Addressables.MergeMode)mode);
            if (action != null)
                handle.Completed += (h) => action.Invoke(h.Result);
            var resourceHandle = CreateHandle(handle);
            if (unRegisterHandle != null)
                resourceHandle.GetUnRegister().AttachUnRegister(unRegisterHandle);
            return resourceHandle;
        }
        #endregion
        /// <summary>
        /// Start from 1 since 0 is always invalid handle
        /// </summary>
        private static int handleIndex = 1;
        private static readonly Dictionary<int, int> instanceIDMap = new();
        private static readonly Dictionary<int, AsyncOperationHandle> internalHandleMap = new();
        private static ResourceHandle<T> CreateHandle<T>(AsyncOperationHandle<T> asyncOperationHandle)
        {
            internalHandleMap.Add(++handleIndex, asyncOperationHandle);
            return new ResourceHandle<T>(handleIndex);
        }
        public static AsyncOperationHandle<T> CastOperationHandle<T>(int handleID)
        {
            if (internalHandleMap.TryGetValue(handleID, out var handle))
            {
                return handle.Convert<T>();
            }
            else
            {
                return default;
            }
        }
        public static AsyncOperationHandle CastOperationHandle(int handleID)
        {
            if (internalHandleMap.TryGetValue(handleID, out var handle))
            {
                return handle;
            }
            else
            {
                return default;
            }
        }
        public static bool IsValid(int handleID)
        {
            return internalHandleMap.TryGetValue(handleID, out _);
        }
    }
}