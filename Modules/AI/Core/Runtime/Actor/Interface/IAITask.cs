namespace Chris.AI
{
    public interface IAITask
    {
        /// <summary>
        /// Set host controller
        /// </summary>
        /// <param name="controller"></param>
        void SetController(AIController controller);
        /// <summary>
        /// Whether this task should automatically start when controller is enabled
        /// </summary>
        /// <returns></returns>
        bool IsStartOnEnabled();
    }
}
