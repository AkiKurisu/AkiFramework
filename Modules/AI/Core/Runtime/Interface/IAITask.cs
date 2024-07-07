namespace Kurisu.Framework.AI
{
    public enum TaskStatus
    {
        //Task is disabled and will not be updated
        Disabled,
        //Task is enabled and is updating by its owner agent
        Enabled,
        //Task is pending currently and will be enabled after agent activated
        Pending
    }
    /// <summary>
    /// Task with flag
    /// </summary>
    public interface ITask
    {
        TaskStatus Status { get; }
        /// <summary>
        /// Entry to tick the task
        /// </summary>
        void Tick();
    }
    /// <summary>
    /// AI controlled task
    /// </summary>
    public interface IAITask : ITask
    {
        string TaskID { get; }
        void Init(AIController host);
        /// <summary>
        /// Persistent task will not be disabled, but can still be pending.
        /// For task never to be disabled or pending, move the task outside of agent
        /// </summary>
        /// <value></value>
        bool IsPersistent { get; }
        void Start();
        void Pause();
        void Stop();
    }
}
