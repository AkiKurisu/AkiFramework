using System;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Kurisu.Framework.Resource
{
    public readonly struct ResourceHandle
    {
        internal readonly int handleID;
        internal readonly AsyncOperationHandle InternalHandle => ResourceSystem.CastOperationHandle(handleID);
        public readonly object Result => InternalHandle.Result;
        public readonly Task Task => InternalHandle.Task;
        public ResourceHandle(int handleID)
        {
            this.handleID = handleID;
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
            return new ResourceHandle<T>(handleID);
        }
    }
    public readonly struct ResourceHandle<T>
    {
        internal readonly int handleID;
        internal readonly AsyncOperationHandle<T> InternalHandle => ResourceSystem.CastOperationHandle<T>(handleID);
        public readonly T Result => InternalHandle.Result;
        public readonly Task<T> Task => InternalHandle.Task;
        public ResourceHandle(int handleID)
        {
            this.handleID = handleID;
        }
        public static implicit operator ResourceHandle(ResourceHandle<T> obj)
        {
            return new ResourceHandle(obj.handleID);
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
    }
}
