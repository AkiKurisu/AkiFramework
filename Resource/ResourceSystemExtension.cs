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
        public static void AddTo(this ResourceHandle handle, ResourceAsyncSequence sequence)
        {
            sequence.Add(handle.InternalHandle);
        }
        public static void AddTo<T>(this ResourceHandle<T> handle, ResourceAsyncSequence sequence) where T : Object
        {
            sequence.Add(handle.InternalHandle);
        }
        internal static CustomUnRegister GetUnRegister<T>(this ResourceHandle<T> handle)
        {
            return new CustomUnRegister(() => ResourceSystem.ReleaseAsset(handle));
        }
        internal static CustomUnRegister GetUnRegister(this ResourceHandle handle)
        {
            return new CustomUnRegister(() => ResourceSystem.ReleaseAsset(handle));
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
        /// Load asset sync, should be careful if is used in Awake() or Start() when scene is also loaded by Addressables,
        /// which may block your whole game!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetReferenceT"></param>
        /// <returns></returns>
        public static T GetAssetSync<T>(this AssetReferenceT<T> assetReferenceT) where T : Object
        {
            return assetReferenceT.LoadAssetAsync().WaitForCompletion();
        }
    }
}
