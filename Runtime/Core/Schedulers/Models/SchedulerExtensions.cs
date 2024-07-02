using System;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace Kurisu.Framework.Schedulers
{
    public static class SchedulerExtensions
    {
        /// <summary>
        /// Async api for scheduler
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="disposeSchedulerWhenCancelled">Also dispose scheduler when cancellation happen</param>
        /// <returns></returns>
        public static UniTask WaitAsync(this SchedulerHandle handle, CancellationToken cancellationToken = default, bool disposeSchedulerWhenCancelled = true)
        {
            if (disposeSchedulerWhenCancelled)
                handle.AddTo(cancellationToken);
            return UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken);
        }
        internal static SchedulerHandle CreateHandle(this IScheduled scheduled)
        {
            return SchedulerRunner.Instance.CreateHandle(scheduled);
        }
        internal static void OverwriteHandle(this IScheduled scheduled, ref SchedulerHandle handle)
        {
            handle.Dispose();
            handle = SchedulerRunner.Instance.CreateHandle(scheduled);
        }
    }
}