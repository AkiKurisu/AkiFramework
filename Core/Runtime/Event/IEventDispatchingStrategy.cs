using System.Diagnostics;
namespace Kurisu.Framework.Events
{
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
        AtTarget = 2,

        // Execute the default action(s) at target.
        /// <summary>
        /// The event is sent to the target element, which can then execute its default actions for the event at the target phase. Event handlers do not receive the event in this phase. Instead, ExecuteDefaultActionAtTarget is called on the target element.
        /// </summary>
        DefaultActionAtTarget = 5,

        // At last, execute the default action(s).
        /// <summary>
        /// The event is sent to the target element, which can then execute its final default actions for the event. Event handlers do not receive the event in this phase. Instead, ExecuteDefaultAction is called on the target element.
        /// </summary>
        DefaultAction = 4
    }
    public interface IEventDispatchingStrategy
    {
        bool CanDispatchEvent(EventBase evt);
        void DispatchEvent(EventBase evt, IEventCoordinator coordinator);
    }
    public class CallBackDispatchingStrategy : IEventDispatchingStrategy
    {
        public bool CanDispatchEvent(EventBase evt)
        {
            return evt.Target is CallbackEventHandler;
        }
        public void DispatchEvent(EventBase evt, IEventCoordinator coordinator)
        {
            if (!evt.IsPropagationStopped && coordinator != null)
            {
                Debug.Assert(!evt.Dispatch, "Event is being dispatched recursively.");
                evt.Dispatch = true;

                (evt.Target as CallbackEventHandler).HandleEventAtTargetPhase(evt);

                evt.Dispatch = false;
            }
            evt.StopDispatch = true;
        }
    }
}