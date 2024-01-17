using System;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Kurisu.Framework.Resource
{
    /// <summary>
    /// Invoke complete call back when all async resource is loaded
    /// </summary>
    public class ResourceAsyncSequence : IPooled
    {
        private int count = 0;
        private Action Completed;
        private bool isPending;
        public ResourceAsyncSequence()
        {
            Reset();
        }
        internal void Reset()
        {
            Completed = null;
            count = 0;
            isPending = true;
        }
        /// <summary>
        /// 添加异步处理,进行计数
        /// </summary>
        /// <param name="handle"></param>
        public ResourceAsyncSequence Add(AsyncOperationHandle handle)
        {
            handle.Completed += Count;
            count++;
            return this;
        }
        public ResourceAsyncSequence Register(Action completeCallBack)
        {
            Completed += completeCallBack;
            return this;
        }
        internal void Check()
        {
            if (isPending)
            {
                isPending = false;
                if (count == 0)
                {
                    Complete();
                }
            }
        }
        private void Count(AsyncOperationHandle handle)
        {
            handle.Completed -= Count;
            count--;
            if (count == 0)
            {
                if (!isPending)
                {
                    Complete();
                }
            }
        }
        private void Complete()
        {
            Completed?.Invoke();
            Completed = null;
            this.ObjectPushPool();
        }
    }
}
