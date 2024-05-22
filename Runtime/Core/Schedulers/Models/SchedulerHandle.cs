using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduler
    /// </summary>
    public readonly struct SchedulerHandle : IDisposable
    {
        public int Id { get; }
        public readonly bool IsValid
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return false;
                return SchedulerRunner.Instance.IsValid(Id);
            }
        }
        public readonly bool IsDone
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return false;
                if (SchedulerRunner.Instance.TryGet(Id, out IScheduler task))
                {
                    return task.IsDone;
                }
                return false;
            }
        }
        /// <summary>
        /// Get scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        public readonly IScheduler Scheduler
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return null;
                if (SchedulerRunner.Instance.TryGet(Id, out IScheduler task))
                {
                    return task;
                }
                return null;
            }
        }
        public SchedulerHandle(int taskId)
        {
            Id = taskId;
        }
        /// <summary>
        /// Cancel a scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            if (!SchedulerRunner.IsInitialized) return;
            if (!IsValid) return;
            SchedulerRunner.Instance.CancelScheduler(Id);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}