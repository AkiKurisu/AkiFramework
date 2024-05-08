using System;
using Kurisu.Framework.Events;
namespace Kurisu.Framework.React
{
    public static class ReactExtensions
    {
        /// <summary>
        /// React version for <see cref="CallbackEventHandler.RegisterCallback"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="callback"></param>
        /// <typeparam name="TEventType"></typeparam>
        /// <returns></returns>
        public static IDisposable Subscribe<TEventType>(this CallbackEventHandler handler, EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            handler.RegisterCallback(callback);
            return new CallBackDisposable(() => handler.UnregisterCallback(callback));
        }
        /// <summary>
        /// React version for <see cref="IReadonlyReactiveProperty{T}.RegisterValueChangeCallback"/>
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDisposable SubscribeValueChange<T>(this IReadonlyReactiveProperty<T> handler, EventCallback<ChangeEvent<T>> callback)
        {
            handler.RegisterValueChangeCallback(callback);
            return new CallBackDisposable(() => handler.UnregisterValueChangeCallback(callback));
        }
    }
}
