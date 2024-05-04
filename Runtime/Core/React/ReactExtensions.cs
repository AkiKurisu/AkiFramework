using Kurisu.Framework.Events;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public static class ReactExtensions
    {
        public static IUnRegisterHandle RegisterCallbackWithUnRegister<TEventType>(this CallbackEventHandler handler, EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback(callback);
            return new UnRegisterCallBackHandle(() => handler.UnregisterCallback(callback));
        }
        public static IUnRegisterHandle RegisterCallbackWithUnRegister<T>(this IReadonlyReactiveValue<T> handler, EventCallback<ChangeEvent<T>> callback)
        {
            handler.RegisterCallback(callback);
            return new UnRegisterCallBackHandle(() => handler.UnregisterCallback(callback));
        }
    }
}
