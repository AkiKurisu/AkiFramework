using System;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public static class DisposableExtensions
    {
        /// <summary>
        /// Dispose when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, GameObject gameObject) where T : IDisposable
        {
            gameObject.GetUnRegister().Add(handle);
            return handle;
        }
        /// <summary>
        /// Get or create an UnRegister from GameObject, listening OnDestroy event
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegister GetUnRegister(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<ObservableDestroyTrigger>();
        }
        /// <summary>
        /// Dispose subscription when GameObject destroy
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T subscription, Component component) where T : IDisposable
        {
            component.gameObject.GetUnRegister().Add(subscription);
            return subscription;
        }
        /// <summary>
        /// Dispose subscription managed by a <see cref="IUnRegister"/>
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T subscription, IUnRegister unRegister) where T : IDisposable
        {
            unRegister.Add(subscription);
            return subscription;
        }
    }
}
