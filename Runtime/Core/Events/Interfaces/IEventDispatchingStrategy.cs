using System.Diagnostics;
namespace Kurisu.Framework.Events
{
    // Simplified from UIElement
    // Remove TrickleDown and BubbleUp phase type
    public enum PropagationPhase
    {
        // Not propagating at the moment.
        /// <summary>
        /// The event is not propagated.
        /// </summary>
        None = 0,

        // Event is at target.
        /// <summary>
        /// The event is sent to the target.
        /// </summary>
        AtTarget = 1,

        // Execute the default action(s) at target.
        /// <summary>
        /// The event is sent to the target element, which can then execute its default actions for the event at the target phase. Event handlers do not receive the event in this phase. Instead, ExecuteDefaultActionAtTarget is called on the target element.
        /// </summary>
        DefaultActionAtTarget = 2,

        // At last, execute the default action(s).
        /// <summary>
        /// The event is sent to the target element, which can then execute its final default actions for the event. Event handlers do not receive the event in this phase. Instead, ExecuteDefaultAction is called on the target element.
        /// </summary>
        DefaultAction = 3
    }
    public interface IEventDispatchingStrategy
    {
        bool CanDispatchEvent(EventBase evt);
        void DispatchEvent(EventBase evt, IEventCoordinator coordinator);
    }
}