using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Interface for class can be scheduled
    /// </summary>
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Get whether or not the scheduler has finished running for any reason.
        /// </summary>
        bool IsDone { get; }
        /// <summary>
        /// Update scheduler
        /// </summary>
        void Update();
        /// <summary>
        /// Stop a scheduler that is in-progress or paused. The scheduler's on completion callback will not be called.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Pause a running scheduler. A paused scheduler can be resumed from the same point it was paused.
        /// </summary>
        void Pause();
        /// <summary>
        /// Continue a paused scheduler. Does nothing if the scheduler has not been paused.
        /// </summary>
        void Resume();
    }
}