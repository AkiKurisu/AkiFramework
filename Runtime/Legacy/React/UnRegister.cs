using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework
{
    /// <summary>
    /// Class to manage <see cref="IDisposable"/>
    /// </summary>
    public interface IUnRegister
    {
        void AddDisposable(IDisposable disposable);
        void RemoveDisposable(IDisposable disposable);
    }
    /// <summary>
    /// CallBack implement of <see cref="IDisposable"/>, will invoke callback on dispose
    /// </summary>
    public struct CallBackDisposable : IDisposable
    {
        private Action OnDisposable { get; set; }
        public CallBackDisposable(Action onUnRegister)
        {
            OnDisposable = onUnRegister;
        }
        public void Dispose()
        {
            OnDisposable.Invoke();
            OnDisposable = null;
        }
    }
    /// <summary>
    /// Composite implement of <see cref="IDisposable"/> and <see cref="IUnRegister"/>, will dispose inner disposable children on dispose
    /// </summary>
    public class CompositeDisposable : IUnRegister, IDisposable
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
        private bool isDestroyed = false;
        public void AddDisposable(IDisposable disposable)
        {
            if (isDestroyed)
            {
                disposable.Dispose();
                return;
            }
            disposables.Add(disposable);
        }

        public void RemoveDisposable(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
}