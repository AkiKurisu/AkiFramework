namespace Kurisu.Framework.AI
{
    public interface IAITask
    {
        void SetController(AIController controller);
        /// <summary>
        /// Whether this task should automatically start when controller is enabled
        /// </summary>
        /// <returns></returns>
        bool IsStartOnEnabled();
    }
}
