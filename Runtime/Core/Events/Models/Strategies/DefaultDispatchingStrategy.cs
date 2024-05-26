namespace Kurisu.Framework.Events
{
    public class DefaultDispatchingStrategy : IEventDispatchingStrategy
    {
        public bool CanDispatchEvent(EventBase evt)
        {
            return true;
        }

        public void DispatchEvent(EventBase evt, IEventCoordinator coordinator)
        {
            if (evt.Target is CallbackEventHandler ve && ve.Root == coordinator)
            {
                EventDispatchUtilities.PropagateEvent(evt);
            }
            evt.StopDispatch = true;
        }
    }
}