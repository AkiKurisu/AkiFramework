namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduled task
    /// </summary>
    public readonly struct SchedulerHandle
    {
        public int TaskId { get; }
        public readonly bool IsValid
        {
            get
            {
                return SchedulerRunner.Instance.IsValid(TaskId);
            }
        }
        public readonly bool IsDone
        {
            get
            {
                if (SchedulerRunner.Instance.TryGet(TaskId, out IScheduler task))
                {
                    return task.IsDone;
                }
                return false;
            }
        }
        /// <summary>
        /// Get task if task is valid (Not be disposed and done)
        /// </summary>
        /// <value></value>
        public readonly IScheduler Task
        {
            get
            {
                if (SchedulerRunner.Instance.TryGet(TaskId, out IScheduler task))
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
        /// Cancel a task if task is valid (haven't been disposed or done)
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            //Task manager is destroyed
            if (!SchedulerRunner.IsInitialized) return;
            if (!IsValid) return;
            SchedulerRunner.Instance.CancelScheduler(TaskId);
        }
    }
}