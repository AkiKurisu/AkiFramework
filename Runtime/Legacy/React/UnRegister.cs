using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework
{
    public interface IUnRegisterHandle
    {
        void UnRegister();
    }
    public interface IUnRegister
    {
        void AddUnRegisterHandle(IUnRegisterHandle handle);
        void RemoveUnRegisterHandle(IUnRegisterHandle handle);
    }
    public struct UnRegisterCallBackHandle : IUnRegisterHandle
    {
        private Action OnUnRegister { get; set; }
        public UnRegisterCallBackHandle(Action onUnRegister)
        {
            OnUnRegister = onUnRegister;
        }
        public void UnRegister()
        {
            OnUnRegister.Invoke();
            OnUnRegister = null;
        }
    }
    public class UnRegister : IUnRegister, IDisposable
    {
        private readonly HashSet<IUnRegisterHandle> mUnRegisters = new();

        public void AddUnRegisterHandle(IUnRegisterHandle unRegister)
        {
            mUnRegisters.Add(unRegister);
        }

        public void RemoveUnRegisterHandle(IUnRegisterHandle unRegister)
        {
            mUnRegisters.Remove(unRegister);
        }

        public void Dispose()
        {
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }
            mUnRegisters.Clear();
        }
    }
    /// <summary>
    /// UnRegister called on GameObject Destroy
    /// </summary>
    public class GameObjectOnDestroyUnRegister : MonoBehaviour, IUnRegister
    {
        private readonly HashSet<IUnRegisterHandle> handles = new();

        public void AddUnRegisterHandle(IUnRegisterHandle unRegister)
        {
            handles.Add(unRegister);
        }

        public void RemoveUnRegisterHandle(IUnRegisterHandle unRegister)
        {
            handles.Remove(unRegister);
        }

        private void OnDestroy()
        {
            foreach (var unRegister in handles)
            {
                unRegister.UnRegister();
            }
            handles.Clear();
        }
    }
}