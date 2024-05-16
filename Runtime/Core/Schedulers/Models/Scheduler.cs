using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Main api for schedulers
    /// </summary>
    public static class Scheduler
    {
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static SchedulerHandle Delay(Action callBack, float delay)
        {
            var timer = Timer.Register(delay, callBack);
            var handle = SchedulerRunner.Instance.CreateHandle(timer);
            timer.OnComplete += () => SchedulerRunner.Instance.ReleaseScheduler(handle.TaskId);
            return handle;
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static SchedulerHandle Delay(Action callBack, Action<float> onUpdate, float delay)
        {
            var timer = Timer.Register(delay, callBack, onUpdate);
            var handle = SchedulerRunner.Instance.CreateHandle(timer);
            timer.OnComplete += () => SchedulerRunner.Instance.ReleaseScheduler(handle.TaskId);
            return handle;
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static SchedulerHandle WaitFrame(Action callBack, int frame)
        {
            var counter = FrameCounter.Register(frame, callBack);
            var handle = SchedulerRunner.Instance.CreateHandle(counter);
            counter.OnComplete += () => SchedulerRunner.Instance.ReleaseScheduler(handle.TaskId);
            return handle;
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static SchedulerHandle WaitFrame(Action callBack, Action<int> onUpdate, int frame)
        {
            var counter = FrameCounter.Register(frame, callBack, onUpdate);
            var handle = SchedulerRunner.Instance.CreateHandle(counter);
            counter.OnComplete += () => SchedulerRunner.Instance.ReleaseScheduler(handle.TaskId);
            return handle;
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
