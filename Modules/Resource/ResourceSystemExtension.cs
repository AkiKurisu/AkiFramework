#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Runtime.CompilerServices;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Kurisu.Framework.Resource
{
    public static class ResourceSystemExtension
    {
        public static CallBackDisposable ToDisposable<T>(this ResourceHandle<T> handle)
        {
            if (handle.operationType == ResourceSystem.InstantiateOperation && typeof(T) == typeof(GameObject))
                return new CallBackDisposable(() => ResourceSystem.ReleaseInstance(handle.Result as GameObject));
            else
                return new CallBackDisposable(() => ResourceSystem.ReleaseAsset(handle));
        }
        public static CallBackDisposable ToDisposable(this ResourceHandle handle)
        {
            if (handle.operationType == ResourceSystem.InstantiateOperation && handle.Result.GetType() == typeof(GameObject))
                return new CallBackDisposable(() => ResourceSystem.ReleaseInstance(handle.Result as GameObject));
            else
                return new CallBackDisposable(() => ResourceSystem.ReleaseAsset(handle));
        }
        public static ResourceHandle<T> AddTo<T>(this ResourceHandle<T> handle, IUnRegister unRegister)
        {
            handle.ToDisposable().AddTo(unRegister);
            return handle;
        }
        public static ResourceHandle<T> Add<T>(this ResourceHandle<T> handle, IUnRegister unRegister)
        {
            handle.ToDisposable().AddTo(unRegister);
            return handle;
        }
        public static ResourceHandle AddTo(this ResourceHandle handle, IUnRegister unRegister)
        {
            handle.ToDisposable().AddTo(unRegister);
            return handle;
        }
        public static ResourceHandle<T> AddTo<T>(this ResourceHandle<T> handle, GameObject unRegisterGameObject)
        {
            handle.ToDisposable().AddTo(unRegisterGameObject);
            return handle;
        }
        public static ResourceHandle AddTo(this ResourceHandle handle, GameObject unRegisterGameObject)
        {
            handle.ToDisposable().AddTo(unRegisterGameObject);
            return handle;
        }
#if UNITASK_SUPPORT
        public static UniTask<T>.Awaiter GetAwaiter<T>(this ResourceHandle<T> handle)
        {
            return handle.InternalHandle.GetAwaiter();
        }
        public static UniTask.Awaiter GetAwaiter(this ResourceHandle handle)
        {
            return handle.InternalHandle.GetAwaiter();
        }
#else
        public static TaskAwaiter<T> GetAwaiter<T>(this ResourceHandle<T> handle)
        {
            return handle.Task.GetAwaiter();
        }
        public static TaskAwaiter GetAwaiter(this ResourceHandle handle)
        {
            return handle.Task.GetAwaiter();
        }
#endif
        /// <summary>
        /// Whether resource handle is empty
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsNull(this ResourceHandle handle)
        {
            return handle.handleID <= 0;
        }
        /// <summary>
        /// Whether resource handle is empty
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this ResourceHandle<T> handle)
        {
            return handle.handleID <= 0;
        }
        /// <summary>
        /// Whether internal operation is valid
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsValid(this ResourceHandle handle)
        {
            return ResourceSystem.IsValid(handle.handleID);
        }
        /// <summary>
        /// Whether internal operation is valid
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsValid<T>(this ResourceHandle<T> handle)
        {
            return ResourceSystem.IsValid(handle.handleID);
        }
        /// <summary>
        /// Whether internal operation is done
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsDone(this ResourceHandle handle)
        {
            return ResourceSystem.IsValid(handle.handleID) && handle.InternalHandle.IsDone;
        }
        /// <summary>
        /// Whether internal operation is done
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsDone<T>(this ResourceHandle<T> handle)
        {
            return ResourceSystem.IsValid(handle.handleID) && handle.InternalHandle.IsDone;
        }
        /// <summary>
        /// Convert to <see cref="ResourceHandle{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetReferenceT"></param>
        /// <returns></returns>
        public static ResourceHandle<T> AsyncLoadAsset<T>(this AssetReferenceT<T> assetReferenceT) where T : Object
        {
            return ResourceSystem.CreateHandle(assetReferenceT.LoadAssetAsync(), ResourceSystem.AssetLoadOperation);
        }
    }
}
