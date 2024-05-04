using System.Diagnostics;
namespace Kurisu.Framework.Events
{
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