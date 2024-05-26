using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework.React
{
    public class ObservableUpdateTrigger : MonoBehaviour
    {
        private AkiEvent onUpdate;
        public IObservable<Unit> UpdateAsObservable()
        {
            return onUpdate ??= new();
        }
        private void Update()
        {
            onUpdate?.Trigger();
        }
    }
    public class ObservableLateUpdateTrigger : MonoBehaviour
    {
        private AkiEvent onLateUpdate;
        public IObservable<Unit> LateUpdateAsObservable()
        {
            return onLateUpdate ??= new();
        }
        private void LateUpdate()
        {
            onLateUpdate?.Trigger();
        }
    }
    public class ObservableFixedUpdateTrigger : MonoBehaviour
    {
        private AkiEvent onFixedUpdate;
        public IObservable<Unit> FixedUpdateAsObservable()
        {
            return onFixedUpdate ??= new();
        }
        private void FixedUpdate()
        {
            onFixedUpdate?.Trigger();
        }
    }
    public class ObservableDestroyTrigger : MonoBehaviour, IUnRegister
    {
        private readonly HashSet<IDisposable> disposables = new();
        private bool isDestroyed = false;
        private AkiEvent onDestroy;
        public IObservable<Unit> OnDestroyAsObservable()
        {
            return onDestroy ??= new();
        }
        public void Add(IDisposable disposable)
        {
            if (isDestroyed)
            {
                disposable.Dispose();
                return;
            }
            disposables.Add(disposable);
        }

        public void Remove(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            onDestroy?.Trigger();
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
}