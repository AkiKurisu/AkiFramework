using System;
using R3.Triggers;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public interface IUnRegister
    {
        void Add(IDisposable disposable);
    }
    internal readonly struct ObservableDestroyTriggerUnRegister : IUnRegister
    {
        private readonly ObservableDestroyTrigger trigger;
        public ObservableDestroyTriggerUnRegister(ObservableDestroyTrigger trigger)
        {
            this.trigger = trigger;
        }
        public readonly void Add(IDisposable disposable)
        {
            trigger.AddDisposableOnDestroy(disposable);
        }
    }
    public static class DisposableExtensions
    {
        /// <summary>
        /// Get or create an UnRegister from GameObject, listening OnDestroy event
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegister GetUnRegister(this GameObject gameObject)
        {
            return new ObservableDestroyTriggerUnRegister(gameObject.GetOrAddComponent<ObservableDestroyTrigger>());
        }
        public static T AddTo<T>(this T disposable, IUnRegister unRegister) where T : IDisposable
        {
            unRegister.Add(disposable);
            return disposable;
        }
    }
}
