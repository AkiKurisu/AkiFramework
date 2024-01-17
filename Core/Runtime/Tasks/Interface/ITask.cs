namespace Kurisu.Framework.Tasks
{
    /// <summary>
    /// Task scheduled by task manager
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Get whether or not the task has finished running for any reason.
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        /// Update task job
        /// </summary>
        void Update();
        /// <summary>
        /// Stop a task that is in-progress or paused. The task's on completion callback will not be called.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Pause a running task. A paused task can be resumed from the same point it was paused.
        /// </summary>
        void Pause();
        /// <summary>
        /// Continue a paused task. Does nothing if the task has not been paused.
        /// </summary>
        void Resume();
        /// <summary>
        /// Release task
        /// </summary>
        void Dispose();
    }
}