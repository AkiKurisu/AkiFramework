using System.Collections.Generic;
namespace Kurisu.Framework.Pool
{
    /// <summary>
    /// Represents a simple object pool that stores objects without a factory method.
    /// </summary>
    internal class StoreOnlyObjectPool
    {
        private const int PoolCapacity = 10;
        public StoreOnlyObjectPool() { }
        public StoreOnlyObjectPool(IPooled obj)
        {
            Release(obj);
        }
        internal readonly Queue<IPooled> poolQueue = new(PoolCapacity);
        public void Release(IPooled obj)
        {
            poolQueue.Enqueue(obj);
        }
        public object Get()
        {
            if (poolQueue.TryDequeue(out IPooled result))
            {
                return result;
            }
            return null;
        }
    }
}