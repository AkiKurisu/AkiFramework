using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduled task
    /// </summary>
    public readonly struct SchedulerHandle : IDisposable
    {
        public int TaskId { get; }
        /// <summary>
        /// Get scheduled task whether is valid
        /// </summary>
        /// <value></value>
        public readonly bool IsValid
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
                return SchedulerRunner.Instance.IsValid(TaskId);
            }
        }
        /// <summary>
        /// Get scheduled task whether is done
        /// </summary>
        /// <value></value>
        public readonly bool IsDone
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
                if (SchedulerRunner.Instance.TryGet(TaskId, out IScheduled task))
                {
                    return task.IsDone;
                }
                return true;
            }
        }
        /// <summary>
        /// Get scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        internal readonly IScheduled Task
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
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