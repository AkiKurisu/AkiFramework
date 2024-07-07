using UnityEngine;
using UnityEngine.Playables;
namespace Kurisu.Framework.AI.Playables
{
    /// <summary>
    /// A task to fade in playable according to playable's duration
    /// </summary>
    public class FadeInPlayableTask : ITask
    {
        public TaskStatus Status { get; private set; } = TaskStatus.Enabled;
        private readonly Playable clipPlayable;
        private Playable mixerPlayable;
        private readonly float fadeInTime;
        public FadeInPlayableTask(Playable mixerPlayable, Playable clipPlayable, float fadeInTime)
        {
            this.clipPlayable = clipPlayable;
            this.mixerPlayable = mixerPlayable;
            this.fadeInTime = fadeInTime;
        }
        public void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                Status = TaskStatus.Disabled;
                return;
            }
            clipPlayable.SetSpeed(1d);
            double current = clipPlayable.GetTime();
            if (current >= fadeInTime)
            {
                mixerPlayable.SetInputWeight(0, 0);
                mixerPlayable.SetInputWeight(1, 1);
                Status = TaskStatus.Disabled;
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
    public class FadeOutPlayableTask : ITask
    {
        public TaskStatus Status { get; private set; } = TaskStatus.Enabled;
        private readonly Playable clipPlayable;
        private Playable mixerPlayable;
        private readonly float fadeOutTime;
        private readonly double duration;
        public FadeOutPlayableTask(Playable mixerPlayable, Playable clipPlayable, float fadeOutTime)
        {
            this.clipPlayable = clipPlayable;
            this.mixerPlayable = mixerPlayable;
            this.fadeOutTime = fadeOutTime;
            duration = clipPlayable.GetDuration();
        }
        public void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                Status = TaskStatus.Disabled;
                return;
            }
            double current = clipPlayable.GetTime();
            if (current >= duration)
            {
                mixerPlayable.SetInputWeight(0, 1);
                mixerPlayable.SetInputWeight(1, 0);
                Status = TaskStatus.Disabled;
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
    public class AbsolutelyFadeOutPlayableTask : ITask
    {
        public TaskStatus Status { get; private set; } = TaskStatus.Enabled;
        private Playable mixerPlayable;
        private readonly float fadeOutTime;
        private float timer;
        public AbsolutelyFadeOutPlayableTask(Playable mixerPlayable, float fadeOutTime)
        {
            this.mixerPlayable = mixerPlayable;
            this.fadeOutTime = fadeOutTime;
            timer = 0;
        }
        public void Tick()
        {
            if (!mixerPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                Status = TaskStatus.Disabled;
                return;
            }
            timer += Time.deltaTime;
            if (timer >= fadeOutTime)
            {
                mixerPlayable.SetInputWeight(0, 1);
                mixerPlayable.SetInputWeight(1, 0);
                Status = TaskStatus.Disabled;
            }
            else
            {
                float weight = timer / fadeOutTime;
                mixerPlayable.SetInputWeight(0, Mathf.Lerp(0, 1, weight));
                mixerPlayable.SetInputWeight(1, Mathf.Lerp(1, 0, weight));
            }
        }
    }
    public class WaitPlayableTask : ITask
    {
        public TaskStatus Status { get; private set; } = TaskStatus.Enabled;
        private readonly Playable clipPlayable;
        private readonly double waitTime;
        public WaitPlayableTask(Playable clipPlayable, double waitTime)
        {
            this.clipPlayable = clipPlayable;
            this.waitTime = waitTime;
        }
        public void Tick()
        {
            if (!clipPlayable.IsValid())
            {
                Debug.LogWarning("Playable is already destroyed");
                Status = TaskStatus.Disabled;
                return;
            }
            if (clipPlayable.GetTime() >= waitTime)
            {
                Status = TaskStatus.Disabled;
            }
        }
    }
}
