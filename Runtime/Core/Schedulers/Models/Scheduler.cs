using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Main api for schedulers
    /// </summary>
    public static class Scheduler
    {
        public static DateTimeOffset Now
        {
            get { return DateTimeOffset.UtcNow; }
        }

        public static TimeSpan Normalize(TimeSpan timeSpan)
        {
            return timeSpan >= TimeSpan.Zero ? timeSpan : TimeSpan.Zero;
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static SchedulerHandle Delay(float delay, Action callBack, bool ignoreTimeScale = false)
        {
            var timer = Timer.Register(delay, callBack, useRealTime: ignoreTimeScale);
            return SchedulerRunner.Instance.CreateHandle(timer);
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static SchedulerHandle Delay(float delay, Action callBack, Action<float> onUpdate, bool ignoreTimeScale = false)
        {
            var timer = Timer.Register(delay, callBack, onUpdate, useRealTime: ignoreTimeScale);
            return SchedulerRunner.Instance.CreateHandle(timer);
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static SchedulerHandle WaitFrame(int frame, Action callBack)
        {
            var counter = FrameCounter.Register(frame, callBack);
            return SchedulerRunner.Instance.CreateHandle(counter);
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static SchedulerHandle WaitFrame(int frame, Action callBack, Action<int> onUpdate)
        {
            var counter = FrameCounter.Register(frame, callBack, onUpdate);
            return SchedulerRunner.Instance.CreateHandle(counter);
        }
        /// <summary>
        /// Enable schedular debug mode
        /// </summary>
        /// <value></value>
        public static bool Debug
        {
            get => SchedulerRunner.Instance.DebugMode;
            set => SchedulerRunner.Instance.DebugMode = value;
        }
    }
}
