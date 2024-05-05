using System;
#if UNITASK_SUPPORT
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
    public readonly struct ResourceHandle : IEquatable<ResourceHandle>
    {
        internal readonly int handleID;
        internal readonly int operationType;
        internal readonly AsyncOperationHandle InternalHandle => ResourceSystem.CastOperationHandle(handleID);
        public readonly object Result => InternalHandle.Result;
#if UNITASK_SUPPORT
        public readonly UniTask Task => InternalHandle.ToUniTask();
#else
        public readonly Task Task => InternalHandle.Task;
#endif
        public ResourceHandle(int handleID, int operationType)
        {
            this.handleID = handleID;
            this.operationType = operationType;
        }
        public readonly void RegisterCallBack(Action callBack)
        {
            InternalHandle.Completed += (h) => callBack?.Invoke();
        }
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
    }
    /// <summary>
    /// A light weight replacement of <see cref="AsyncOperationHandle{T}"/>
    /// </summary>
    public readonly struct ResourceHandle<T> : IEquatable<ResourceHandle<T>>
    {
        internal readonly int handleID;
        internal readonly int operationType;
        internal readonly AsyncOperationHandle<T> InternalHandle => ResourceSystem.CastOperationHandle<T>(handleID);
        public readonly T Result => InternalHandle.Result;
#if UNITASK_SUPPORT
        public readonly UniTask<T> Task => InternalHandle.ToUniTask();
#else
        public readonly Task<T> Task => InternalHandle.Task;
#endif
        public ResourceHandle(int handleID, int operationType)
        {
            this.handleID = handleID;
            this.operationType = operationType;
        }
        public static implicit operator ResourceHandle(ResourceHandle<T> obj)
        {
            return new ResourceHandle(obj.handleID, obj.operationType);
        }
        public readonly void RegisterCallBack(Action<T> callBack)
        {
            InternalHandle.Completed += (h) => callBack?.Invoke(h.Result);
        }
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
    }
}
