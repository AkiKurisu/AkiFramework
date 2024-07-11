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
        /// <param name="delay"></param>
        /// <param name="callBack"></param>
        /// <param name="isLooped"></param>
        /// <param name="ignoreTimeScale"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle Delay(float delay, Action callBack, bool isLooped = false, bool ignoreTimeScale = false)
        {
            return Timer.Register(delay, callBack, null, isLooped, ignoreTimeScale).CreateHandle();
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="onUpdate"></param>
        /// <param name="isLooped"></param>
        /// <param name="ignoreTimeScale"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle Delay(float delay, Action<float> onUpdate, bool isLooped = false, bool ignoreTimeScale = false)
        {
            return Timer.Register(delay, null, onUpdate, isLooped, ignoreTimeScale).CreateHandle();
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle Delay(float delay, Action callBack, Action<float> onUpdate, bool ignoreTimeScale = false)
        {
            return Timer.Register(delay, callBack, onUpdate, useRealTime: ignoreTimeScale).CreateHandle();
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle WaitFrame(int frame, Action callBack)
        {
            return FrameCounter.Register(frame, callBack).CreateHandle();
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="onUpdate"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle WaitFrame(int frame, Action<int> onUpdate)
        {
            return FrameCounter.Register(frame, null, onUpdate).CreateHandle();
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static SchedulerHandle WaitFrame(int frame, Action callBack, Action<int> onUpdate)
        {
            return FrameCounter.Register(frame, callBack, onUpdate).CreateHandle();
        }
        #region Unreal Style
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="delay"></param>
        /// <param name="callBack"></param>
        /// <param name="isLooped"></param>
        /// <param name="ignoreTimeScale"></param>
        [StackTraceFrame]
        public static void Delay(ref SchedulerHandle handle, float delay, Action callBack, bool isLooped = false, bool ignoreTimeScale = false)
        {
            Timer.Register(delay, callBack, null, isLooped, ignoreTimeScale).OverwriteHandle(ref handle);
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="delay"></param>
        /// <param name="onUpdate"></param>
        /// <param name="isLooped"></param>
        /// <param name="ignoreTimeScale"></param>
        /// <returns></returns>
        [StackTraceFrame]
        public static void Delay(ref SchedulerHandle handle, float delay, Action<float> onUpdate, bool isLooped = false, bool ignoreTimeScale = false)
        {
            Timer.Register(delay, null, onUpdate, isLooped, ignoreTimeScale).OverwriteHandle(ref handle);
        }
        /// <summary>
        /// Delay some time and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="delay"></param>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        /// <param name="ignoreTimeScale"></param>
        [StackTraceFrame]
        public static void Delay(ref SchedulerHandle handle, float delay, Action callBack, Action<float> onUpdate, bool ignoreTimeScale = false)
        {
            Timer.Register(delay, callBack, onUpdate, useRealTime: ignoreTimeScale).OverwriteHandle(ref handle);
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="frame"></param>
        /// <param name="callBack"></param>
        [StackTraceFrame]
        public static void WaitFrame(ref SchedulerHandle handle, int frame, Action callBack)
        {
            FrameCounter.Register(frame, callBack).OverwriteHandle(ref handle);
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="frame"></param>
        /// <param name="onUpdate"></param>
        [StackTraceFrame]
        public static void WaitFrame(ref SchedulerHandle handle, int frame, Action<int> onUpdate)
        {
            FrameCounter.Register(frame, null, onUpdate).OverwriteHandle(ref handle);
        }
        /// <summary>
        /// Wait some frames and invoke callBack
        /// </summary>
        /// <param name="handle">Handle to overwrite</param>
        /// <param name="frame"></param>
        /// <param name="callBack"></param>
        /// <param name="onUpdate"></param>
        [StackTraceFrame]
        public static void WaitFrame(ref SchedulerHandle handle, int frame, Action callBack, Action<int> onUpdate)
        {
            FrameCounter.Register(frame, callBack, onUpdate).OverwriteHandle(ref handle);
        }
        #endregion
    }
}
