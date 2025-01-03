using System;
using System.Collections.Generic;
using Chris.Collections;
using Chris.Pool;
using Unity.Profiling;
using UnityEngine;
namespace Chris.Schedulers
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
        /// <summary>
        /// Class for easier dispose control
        /// </summary>
        internal class ScheduledItem : IDisposable
        {
            private static readonly _ObjectPool<ScheduledItem> pool = new(() => new());
#if UNITY_EDITOR
            public double Timestamp { get; private set; }
#endif
            public IScheduled Value { get; private set; }
            private bool delay;
            public TickFrame TickFrame { get; private set; }
            private static readonly ProfilerMarker profilerMarker = new("SchedulerRunner.UpdateAll.UpdateStep.UpdateItem");
            public static ScheduledItem GetPooled(IScheduled scheduled, TickFrame tickFrame, bool delay)
            {
                var item = pool.Get();
                item.Value = scheduled;
#if UNITY_EDITOR
                item.Timestamp = Time.timeSinceLevelLoadAsDouble;
#endif
                item.delay = delay;
                item.TickFrame = tickFrame;
                return item;
            }
            /// <summary>
            /// Whether internal scheduled task is done
            /// </summary>
            /// <returns></returns>
            public bool IsDone() => Value.IsDone;
            public void Update(TickFrame tickFrame)
            {
                using (profilerMarker.Auto())
                {
                    if (Value.IsDone) return;
                    if (TickFrame != tickFrame) return;
                    if (delay)
                    {
                        delay = false;
                        return;
                    }
                    Value.Update();
                }
            }
            /// <summary>
            /// Cancel internal scheduled task
            /// </summary>
            public void Cancel()
            {
                if (!Value.IsDone) Value.Cancel();
            }
            /// <summary>
            /// Dispose self and internal scheduled task
            /// </summary>
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
            public void Pause()
            {
                Value.Pause();
            }
            public void Resume()
            {
                Value.Resume();
            }
        }
        private const int InitialCapacity = 100;
        internal SparseList<ScheduledItem> scheduledItems = new(InitialCapacity, SchedulerHandle.MaxIndex + 1);
        private ulong serialNum = 1;
        // buffer adding tasks so we don't edit a collection during iteration
        private readonly List<SchedulerHandle> pendingHandles = new(InitialCapacity);
        private readonly List<SchedulerHandle> activeHandles = new(InitialCapacity);
        private bool isDestroyed;
        private bool isGateOpen;
        private int lastFrame;
        public static bool IsInitialized => instance != null;
        private static SchedulerRunner instance;
        private static readonly ProfilerMarker UpdateStepPM = new("SchedulerRunner.UpdateAll.UpdateStep");
        public static SchedulerRunner Get()
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
            UpdateAll(TickFrame.Update);

        }
        private void FixedUpdate()
        {
            UpdateAll(TickFrame.FixedUpdate);
        }

        private void LateUpdate()
        {
            UpdateAll(TickFrame.LateUpdate);
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
            pendingHandles.Clear();
        }
        /// <summary>
        /// Register scheduled task to managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Register(IScheduled scheduled, TickFrame tickFrame, Delegate @delegate)
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
            var item = ScheduledItem.GetPooled(scheduled, tickFrame, needDelayFrame);
            // Assign item
            scheduledItems[index] = item;
            pendingHandles.Add(scheduled.Handle);
#if UNITY_EDITOR && !AF_SCHEDULER_STACK_TRACE_DISABLE
            SchedulerRegistry.RegisterListener(scheduled, @delegate);
#endif
        }
        public SchedulerHandle NewHandle()
        {
            // Allocate placement, not really add
            return new SchedulerHandle(serialNum, scheduledItems.AddUninitialized());
        }
        /// <summary>
        ///  Unregister scheduled task from managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Unregister(IScheduled scheduled, Delegate @delegate)
        {
            scheduledItems.RemoveAt(scheduled.Handle.GetIndex());
#if UNITY_EDITOR && !AF_SCHEDULER_STACK_TRACE_DISABLE
            SchedulerRegistry.UnregisterListener(scheduled, @delegate);
#endif
        }
        /// <summary>
        /// Cancel all scheduled task
        /// </summary>
        public void CancelAll()
        {
            foreach (var handle in activeHandles)
            {
                var item = FindItem(handle);
                item.Cancel();
                if (isGateOpen)
                {
                    item.Dispose();
                }
            }
            if (isGateOpen)
            {
                activeHandles.Clear();
            }
            pendingHandles.Clear();
        }
        /// <summary>
        /// Pause all scheduled task
        /// </summary>
        public void PauseAll()
        {
            foreach (var handle in pendingHandles)
            {
                var item = FindItem(handle);
                item.Pause();
            }
            foreach (var handle in activeHandles)
            {
                var item = FindItem(handle);
                item.Pause();
            }
        }
        /// <summary>
        /// Resume all scheduled task
        /// </summary>
        public void ResumeAll()
        {
            foreach (var handle in pendingHandles)
            {
                var item = FindItem(handle);
                item.Resume();
            }
            foreach (var handle in activeHandles)
            {
                var item = FindItem(handle);
                item.Resume();
            }
        }
        private void UpdateAll(TickFrame tickFrame)
        {
            isGateOpen = false;
            // Add
            if (pendingHandles.Count > 0)
            {
                activeHandles.AddRange(pendingHandles);
                pendingHandles.Clear();
                // increase serial
                serialNum++;
            }

            // Update
            using (UpdateStepPM.Auto())
            {
                for (int i = activeHandles.Count - 1; i >= 0; --i)
                {
                    var item = FindItem(activeHandles[i]);
                    item.Update(tickFrame);
                    if (item.IsDone())
                    {
                        activeHandles.RemoveAt(i);
                        item.Dispose();
                    }
                }
            }

            // Shrink
            if (tickFrame == TickFrame.LateUpdate)
            {
                // check match shrink threshold that capacity is more than initial capacity
                bool canShrink = scheduledItems.InternalCapacity > 2 * InitialCapacity;
                // shrink list when non-allocated elements are far more than allocated ones
                bool overlapAllocated = scheduledItems.NumFreeIndices > 4 * scheduledItems.Count;
                if (canShrink && overlapAllocated)
                {
                    scheduledItems.Shrink();
                }
            }
            isGateOpen = true;
        }
        private ScheduledItem FindItem(SchedulerHandle handle)
        {
            int handleIndex = handle.GetIndex();
            ulong handleSerial = handle.GetSerialNumber();
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
            var item = FindItem(handle);
            if (item == null) return true;
            return item.IsDone();
        }
        /// <summary>
        /// Cancel target scheduled task
        /// </summary>
        /// <param name="handle"></param>
        public void Cancel(SchedulerHandle handle)
        {
            var item = FindItem(handle);
            if (item == null) return;
            item.Cancel();
            // ensure pending buffer also remove task
            if (pendingHandles.Remove(handle))
            {
                item.Dispose();
            }
            else if (isGateOpen)
            {
                activeHandles.Remove(handle);
                item.Dispose();
            }
        }
        /// <summary>
        /// Pause target scheduled task
        /// </summary>
        /// <param name="handle"></param>
        public void Pause(SchedulerHandle handle)
        {
            var item = FindItem(handle);
            if (item == null) return;
            item.Pause();
        }
        /// <summary>
        /// Resume target scheduled task
        /// </summary>
        /// <param name="handle"></param>
        public void Resume(SchedulerHandle handle)
        {
            var item = FindItem(handle);
            if (item == null) return;
            item.Resume();
        }
    }
}