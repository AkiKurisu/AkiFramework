namespace Kurisu.Framework.Tasks
{
    /// <summary>
    /// Handle give you access to target task
    /// </summary>
    public readonly struct JobHandle
    {
        public int JobID { get; }
        public readonly bool IsValid
        {
            get
            {
                return TaskManager.Instance.IsValidJob(JobID);
            }
        }
        public readonly bool IsDone
        {
            get
            {
                if (TaskManager.Instance.TryGetTask(JobID, out ITask task))
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
                if (TaskManager.Instance.TryGetTask(JobID, out ITask task))
                {
                    return task;
                }
                return null;
            }
        }
        public JobHandle(int jobID)
        {
            JobID = jobID;
        }
        /// <summary>
        /// Cancel a task if task is valid (Haven't been disposed or done)
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            //Task manager is destroyed
            if (!TaskManager.IsInitialized) return;
            if (!IsValid) return;
            TaskManager.Instance.CancelJob(JobID);
        }
    }
}