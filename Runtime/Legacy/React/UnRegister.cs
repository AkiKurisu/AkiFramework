using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework
{
    public interface IUnRegister
    {
        void AddDisposable(IDisposable disposable);
        void RemoveDisposable(IDisposable disposable);
    }
    public struct CallBackDisposableHandle : IDisposable
    {
        private Action OnDisposable { get; set; }
        public CallBackDisposableHandle(Action onUnRegister)
        {
            OnDisposable = onUnRegister;
        }
        public void Dispose()
        {
            OnDisposable.Invoke();
            OnDisposable = null;
        }
    }
    public class ComposableUnRegister : IUnRegister, IDisposable
    {
        private readonly HashSet<IDisposable> disposables = new();

        public void AddDisposable(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void RemoveDisposable(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
    /// <summary>
    /// UnRegister called on GameObject Destroy
    /// </summary>
    public class GameObjectOnDestroyUnRegister : MonoBehaviour, IUnRegister
    {
        private readonly HashSet<IDisposable> disposables = new();

        public void AddDisposable(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void RemoveDisposable(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        private void OnDestroy()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
}