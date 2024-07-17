using System;
using System.Collections.Generic;
using Kurisu.Framework.Collections;
using Kurisu.Framework.Pool;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Manages updating all the <see cref="IScheduled"/> tasks that are running in the scene.
    /// This will be instantiated the first time you create a task.
    /// You do not need to add it into the scene manually. Similar to Unreal's TimerManager.
    /// </summary>
    /// <remarks>
    /// Currently only work on Update().
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    internal class SchedulerRunner : MonoBehaviour
    {
        // not use struct for easier dispose control
        internal class ScheduledItem : IDisposable
        {
            private static readonly _ObjectPool<ScheduledItem> pool = new(() => new());
#if UNITY_EDITOR
            public double Timestamp { get; private set; }
#endif
            public IScheduled Value { get; private set; }
            private bool delay;
            public static ScheduledItem GetPooled(IScheduled scheduled, bool delay)
            {
                var item = pool.Get();
                item.Value = scheduled;
#if UNITY_EDITOR
                item.Timestamp = Time.timeSinceLevelLoadAsDouble;
#endif
                item.delay = delay;
                return item;
            }
            public bool IsDone() => Value.IsDone;
            public void Update()
            {
                if (Value.IsDone) return;
                if (delay)
                {
                    delay = false;
                    return;
                }
                Value.Update();
            }
            public void Cancel()
            {
                if (!Value.IsDone) Value.Cancel();
            }
            public void Dispose()
            {
                Value?.Dispose();
                Value = default;
#if UNITY_EDITOR
                Timestamp = default;
#endif
                delay = default;
                pool.Release(this);
            }
        }
        private const int InitialCapacity = 100;
        internal SparseList<ScheduledItem> scheduledItems = new(InitialCapacity, SchedulerHandle.MaxIndex + 1);
        private uint serialNum = 0;
        // buffer adding tasks so we don't edit a collection during iteration
        private readonly List<ScheduledItem> scheduledToAdd = new(InitialCapacity);
        private bool isDestroyed;
        private bool isGateOpen;
        private int lastFrame;
        public static SchedulerRunner Instance => instance != null ? instance : GetInstance();
        public static bool IsInitialized => instance != null;
        private static SchedulerRunner instance;
        private static SchedulerRunner GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogError("[Scheduler] Scheduler can not be used in Editor Mode.");
                return null;
            }
