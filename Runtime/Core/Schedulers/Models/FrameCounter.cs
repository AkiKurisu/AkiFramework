using UnityEngine;
using System;
using Kurisu.Framework.Pool;
namespace Kurisu.Framework.Schedulers
{
    internal class FrameCounter : IScheduled
    {
        private static readonly _ObjectPool<FrameCounter> pool = new(() => new());
        #region Public Properties/Fields
        public SchedulerHandle Handle { get; private set; }
        /// <summary>
        /// How many frame the counter takes to complete from start to finish.
        /// </summary>
        public int Frame { get; private set; }
        private int count;
        /// <summary>
        /// Whether the counter will run again after completion.
        /// </summary>
        public bool IsLooped { get; set; }

        /// <summary>
        /// Whether or not the counter completed running. This is false if the counter was cancelled.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Whether the counter is currently paused.
        /// </summary>
        public bool IsPaused
        {
            get { return _timeElapsedBeforePause.HasValue; }
        }

        /// <summary>
        /// Whether or not the counter was cancelled.
        /// </summary>
        public bool IsCancelled
        {
            get { return _timeElapsedBeforeCancel.HasValue; }
        }

        public bool IsDone
        {
            get { return IsCompleted || IsCancelled; }
        }

        #endregion
        #region Public Static Methods
        /// <summary>
        /// Register a new counter that should fire an event after a certain amount of frame
        /// has elapsed.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="onComplete"></param>
        /// <param name="onUpdate"></param>
        /// <param name="tickFrame"></param>
        /// <param name="isLooped"></param>
        /// <returns></returns>
        internal static FrameCounter Register(int frame, SchedulerUnsafeBinding onComplete, SchedulerUnsafeBinding<int> onUpdate,
         TickFrame tickFrame = TickFrame.Update, bool isLooped = false)
        {
            FrameCounter timer = pool.Get();
            timer.Init(SchedulerRunner.Instance.NewHandle(), frame, ref onComplete, ref onUpdate, isLooped);
            SchedulerRunner.Instance.Register(timer, tickFrame, onComplete.IsValid() ? onComplete.GetDelegate() : onUpdate.GetDelegate());
            return timer;
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Stop a counter that is in-progress or paused. The counter's on completion callback will not be called.
        /// </summary>
        public void Cancel()
        {
            if (IsDone) return;
            _timeElapsedBeforeCancel = Time.time;
            _timeElapsedBeforePause = null;
        }
        public void Dispose()
        {
            SchedulerRunner.Instance.Unregister(this, _onComplete.IsValid() ? _onComplete.GetDelegate() : _onUpdate.GetDelegate());
            _onUpdate = default;
            _onComplete = default;
            pool.Release(this);
        }
        /// <summary>
        /// Pause a running counter. A paused counter can be resumed from the same point it was paused.
        /// </summary>
        public void Pause()
        {
            if (IsPaused || IsDone)
            {
                return;
            }

            _timeElapsedBeforePause = Time.time;
        }

        /// <summary>
        /// Continue a paused counter. Does nothing if the counter has not been paused.
        /// </summary>
        public void Resume()
        {
            if (!IsPaused || IsDone)
            {
                return;
            }

            _timeElapsedBeforePause = null;
        }

        /// <summary>
        /// Get how many frame remain before the counter completes.
        /// </summary>
        /// <returns></returns>
        public float GetFrameRemaining()
        {
            return Frame - count;
        }

        #endregion
        #region Private Properties/Fields
        private SchedulerUnsafeBinding _onComplete;
        private SchedulerUnsafeBinding<int> _onUpdate;
        private float? _timeElapsedBeforeCancel;
        private float? _timeElapsedBeforePause;
        #endregion
        #region Private Constructor
        private void Init(SchedulerHandle handle, int frame, ref SchedulerUnsafeBinding onComplete, ref SchedulerUnsafeBinding<int> onUpdate, bool isLooped)
        {
            Handle = handle;
            Frame = frame;
            _onComplete = onComplete;
            _onUpdate = onUpdate;

            IsLooped = isLooped;

            count = 0;

            IsCompleted = false;
            _timeElapsedBeforeCancel = null;
            _timeElapsedBeforePause = null;
        }

        #endregion

        public void Update()
        {
            if (IsDone)
            {
                return;
            }

            if (IsPaused)
            {
                return;
            }

            ++count;

            _onUpdate.Invoke(count);

            if (count >= Frame)
            {
                _onComplete.Invoke();

                if (IsLooped)
                {
                    count = 0;
                }
                else
                {
                    IsCompleted = true;
                }
            }
        }

    }
}