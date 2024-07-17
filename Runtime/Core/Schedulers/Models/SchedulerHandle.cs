using System;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Handle give you access to track scheduled task
    /// </summary>
    public readonly struct SchedulerHandle : IDisposable
    {
        /// <summary>
        /// Handle id for scheduled task
        /// </summary>
        /// <value></value>
        public long Handle { get; }
        public const int IndexBits = 24;
        public const int SerialNumberBits = 40;
        public const int MaxIndex = 1 << IndexBits;
        public const uint MaxSerialNumber = (uint)1 << SerialNumberBits;
        public int GetIndex() => (int)(Handle >> 40);
        public int GetSerialNumber() => (int)(Handle & 0xFFFFFFFFFF);
        /// <summary>
        /// Get scheduled task whether is valid
        /// </summary>
        /// <value></value>
        public readonly bool IsValid
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
                return SchedulerRunner.Instance.IsValid(this);
            }
        }
        /// <summary>
        /// Get scheduled task whether is done
        /// </summary>
        /// <value></value>
        public readonly bool IsDone
        {
            get
            {
                if (!SchedulerRunner.IsInitialized) return default;
                return SchedulerRunner.Instance.IsDone(this);
            }
        }
        public SchedulerHandle(int index, uint serialNum)
        {
            Handle = ((long)index << 40) | serialNum;
        }
        /// <summary>
        /// Cancel a scheduler if scheduler is valid
        /// </summary>
        /// <value></value>
        public void Cancel()
        {
            if (!SchedulerRunner.IsInitialized) return;
            if (!IsValid) return;
            SchedulerRunner.Instance.Cancel(this);
        }

        public void Dispose()
        {
            Cancel();
        }
        public static bool operator ==(SchedulerHandle left, SchedulerHandle right)
        {
            return left.Handle == right.Handle;
        }
        public static bool operator !=(SchedulerHandle left, SchedulerHandle right)
        {
            return left.Handle != right.Handle;
        }
        public override bool Equals(object obj)
        {
            if (obj is not SchedulerHandle handle) return false;
            return handle.Handle == Handle;
        }
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
    }
}