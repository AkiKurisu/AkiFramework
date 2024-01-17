using System.Collections.Generic;
using UnityEngine;
using System;
namespace Kurisu.Framework
{
    public class EventBase
    {
        private IEventHandler currentTarget;
        public IEventHandler Target
        {
            get => currentTarget;
            internal set => currentTarget = value;
        }
    }
    /// <summary>
    /// Pooled event argument
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventBase<T> : EventBase where T : EventBase<T>, new()
    {
        private static readonly ObjectPool<T> pool = new(() => new());
        public bool Pooled { get; private set; }
        public static T GetPooled()
        {
            var eventBase = pool.Get();
            eventBase.Pooled = false;
            eventBase.Target = null;
            eventBase.OnInit();
            return eventBase;
        }
        /// <summary>
        /// Reset data
        /// </summary>
        protected virtual void OnInit() { }
        public void Release()
        {
            if (Pooled) return;
            pool.Push((T)this);
            Pooled = true;
        }
    }
    /// <summary>
    /// Handle Event
    /// </summary>
    public interface IEventHandler
    {
        void Send<TEventType>(TEventType eventBase) where TEventType : EventBase<TEventType>, new();
    }
    /// <summary>
    /// Send callBack to handle event
    /// </summary>
    public class CallbackEventHandler : IEventHandler
    {
        public void RegisterCallback<TEventType>(Action<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            //Register callback to global
            EventSystem.Register(this, callback);
        }
        public IUnRegister RegisterCallbackWithUnRegister<TEventType>(Action<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            //Register callback to global
            EventSystem.Register(this, callback);
            return new CustomUnRegister(() => UnRegisterCallback(callback));
        }
        public void UnRegisterCallback<TEventType>(Action<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            //Register callback to global
            EventSystem.UnRegister(this, callback);
        }
        public void Send<TEventType>(TEventType eventBase) where TEventType : EventBase<TEventType>, new()
        {
            eventBase.Target = this;
            //Send local callBack
            EventSystem.Send(this, eventBase);
            eventBase.Release();
        }
    }
    internal class EventSystem
    {
        private static readonly Dictionary<IEventHandler, AkiEvents> mEvents = new();
        public static void Send<T>(IEventHandler eventHandler, T args)
        {
            if (mEvents.TryGetValue(eventHandler, out var mEvent))
            {
                mEvent.GetEvent<AkiEvent<T>>()?.Trigger(args);
            }
        }
        public static IUnRegister Register<T>(IEventHandler eventHandler, Action<T> onEvent)
        {
            if (!mEvents.TryGetValue(eventHandler, out var mEvent)) mEvent = mEvents[eventHandler] = new();
            var e = mEvent.GetOrAddEvent<AkiEvent<T>>();
            return e.Register(onEvent);
        }
        public static void UnRegister<T>(IEventHandler eventHandler, Action<T> onEvent)
        {
            if (mEvents.TryGetValue(eventHandler, out var mEvent))
            {
                var e = mEvent.GetEvent<AkiEvent<T>>();
                e?.UnRegister(onEvent);
            }
        }
    }
    public interface IUnRegister
    {
        void UnRegister();
    }
    public interface IUnRegisterHandle
    {
        void AddUnRegister(IUnRegister unRegister);
    }

    public interface IUnRegisterList
    {
        List<IUnRegister> UnregisterList { get; }
    }

    public static class IUnRegisterListExtension
    {
        public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList)
        {
            unRegisterList.UnregisterList.Add(self);
        }

        public static void UnRegisterAll(this IUnRegisterList self)
        {
            foreach (var unRegister in self.UnregisterList)
            {
                unRegister.UnRegister();
            }

            self.UnregisterList.Clear();
        }
    }

    public struct CustomUnRegister : IUnRegister
    {
        /// <summary>
        /// 委托对象
        /// </summary>
        private Action OnUnRegister { get; set; }

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="onDispose"></param>
        public CustomUnRegister(Action onUnRegister)
        {
            OnUnRegister = onUnRegister;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
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
