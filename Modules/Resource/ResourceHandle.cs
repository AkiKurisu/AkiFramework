using System;
#if AF_UNITASK_INSTALL
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Kurisu.Framework.Resource
{
    /// <summary>
    /// A light weight replacement of <see cref="AsyncOperationHandle"/>
    /// </summary>
    public readonly struct ResourceHandle : IEquatable<ResourceHandle>, IDisposable
    {
        internal readonly uint handleID;
        internal readonly byte operationType;
        internal readonly AsyncOperationHandle InternalHandle => ResourceSystem.CastOperationHandle(handleID);
        public readonly object Result => InternalHandle.Result;
#if AF_UNITASK_INSTALL
        public readonly UniTask Task => InternalHandle.ToUniTask();
#else
        public readonly Task Task => InternalHandle.Task;
#endif
        public ResourceHandle(uint handleID, byte operationType)
        {
            this.handleID = handleID;
            this.operationType = operationType;
        }
        /// <summary>
        /// Register completed result callback, no need to unregister since delegate list is clear after fire event
        /// </summary>
        /// <param name="callBack"></param>
        public readonly void RegisterCallBack(Action callBack)
        {
            InternalHandle.Completed += (h) => callBack?.Invoke();
        }
        /// <summary>
        /// Register completed result callback, no need to unregister since delegate list is clear after fire event
        /// </summary>
        /// <returns></returns>
        public readonly object WaitForCompletion()
        {
            return InternalHandle.WaitForCompletion();
        }
        public ResourceHandle<T> Convert<T>()
        {
            return new ResourceHandle<T>(handleID, operationType);
        }
        public bool Equals(ResourceHandle other)
        {
            return other.handleID == handleID && other.InternalHandle.Equals(InternalHandle);
        }
        /// <summary>
        /// Implement of <see cref="IDisposable"/> to release resource
        /// </summary>
        public void Dispose()
        {
            ResourceSystem.Release(this);
        }
    }
    /// <summary>
    /// A light weight replacement of <see cref="AsyncOperationHandle{T}"/>
    /// </summary>
    public readonly struct ResourceHandle<T> : IEquatable<ResourceHandle<T>>, IDisposable
    {
        internal readonly uint handleID;
        internal readonly byte operationType;
        internal readonly AsyncOperationHandle<T> InternalHandle => ResourceSystem.CastOperationHandle<T>(handleID);
        public readonly T Result => InternalHandle.Result;
#if AF_UNITASK_INSTALL
        public readonly UniTask<T> Task => InternalHandle.ToUniTask();
#else
        public readonly Task<T> Task => InternalHandle.Task;
#endif
        public ResourceHandle(uint handleID, byte operationType)
        {
            this.handleID = handleID;
            this.operationType = operationType;
        }
        public static implicit operator ResourceHandle(ResourceHandle<T> obj)
        {
            return new ResourceHandle(obj.handleID, obj.operationType);
        }
        /// <summary>
        /// Register completed result callback, no need to unregister since delegate list is clear after fire event
        /// </summary>
        /// <param name="callBack"></param>
        public readonly void RegisterCallBack(Action<T> callBack)
        {
            InternalHandle.Completed += (h) => callBack?.Invoke(h.Result);
        }
        /// <summary>
        /// Register completed result callback, no need to unregister since delegate list is clear after fire event
        /// </summary>
        /// <param name="callBack"></param>
        public readonly void RegisterCallBack(Action callBack)
        {
            InternalHandle.Completed += (h) => callBack?.Invoke();
        }
        public readonly T WaitForCompletion()
        {
            return InternalHandle.WaitForCompletion();
        }

        public bool Equals(ResourceHandle<T> other)
        {
            return other.handleID == handleID && other.InternalHandle.Equals(InternalHandle);
        }
        /// <summary>
        /// Implement of <see cref="IDisposable"/> to release resource
        /// </summary>
        public void Dispose()
        {
            ResourceSystem.Release(this);
        }
    }
}
