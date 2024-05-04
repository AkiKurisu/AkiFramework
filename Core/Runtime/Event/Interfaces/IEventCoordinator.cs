namespace Kurisu.Framework.Events
{
    /// <summary>
    /// Interface to certain event processing environment
    /// </summary>
    public interface IEventCoordinator
    {
        /// <summary>
        /// This coordinator EventDispatcher.
        /// </summary>
        EventDispatcher Dispatcher { get; }
    }
}
