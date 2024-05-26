using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduled task
    /// </summary>
    public readonly struct SchedulerHandle : IDisposable
    {
        public int TaskId { get; }
        public readonly bool IsValid
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return false;
                return SchedulerRunner.Instance.IsValid(TaskId);
            }
        }
        public readonly bool IsDone
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return false;
                if (SchedulerRunner.Instance.TryGet(TaskId, out IScheduled task))
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
        public readonly IScheduled Task
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return null;
                if (SchedulerRunner.Instance.TryGet(TaskId, out IScheduled task))
                {
                    return task;
                }
                return null;
            }
        }
        public SchedulerHandle(int taskId)
        {
            TaskId = taskId;
        }
        /// <summary>
        /// Cancel a scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            if (!SchedulerRunner.IsInitialized) return;
            if (!IsValid) return;
            SchedulerRunner.Instance.Cancel(TaskId);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}