using System;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Chris.Resource
{
    /// <summary>
    /// A lightweight encapsulation of <see cref="AsyncOperationHandle"/>
    /// </summary>
    public readonly struct ResourceHandle : IEquatable<ResourceHandle>, IDisposable
    {
        internal readonly uint version;
        internal readonly int index;
        internal readonly byte operationType;
        internal readonly AsyncOperationHandle InternalHandle => ResourceSystem.CastOperationHandle(version, index);
        public readonly object Result => InternalHandle.Result;
        public readonly UniTask Task => InternalHandle.ToUniTask();
        public ResourceHandle(uint version, int index, byte operationType)
        {
            this.version = version;
            this.index = index;
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
        /// <summary>
        /// Converts handle to be typed.
        /// To convert back to non-typed, implicit conversion is available.
        /// </summary>
        /// <typeparam name="T">The type of the handle.</typeparam>
        /// <returns>A new handle that is typed.</returns>
        public ResourceHandle<T> Convert<T>()
        {
            return (ResourceHandle<T>)this;
        }
        public bool Equals(ResourceHandle other)
        {
            return other.index == index && other.InternalHandle.Equals(InternalHandle);
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
    /// A lightweight replacement of <see cref="AsyncOperationHandle{T}"/>
    /// </summary>
    public readonly struct ResourceHandle<T> : IEquatable<ResourceHandle<T>>, IDisposable
    {
        internal readonly uint version;
        internal readonly int index;
        internal readonly byte operationType;
        internal readonly AsyncOperationHandle<T> InternalHandle => ResourceSystem.CastOperationHandle<T>(version, index);
        public readonly T Result => InternalHandle.Result;
        public readonly UniTask<T> Task => InternalHandle.ToUniTask();
        public ResourceHandle(uint version, int index, byte operationType)
        {
            this.version = version;
            this.index = index;
            this.operationType = operationType;
        }
        public static implicit operator ResourceHandle(ResourceHandle<T> obj)
        {
            return new ResourceHandle(obj.version, obj.index, obj.operationType);
        }
        public static implicit operator ResourceHandle<T>(ResourceHandle obj)
        {
            return new ResourceHandle<T>(obj.version, obj.index, obj.operationType);
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
            return other.index == index && other.InternalHandle.Equals(InternalHandle);
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