#endif
            if (instance == null)
            {
                SchedulerRunner managerInScene = FindObjectOfType<SchedulerRunner>();
                if (managerInScene != null)
                {
                    instance = managerInScene;
                }
                else
                {
                    instance = new GameObject(nameof(SchedulerRunner)).AddComponent<SchedulerRunner>();
                }
            }
            return instance;
        }
        private void Update()
        {
            isGateOpen = false;
            UpdateAll();
            isGateOpen = true;
            lastFrame = Time.frameCount;
        }
        private void OnDestroy()
        {
            isDestroyed = true;
            foreach (ScheduledItem scheduled in scheduledItems)
            {
                scheduled.Cancel();
                scheduled.Dispose();
            }
            SchedulerRegistry.CleanListeners();
            scheduledItems.Clear();
            scheduledToAdd.Clear();
        }
        /// <summary>
        /// Register scheduled task to managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Register(IScheduled scheduled, Delegate @delegate)
        {
            if (isDestroyed)
            {
                Debug.LogWarning("[Scheduler] Can not schedule task when scene is destroying.");
                scheduled.Dispose();
                return;
            }
            // schedule one frame if register before runner update
            bool needDelayFrame = lastFrame < Time.frameCount;
            int index = scheduled.Handle.GetIndex();
            var item = ScheduledItem.GetPooled(scheduled, needDelayFrame);
            if (isGateOpen)
            {
                scheduledItems[index] = item;
            }
            else
            {
                scheduledToAdd.Add(item);
            }
#if UNITY_EDITOR
            SchedulerRegistry.RegisterListener(scheduled, @delegate);
#endif
        }
        public SchedulerHandle NewHandle()
        {
            // Allocate placement, not really add
            return new SchedulerHandle(scheduledItems.AddUninitialized(), serialNum);
        }
        /// <summary>
        ///  Unregister scheduled task from managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Unregister(IScheduled scheduled, Delegate @delegate)
        {
            scheduledItems.RemoveAt(scheduled.Handle.GetIndex());
#if UNITY_EDITOR
            SchedulerRegistry.UnregisterListener(scheduled, @delegate);
#endif
        }
        /// <summary>
        /// Cancel all scheduled task
        /// </summary>
        public void CancelAll()
        {
            foreach (ScheduledItem scheduled in scheduledItems)
            {
                scheduled.Cancel();
                if (isGateOpen)
                {
                    scheduled.Dispose();
                }
            }
            if (isGateOpen)
            {
                scheduledItems.Clear();
            }
            scheduledToAdd.Clear();
        }
        /// <summary>
        /// Pause all scheduled task
        /// </summary>
        public void PauseAll()
        {
            foreach (ScheduledItem scheduled in scheduledItems)
            {
                scheduled.Value.Pause();
            }
        }
        /// <summary>
        /// Resume all scheduled task
        /// </summary>
        public void ResumeAll()
        {
            foreach (ScheduledItem scheduled in scheduledItems)
            {
                scheduled.Value.Resume();
            }
        }
        private void UpdateAll()
        {
            // Add
            if (scheduledToAdd.Count > 0)
            {
                foreach (var scheduled in scheduledToAdd)
                {
                    // Assign value
                    scheduledItems[scheduled.Value.Handle.GetIndex()] = scheduled;
                }
                scheduledToAdd.Clear();
            }

            // increase serial
            serialNum++;

            // Update
            var releaseIndex = ListPool<int>.Get();
            foreach (ScheduledItem item in scheduledItems)
            {
                item.Update();
                if (item.IsDone())
                {
                    releaseIndex.Add(item.Value.Handle.GetIndex());
                }
            }

            // Release
            foreach (int index in releaseIndex)
            {
                var item = scheduledItems[index];
                scheduledItems.RemoveAt(index);
                item.Dispose();
            }
            ListPool<int>.Release(releaseIndex);
        }
        /// <summary>
        /// Whether scheduled task is valid
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool IsValid(SchedulerHandle handle)
        {
            return GetScheduledItem(handle) != null;
        }
        private ScheduledItem GetScheduledItem(SchedulerHandle handle)
        {
            int handleIndex = handle.GetIndex();
            int handleSerial = handle.GetSerialNumber();
            // if is current serial which means create and cancel scheduled task in same frame
            if (handleSerial == serialNum)
            {
                // lookup add buffer
                foreach (var item in scheduledToAdd)
                {
                    if (item.Value.Handle == handle) return item;
                }
                return null;
            }
            var scheduledItem = scheduledItems[handleIndex];
            if (scheduledItem == null || scheduledItem.Value.Handle.GetSerialNumber() != handleSerial) return null;
            return scheduledItem;
        }
        /// <summary>
        /// Whether internal scheduled task is done
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool IsDone(SchedulerHandle handle)
        {
            var item = GetScheduledItem(handle);
            if (item == null) return true;
            return item.IsDone();
        }
        /// <summary>
        /// Cancel target scheduled task
        /// </summary>
        /// <param name="handle"></param>
        public void Cancel(SchedulerHandle handle)
        {
            var item = scheduledItems[handle.GetIndex()];
            if (item == null) return;
            item.Cancel();
            int index = item.Value.Handle.GetIndex();
            // ensure add buffer also remove task
            if (scheduledToAdd.Remove(item) || isGateOpen)
            {
                // need clear allocation though not really add yet
                scheduledItems.RemoveAt(index);
                item.Dispose();
            }
        }
    }
}