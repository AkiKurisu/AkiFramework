using System;
using System.Threading;
using R3;
using R3.Triggers;
using UnityEngine;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Unregister scope interface for managing <see cref="IDisposable"/> 
    /// </summary>
    public interface IDisposableUnregister
    {
        /// <summary>
        /// Register new disposable to this unregister scope
        /// </summary>
        /// <param name="disposable"></param>
        void Register(IDisposable disposable);
    }
    public readonly struct ObservableDestroyTriggerUnregister : IDisposableUnregister
    {
        private readonly ObservableDestroyTrigger trigger;
        public ObservableDestroyTriggerUnregister(ObservableDestroyTrigger trigger)
        {
            this.trigger = trigger;
        }
        public readonly void Register(IDisposable disposable)
        {
            trigger.AddDisposableOnDestroy(disposable);
        }
    }
    public readonly struct CancellationTokenUnregister : IDisposableUnregister
    {
        private readonly CancellationToken cancellationToken;
        public CancellationTokenUnregister(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }
        public readonly void Register(IDisposable disposable)
        {
            disposable.RegisterTo(cancellationToken);
        }
    }
    public static class DisposableExtensions
    {
        /// <summary>
        /// Get or create an UnRegister from <see cref="GameObject"/>, listening OnDestroy event
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static ObservableDestroyTriggerUnregister GetUnregister(this GameObject gameObject)
        {
            return new ObservableDestroyTriggerUnregister(gameObject.GetOrAddComponent<ObservableDestroyTrigger>());
        }
        /// <summary>
        ///  Get or create an UnRegister from <see cref="MonoBehaviour"/>, listening OnDestroy event
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <returns></returns>
        public static CancellationTokenUnregister GetUnregister(this MonoBehaviour monoBehaviour)
        {
            return new CancellationTokenUnregister(monoBehaviour.destroyCancellationToken);
        }
        public static T AddTo<T, K>(this T disposable, ref K unRegister) where T : IDisposable where K : struct, IDisposableUnregister
        {
            unRegister.Register(disposable);
            return disposable;
        }
        public static T AddTo<T>(this T disposable, IDisposableUnregister unRegister) where T : IDisposable
        {
            unRegister.Register(disposable);
            return disposable;
        }
    }
}
