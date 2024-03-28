#if UNITASK_SUPPORT
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
namespace Kurisu.Framework.Resource
{
    public class SequencePool
    {
        private static readonly ObjectPool<Sequence> pool = new(() => new());
        public static Sequence Get()
        {
            var sequence = pool.Get();
            sequence.Clear();
            return sequence;
        }
        public static void Return(Sequence sequencePool)
        {
            pool.Push(sequencePool);
        }
    }
    public class Sequence : List<UniTask>, IDisposable
    {
        public void Dispose()
        {
            SequencePool.Return(this);
        }
        public UniTask.Awaiter GetAwaiter()
        {
            return UniTask.WhenAll(this).GetAwaiter();
        }
    }
}
#endif