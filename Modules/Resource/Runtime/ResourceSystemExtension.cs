using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Kurisu.Framework.Resource
{
    public static class ResourceSystemExtension
    {
        public static UniTask<T>.Awaiter GetAwaiter<T>(this ResourceHandle<T> handle)
        {
            return handle.InternalHandle.GetAwaiter();
        }
        public static UniTask.Awaiter GetAwaiter(this ResourceHandle handle)
        {
            return handle.InternalHandle.GetAwaiter();
        }
        public static UniTask<T> WithCancellation<T>(this ResourceHandle<T> handle, CancellationToken cancellationToken, bool cancelImmediately = false, bool autoReleaseWhenCanceled = false)
        {
            return handle.InternalHandle.WithCancellation(cancellationToken, cancelImmediately, autoReleaseWhenCanceled);
        }
        public static UniTask WithCancellation(this ResourceHandle handle, CancellationToken cancellationToken, bool cancelImmediately = false, bool autoReleaseWhenCanceled = false)
        {
            return handle.InternalHandle.WithCancellation(cancellationToken, cancelImmediately, autoReleaseWhenCanceled);
        }
        /// <summary>
        /// Whether resource handle is empty
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsNull(this ResourceHandle handle)
        {
            return handle.index <= 0;
        }
        /// <summary>
        /// Whether resource handle is empty
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this ResourceHandle<T> handle)
        {
            return handle.index <= 0;
        }
        /// <summary>
        /// Whether internal operation is valid
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsValid(this ResourceHandle handle)
        {
            return ResourceSystem.IsValid(handle.version, handle.index);
        }
        /// <summary>
        /// Whether internal operation is valid
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsValid<T>(this ResourceHandle<T> handle)
        {
            return ResourceSystem.IsValid(handle.version, handle.index);
        }
        /// <summary>
        /// Whether internal operation is done
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsDone(this ResourceHandle handle)
        {
            return ResourceSystem.IsValid(handle.version, handle.index) && handle.InternalHandle.IsDone;
        }
        /// <summary>
        /// Whether internal operation is done
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsDone<T>(this ResourceHandle<T> handle)
        {
            return ResourceSystem.IsValid(handle.version, handle.index) && handle.InternalHandle.IsDone;
        }
        /// <summary>
        /// Async load asset by <see cref="AssetReferenceT{T}"/> and convert operation to <see cref="ResourceHandle{T}"/>
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
