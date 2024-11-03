using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.Schedulers;
using UnityEngine.Animations;
using UnityEngine.Playables;
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
    public struct AnimationNotifier
    {
        public string Name;
        public float NormalizedTime;
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
                var notifier = notifierContexts[i].Notifier;
                var context = notifierContexts[i];
                var playable = GetLeafPlayable(notifierContexts[i].LayerHandle);

                float normalizedTime;
                if (playable.IsPlayableOfType<AnimatorControllerPlayable>())
                {
                    /* AnimatorControllerPlayable can get normalized time directly */
                    var proxy = GetAnimatorControllerInstanceProxy(context.LayerHandle);
                    normalizedTime = proxy.GetCurrentAnimatorStateInfo(0).normalizedTime;
                }
                else
                {
                    var proxy = GetAnimationClipInstanceProxy(context.LayerHandle);
                    var length = proxy.GetAnimationClip().length;
                    normalizedTime = (float)(playable.GetTime() % length);
                }

                /* Whether event should be fired */
                if (normalizedTime >= notifier.NormalizedTime && context.LastTime < notifier.NormalizedTime)
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
        public void UnregisterNotifyCallback<TEventType>(EventCallback<AnimationNotifyEvent> callback)
        {
            GetEventHandler().UnregisterCallback(callback, default);
        }
    }
}
