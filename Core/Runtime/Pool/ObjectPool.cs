using System;
using System.Collections.Generic;
namespace Kurisu.Framework
{
    public class ObjectPool
    {
        private const int PoolCapacity = 10;
        public ObjectPool(object obj)
        {
            Push(obj);
        }
        internal readonly Queue<object> poolQueue = new(PoolCapacity);
        public void Push(object obj)
        {
            poolQueue.Enqueue(obj);
        }
        public object Get()
        {
            if (poolQueue.TryDequeue(out object result))
            {
                return result;
            }
            return null;
        }
    }
    public class ObjectPool<T>
    {
        private const int PoolCapacity = 10;
        private readonly Func<T> instanceFunc;
        public ObjectPool(Func<T> instanceFunc)
        {
            this.instanceFunc = instanceFunc;
            poolQueue = new(PoolCapacity);
        }
        public ObjectPool(Func<T> instanceFunc, int capacity)
        {
            this.instanceFunc = instanceFunc;
            poolQueue = new(capacity);
        }
        internal readonly Queue<T> poolQueue;
        public void Push(T obj)
        {
            poolQueue.Enqueue(obj);
        }
        public T Get()
        {
            if (!poolQueue.TryDequeue(out T result))
            {
                result = instanceFunc();
            }
            return result;
        }
    }
}