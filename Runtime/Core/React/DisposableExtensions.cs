using System;
using System.Threading;
using R3;
using R3.Triggers;
using UnityEngine;
namespace Kurisu.Framework.React
{
    public interface IUnRegister
    {
        void Add(IDisposable disposable);
    }
    public readonly struct ObservableDestroyTriggerUnRegister : IUnRegister
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
    public readonly struct CancellationTokenUnRegister : IUnRegister
    {
        private readonly CancellationToken cancellationToken;
        public CancellationTokenUnRegister(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }
        public readonly void Add(IDisposable disposable)
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
        public static ObservableDestroyTriggerUnRegister GetUnRegister(this GameObject gameObject)
        {
            return new ObservableDestroyTriggerUnRegister(gameObject.GetOrAddComponent<ObservableDestroyTrigger>());
        }
        /// <summary>
        ///  Get or create an UnRegister from <see cref="MonoBehaviour"/>, listening OnDestroy event
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <returns></returns>
        public static CancellationTokenUnRegister GetUnRegister(this MonoBehaviour monoBehaviour)
        {
            return new CancellationTokenUnRegister(monoBehaviour.destroyCancellationToken);
        }
        public static T AddTo<T, K>(this T disposable, ref K unRegister) where T : IDisposable where K : struct, IUnRegister
        {
            unRegister.Add(disposable);
            return disposable;
        }
        public static T AddTo<T>(this T disposable, IUnRegister unRegister) where T : IDisposable
        {
            unRegister.Add(disposable);
            return disposable;
        }
    }
}
