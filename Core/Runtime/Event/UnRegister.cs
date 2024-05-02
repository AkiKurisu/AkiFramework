using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework
{
    public interface IUnRegister
    {
        void UnRegister();
    }
    public interface IUnRegisterHandle
    {
        void AddUnRegister(IUnRegister unRegister);
        void RemoveUnRegister(IUnRegister unRegister);
    }
    public struct CustomUnRegister : IUnRegister
    {
        private Action OnUnRegister { get; set; }
        public CustomUnRegister(Action onUnRegister)
        {
            OnUnRegister = onUnRegister;
        }
        public void UnRegister()
        {
            OnUnRegister.Invoke();
            OnUnRegister = null;
        }
    }
    public class UnRegisterHandle : IUnRegisterHandle, IDisposable
    {
        private readonly HashSet<IUnRegister> mUnRegisters = new();

        public void AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
        }

        public void RemoveUnRegister(IUnRegister unRegister)
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
    public class UnRegisterOnDestroyTrigger : MonoBehaviour, IUnRegisterHandle
    {
        private readonly HashSet<IUnRegister> mUnRegisters = new();

        public void AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
        }

        public void RemoveUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Remove(unRegister);
        }

        private void OnDestroy()
        {
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }

            mUnRegisters.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        /// <summary>
        /// Release UnRegister when GameObject destroy
        /// </summary>
        /// <param name="unRegister"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static IUnRegister AttachUnRegister(this IUnRegister unRegister, GameObject gameObject)
        {
            gameObject.GetUnRegister().AddUnRegister(unRegister);
            return unRegister;
        }
        public static IUnRegister AttachUnRegister(this IUnRegister unRegister, IUnRegisterHandle trigger)
        {
            trigger.AddUnRegister(unRegister);
            return unRegister;
        }
        public static UnRegisterOnDestroyTrigger GetUnRegister(this GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();
            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }
            return trigger;
        }
    }
}