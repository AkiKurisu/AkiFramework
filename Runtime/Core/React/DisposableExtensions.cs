using System;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public static class DisposableExtensions
    {
        #region IDisposable
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
        /// Dispose when GameObject destroy
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, Component component) where T : IDisposable
        {
            component.gameObject.GetUnRegister().Add(handle);
            return handle;
        }
        /// <summary>
        /// Dispose managed by a unRegister
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddTo<T>(this T handle, IUnRegister unRegister) where T : IDisposable
        {
            unRegister.Add(handle);
            return handle;
        }
        #endregion
    }
}
