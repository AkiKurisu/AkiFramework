using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduled task
    /// </summary>
    public readonly struct SchedulerHandle : IDisposable
    {
        /// <summary>
        /// Unique id for scheduled task
        /// </summary>
        /// <value></value>
        public uint Handle { get; }
        /// <summary>
        /// Get scheduled task whether is valid
        /// </summary>
        /// <value></value>
        public readonly bool IsValid
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
                return SchedulerRunner.Instance.IsValid(Handle);
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
                if (SchedulerRunner.Instance.TryGet(Handle, out IScheduled task))
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
                if (SchedulerRunner.Instance.TryGet(Handle, out IScheduled task))
                {
                    return task;
                }
                return null;
            }
        }
        public SchedulerHandle(uint taskId)
        {
            Handle = taskId;
        }
        /// <summary>
        /// Cancel a scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            if (!SchedulerRunner.IsInitialized) return;
            if (!IsValid) return;
            SchedulerRunner.Instance.Cancel(Handle);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}