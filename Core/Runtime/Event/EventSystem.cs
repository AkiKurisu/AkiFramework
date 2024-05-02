using System.Collections.Generic;
using System;
namespace Kurisu.Framework
{
    public abstract class EventBase
    {
        private IEventHandler currentTarget;
        public IEventHandler Target
        {
            get => currentTarget;
            internal set => currentTarget = value;
        }
        internal virtual void Release() { }
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
        /// Init data
        /// </summary>
        protected virtual void OnInit() { }
        /// <summary>
        /// Release data
        /// </summary>
        protected virtual void OnRelease() { }
        internal sealed override void Release()
        {
            if (Pooled) return;
            pool.Push((T)this);
            OnRelease();
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

        public void Send(EventBase eventBase)
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
}
