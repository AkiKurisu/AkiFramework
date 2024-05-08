namespace Kurisu.Framework.Tasks
{
    /// <summary>
    /// Handle give you access to track target task
    /// </summary>
    public readonly struct TaskHandle
    {
        public int TaskId { get; }
        public readonly bool IsValid
        {
            get
            {
                return TaskManager.Instance.IsValidTask(TaskId);
            }
        }
        public readonly bool IsDone
        {
            get
            {
                if (TaskManager.Instance.TryGetTask(TaskId, out ITask task))
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
        public readonly ITask Task
        {
            get
            {
                if (TaskManager.Instance.TryGetTask(TaskId, out ITask task))
                {
                    return task;
                }
                return null;
            }
        }
        public TaskHandle(int taskId)
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
            if (!TaskManager.IsInitialized) return;
            if (!IsValid) return;
            TaskManager.Instance.CancelTask(TaskId);
        }
    }
}