using Kurisu.Framework.Events;
namespace Kurisu.Framework.React
{
    public static class ReactExtensions
    {
        public static IUnRegisterHandle RegisterCallbackWithUnRegister<TEventType>(this CallbackEventHandler handler, EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback(callback);
            return new UnRegisterCallBackHandle(() => handler.UnregisterCallback(callback));
        }
        public static IUnRegisterHandle RegisterValueChangeCallbackWithUnRegister<T>(this IReadonlyReactiveValue<T> handler, EventCallback<ChangeEvent<T>> callback)
        {
            handler.RegisterValueChangeCallback(callback);
            return new UnRegisterCallBackHandle(() => handler.UnregisterValueChangeCallback(callback));
        }
    }
}
