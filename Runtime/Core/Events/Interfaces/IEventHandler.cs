using UnityEngine;
namespace Kurisu.Framework.Events
{
    /// <summary>
    /// Interface for class capable of handling events.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// Sends an event to the event handler.
        /// </summary>
        /// <param name="e">The event to send.</param>
        void SendEvent(EventBase e);

        /// <summary>
        /// Handle an event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        void HandleEvent(EventBase evt);

    }
    /// <summary>
    /// Interface for class have behaviour lifetime scope
    /// </summary>
    public interface IBehaviourScope
    {
        Behaviour AttachedBehaviour { get; }
    }
}
