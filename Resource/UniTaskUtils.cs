#if UNITASK_SUPPORT
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
namespace Kurisu.Framework.Resource
{
    public class ParallelTask : List<UniTask>, IDisposable
    {
        private static readonly ObjectPool<ParallelTask> pool = new(() => new(), (e) => e.Clear());
        public static ParallelTask Get()
        {
            return pool.Get();
        }
        public void Dispose()
        {
            pool.Release(this);
        }
        public UniTask.Awaiter GetAwaiter()
        {
            return UniTask.WhenAll(this).GetAwaiter();
        }
    }
    public class ParallelTask<T> : List<UniTask<T>>, IDisposable
    {
        private static readonly ObjectPool<ParallelTask<T>> pool = new(() => new(), (e) => e.Clear());
        public static ParallelTask<T> Get()
        {
            return pool.Get();
        }
        public void Dispose()
        {
            pool.Release(this);
        }
        public UniTask<T[]>.Awaiter GetAwaiter()
        {
            return UniTask.WhenAll(this).GetAwaiter();
        }
    }
    public class SequenceTask : List<UniTask>, IDisposable
    {
        private static readonly ObjectPool<SequenceTask> pool = new(() => new(), (e) => e.Clear());
        public static SequenceTask Get()
        {
            return pool.Get();
        }
        public void Dispose()
        {
            pool.Release(this);
        }
        public UniTask.Awaiter GetAwaiter()
        {
            return AwaitAsSequence().GetAwaiter();
        }
        private async UniTask AwaitAsSequence()
        {
            foreach (var task in this)
            {
                await task;
            };
        }
    }
    public class SequenceTask<T> : List<UniTask<T>>, IDisposable
    {
        private static readonly ObjectPool<SequenceTask<T>> pool = new(() => new(), (e) => e.Clear());
        public static SequenceTask<T> Get()
        {
            return pool.Get();
        }
        public void Dispose()
        {
            pool.Release(this);
        }
        public UniTask<T[]>.Awaiter GetAwaiter()
        {
            return AwaitAsSequence().GetAwaiter();
        }
        private async UniTask<T[]> AwaitAsSequence()
        {
            var results = new T[Count];
            for (int i = 0; i < Count; ++i)
            {
                results[i] = await this[i];
            };
            return results;
        }
    }
}
#endif