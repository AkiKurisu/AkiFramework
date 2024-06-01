namespace Kurisu.Framework.Events
{
    /// <summary>
    /// Interface for specified event processing environment
    /// </summary>
    public interface IEventCoordinator
    {
        /// <summary>
        /// Implement to set coordinator's root event handler, set null if not need
        /// </summary>
        /// <value></value>
        CallbackEventHandler RootEventHandler { get; }
    }
}
