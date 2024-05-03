using System.Collections.Generic;
namespace Kurisu.Framework
{
    internal class ObjectPool
    {
        private const int PoolCapacity = 10;
        public ObjectPool() { }
        public ObjectPool(object obj)
        {
            Release(obj);
        }
        internal readonly Queue<object> poolQueue = new(PoolCapacity);
        public void Release(object obj)
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
}