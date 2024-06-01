using UnityEngine;
using System;
using UnityEngine.Pool;
namespace Kurisu.Framework.Schedulers
{
    public class FrameCounter : IScheduled
    {
        private static readonly ObjectPool<FrameCounter> pool = new(() => new());
        #region Public Properties/Fields
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
        internal static FrameCounter Register(int frame, Action onComplete, Action<int> onUpdate = null,
            bool isLooped = false)
        {
            FrameCounter timer = pool.Get();
            timer.Init(frame, onComplete, onUpdate, isLooped);
            SchedulerRunner.Instance.Register(timer, onComplete == null ? onUpdate : onComplete);
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
            SchedulerRunner.Instance.Unregister(this, OnComplete == null ? _onUpdate : OnComplete);
            _onUpdate = null;
            OnComplete = null;
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
        private Action OnComplete;
        private Action<int> _onUpdate;
        private float? _timeElapsedBeforeCancel;
        private float? _timeElapsedBeforePause;
        #endregion
        #region Private Constructor
        private void Init(int frame, Action onComplete, Action<int> onUpdate, bool isLooped)
        {
            Frame = frame;
            OnComplete = onComplete;
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

            _onUpdate?.Invoke(count);

            if (count >= Frame)
            {
                SchedulerRunner.Instance.Unregister(this, OnComplete == null ? _onUpdate : OnComplete);
                OnComplete?.Invoke();

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