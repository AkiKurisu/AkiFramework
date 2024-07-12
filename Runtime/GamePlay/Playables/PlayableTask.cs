using Kurisu.Framework.Tasks;
using UnityEngine;
using UnityEngine.Playables;
namespace Kurisu.Framework.Playables.Tasks
{
    /// <summary>
    /// A task to fade in playable according to playable's duration
    /// </summary>
    public class FadeInPlayableTask : PooledTaskBase<FadeInPlayableTask>
    {
        private Playable clipPlayable;
        private Playable mixerPlayable;
        private float fadeInTime;
        public static FadeInPlayableTask GetPooled(Playable mixerPlayable, Playable clipPlayable, float fadeInTime)
        {
            var task = GetPooled();
            task.clipPlayable = clipPlayable;
            task.mixerPlayable = mixerPlayable;
            task.fadeInTime = fadeInTime;
            return task;
        }
        protected override void Init()
        {
            base.Init();
            mStatus = TaskStatus.Enabled;
        }
        public override void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                mStatus = TaskStatus.Disabled;
                return;
            }
            clipPlayable.SetSpeed(1d);
            double current = clipPlayable.GetTime();
            if (current >= fadeInTime)
            {
                mixerPlayable.SetInputWeight(0, 0);
                mixerPlayable.SetInputWeight(1, 1);
                mStatus = TaskStatus.Disabled;
            }
            else
            {
                float weight = (float)(current / fadeInTime);
                mixerPlayable.SetInputWeight(0, Mathf.Lerp(1, 0, weight));
                mixerPlayable.SetInputWeight(1, Mathf.Lerp(0, 1, weight));
            }
        }
    }
    /// <summary>
    /// A task to fade out playable according to playable's duration
    /// </summary>
    public class FadeOutPlayableTask : PooledTaskBase<FadeOutPlayableTask>
    {
        private Playable clipPlayable;
        private Playable mixerPlayable;
        private float fadeOutTime;
        private double duration;
        public static FadeOutPlayableTask GetPooled(Playable mixerPlayable, Playable clipPlayable, float fadeOutTime)
        {
            var task = GetPooled();
            task.clipPlayable = clipPlayable;
            task.mixerPlayable = mixerPlayable;
            task.fadeOutTime = fadeOutTime;
            task.duration = clipPlayable.GetDuration();
            return task;
        }
        protected override void Init()
        {
            base.Init();
            mStatus = TaskStatus.Enabled;
        }
        public override void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                mStatus = TaskStatus.Disabled;
                return;
            }
            double current = clipPlayable.GetTime();
            if (current >= duration)
            {
                mixerPlayable.SetInputWeight(0, 1);
                mixerPlayable.SetInputWeight(1, 0);
                mStatus = TaskStatus.Disabled;
            }
            else
            {
                float weight = 1 - (float)((duration - current) / fadeOutTime);
                mixerPlayable.SetInputWeight(0, Mathf.Lerp(0, 1, weight));
                mixerPlayable.SetInputWeight(1, Mathf.Lerp(1, 0, weight));
            }
        }
    }
    /// <summary>
    /// A task to fade out playable according to fadeOutTime 
    /// </summary>
    public class AbsolutelyFadeOutPlayableTask : PooledTaskBase<AbsolutelyFadeOutPlayableTask>
    {
        private Playable mixerPlayable;
        private float fadeOutTime;
        private float timer;
        public static AbsolutelyFadeOutPlayableTask GetPooled(Playable mixerPlayable, float fadeOutTime)
        {
            var task = GetPooled();
            task.mixerPlayable = mixerPlayable;
            task.fadeOutTime = fadeOutTime;
            task.timer = 0;
            return task;
        }
        protected override void Init()
        {
            base.Init();
            mStatus = TaskStatus.Enabled;
        }
        public override void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                mStatus = TaskStatus.Disabled;
                return;
            }
            timer += Time.deltaTime;
            if (timer >= fadeOutTime)
            {
                mixerPlayable.SetInputWeight(0, 1);
                mixerPlayable.SetInputWeight(1, 0);
                mStatus = TaskStatus.Disabled;
            }
            else
            {
                float weight = timer / fadeOutTime;
                mixerPlayable.SetInputWeight(0, Mathf.Lerp(0, 1, weight));
                mixerPlayable.SetInputWeight(1, Mathf.Lerp(1, 0, weight));
            }
        }
    }
    public class WaitPlayableTask : PooledTaskBase<WaitPlayableTask>
    {
        private Playable clipPlayable;
        private double waitTime;
        public static WaitPlayableTask GetPooled(Playable clipPlayable, double waitTime)
        {
            var task = GetPooled();
            task.clipPlayable = clipPlayable;
            task.waitTime = waitTime;
            return task;
        }
        protected override void Init()
        {
            base.Init();
            mStatus = TaskStatus.Enabled;
        }
        public override void Tick()
        {
            if (!clipPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                mStatus = TaskStatus.Disabled;
                return;
            }
            if (clipPlayable.GetTime() >= waitTime)
            {
                mStatus = TaskStatus.Disabled;
            }
        }
    }
}
