using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.Schedulers;
using UnityEngine;
using UnityEngine.Animations;
namespace Kurisu.Framework.Animations
{
    public class AnimationNotifyEvent : EventBase<AnimationNotifyEvent>
    {
        public AnimationNotifier Notifier { get; private set; }
        public static AnimationNotifyEvent GetPooled(AnimationNotifier notifier)
        {
            var evt = GetPooled();
            evt.Notifier = notifier;
            return evt;
        }
    }
    /// <summary>
    /// Class for define an notifier for animation proxy montage
    /// </summary>
    public class AnimationNotifier
    {
        /// <summary>
        /// Notify name
        /// </summary>
        public string Name;
        /// <summary>
        /// Animator layer to observe if montage use animator controller
        /// </summary>
        public int Layer = 0;
        /// <summary>
        /// Normalized time to observe, do not observe time if less than zero
        /// </summary>
        public float NormalizedTime = -1;
        public AnimationNotifier()
        {

        }
        public AnimationNotifier(string name, int layer = 0, float normalizedTime = -1)
        {
            Name = name;
            Layer = layer;
            NormalizedTime = normalizedTime;
        }
        public virtual bool CanNotify(AnimationProxy animationProxy, LayerHandle layerHandle, float lastTime)
        {
            if (NormalizedTime < 0) return true;
            float currentTime = animationProxy.GetLeafAnimationNormalizedTime(layerHandle, Layer);
            float duration = animationProxy.GetLeafAnimationDuration(layerHandle, Layer);
            if (currentTime >= NormalizedTime)
            {
                if (lastTime < NormalizedTime) return true;
                /* Case when last tick time is in last loop */
                if (lastTime > currentTime)
                {
                    /* Validate if interval is less than 2 frames */
                    if ((1 - lastTime + currentTime) * duration < Time.deltaTime * 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    /// <summary>
    /// Notifier with specific animation state hash
    /// </summary>
    public class AnimationNotifier_AnimationState : AnimationNotifier
    {
        public int StateHash;
        public AnimationNotifier_AnimationState()
        {

        }
        public AnimationNotifier_AnimationState(string name, string stateName, int layer = 0, float normalizedTime = -1)
        : base(name, layer, normalizedTime)
        {
            StateHash = Animator.StringToHash(stateName);
        }
        public AnimationNotifier_AnimationState(string name, int stateHash, int layer = 0, float normalizedTime = -1)
        : base(name, layer, normalizedTime)
        {
            StateHash = stateHash;
        }
        public override bool CanNotify(AnimationProxy animationProxy, LayerHandle layerHandle, float lastTime)
        {
            var playable = animationProxy.GetLeafPlayable(layerHandle);
            // Must use animator controller
            if (!playable.IsPlayableOfType<AnimatorControllerPlayable>()) return false;
            // Check time reach
            if (!base.CanNotify(animationProxy, layerHandle, lastTime)) return false;
            // Check state match
            return animationProxy.GetAnimatorControllerInstanceProxy(layerHandle)
                                 .GetCurrentAnimatorStateInfo(Layer).shortNameHash == StateHash;
        }
    }
    public partial class AnimationProxy
    {
        public class AnimationNotifierContext
        {
            public AnimationNotifier Notifier;
            public LayerHandle LayerHandle;
            public float LastTime;
        }
        private class AnimationEventHandler : CallbackEventHandler
        {
            public override IEventCoordinator Root => EventSystem.Instance;
            public override void SendEvent(EventBase e, DispatchMode dispatchMode = DispatchMode.Default)
            {
                e.Target = this;
                EventSystem.Instance.Dispatch(e, dispatchMode, MonoDispatchType.LateUpdate);
            }
        }
        private readonly List<AnimationNotifierContext> notifierContexts = new();
        private SchedulerHandle eventTrackerTickHandle;
        private AnimationEventHandler eventTracker;
        public CallbackEventHandler GetEventHandler()
        {
            return eventTracker ??= new AnimationEventHandler();
        }
        public void AddNotifier(AnimationNotifier animationNotifier, LayerHandle layerHandle = default)
        {
            notifierContexts.Add(new AnimationNotifierContext()
            {
                Notifier = animationNotifier,
                LayerHandle = layerHandle,
                LastTime = 1
            });
            if (!eventTrackerTickHandle.IsValid())
            {
                Scheduler.WaitFrame(ref eventTrackerTickHandle, 1, TickEvents, TickFrame.LateUpdate, true);
            }
        }
        public void RemoveNotifier(string Name, LayerHandle layerHandle = default)
        {
            int inLayerIndex = GetLayerIndex(layerHandle);
            for (int i = notifierContexts.Count - 1; i >= 0; i--)
            {
                if (notifierContexts[i].Notifier.Name == Name && GetLayerIndex(notifierContexts[i].LayerHandle) == inLayerIndex)
                {
                    notifierContexts.RemoveAt(i);
                    break;
                }
            }
            if (notifierContexts.Count == 0)
            {
                eventTrackerTickHandle.Cancel();
            }
        }
        private void TickEvents(int frame)
        {
            for (int i = 0; i < notifierContexts.Count; ++i)
            {
                var context = notifierContexts[i];
                var notifier = context.Notifier;
                float normalizedTime = GetLeafAnimationNormalizedTime(context.LayerHandle, notifier.Layer);

                /* Whether event should be fired */
                if (notifier.CanNotify(this, context.LayerHandle, context.LastTime))
                {
                    using var evt = AnimationNotifyEvent.GetPooled(notifier);
                    GetEventHandler().SendEvent(evt);
                }
                context.LastTime = normalizedTime;
            }
        }
        public void RegisterNotifyCallback(EventCallback<AnimationNotifyEvent> callback)
        {
            GetEventHandler().RegisterCallback(callback, default);
        }
        public void UnregisterNotifyCallback(EventCallback<AnimationNotifyEvent> callback)
        {
            GetEventHandler().UnregisterCallback(callback, default);
        }
    }
}
