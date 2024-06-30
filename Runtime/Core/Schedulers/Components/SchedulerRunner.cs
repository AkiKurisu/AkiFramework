using System;
using System.Collections.Generic;
using Kurisu.Framework.Pool;
using UnityEngine;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Manages updating all the <see cref="IScheduled"/> tasks that are running in the scene.
    /// This will be instantiated the first time you create a task.
    /// You do not need to add it into the scene manually.
    /// </summary>
    /// <remarks>
    /// Currently only work on Update().
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    internal class SchedulerRunner : MonoBehaviour
    {
        internal class ScheduledItem : IDisposable
        {
            private static readonly _ObjectPool<ScheduledItem> pool = new(() => new());
            public int Id { get; private set; }
            public double Timestamp { get; private set; }
            public IScheduled Value { get; private set; }
            public static ScheduledItem GetPooled(int id, IScheduled scheduled)
            {
                var item = pool.Get();
                item.Id = id;
                item.Value = scheduled;
                item.Timestamp = Time.timeSinceLevelLoadAsDouble;
                return item;
            }
            public void Cancel()
            {
                if (!Value.IsDone) Value.Cancel();
            }

            public void Dispose()
            {
                Value?.Dispose();
                Value = default;
                Id = default;
                Timestamp = default;
                pool.Release(this);
            }
        }
        private const int ManagedCapacity = 200;
        private const int RunningCapacity = 100;
        internal readonly Dictionary<IScheduled, ScheduledItem> managedScheduled = new(ManagedCapacity);
        internal List<ScheduledItem> scheduledRunning = new(RunningCapacity);
        //Start from id=1, should not be 0 since it roles as default/invalid task symbol
        private int taskId = 1;
        // buffer adding tasks so we don't edit a collection during iteration
        private readonly List<ScheduledItem> scheduledToAdd = new(RunningCapacity);
        private bool isDestroyed;
        private bool isGateOpen;
        public static SchedulerRunner Instance => instance != null ? instance : GetInstance();
        public static bool IsInitialized => instance != null;
        private static SchedulerRunner instance;
        private static SchedulerRunner GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
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
                    GameObject managerObject = new() { name = nameof(SchedulerRunner) };
                    instance = managerObject.AddComponent<SchedulerRunner>();
                }
            }
            return instance;
        }
        /// <summary>
        /// Register scheduled task to managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Register(IScheduled scheduled, Delegate @delegate)
        {
            if (isDestroyed)
            {
                Debug.LogWarning("Can not schedule task when scene is destroying.");
                scheduled.Dispose();
                return;
            }
            if (taskId == int.MaxValue) taskId = 1;
            managedScheduled.Add(scheduled, ScheduledItem.GetPooled(taskId++, scheduled));
            (isGateOpen ? scheduledRunning : scheduledToAdd).Add(managedScheduled[scheduled]);
#if UNITY_EDITOR
            SchedulerRegistry.RegisterListener(scheduled, @delegate);
#endif
        }
        /// <summary>
        ///  Unregister scheduled task from managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Unregister(IScheduled scheduled, Delegate @delegate)
        {
            managedScheduled.Remove(scheduled);
#if UNITY_EDITOR
            SchedulerRegistry.UnregisterListener(scheduled, @delegate);
#endif
        }
        /// <summary>
        /// Cancel all scheduled task
        /// </summary>
        public void CancelAll()
        {
            foreach (ScheduledItem scheduled in scheduledRunning)
            {
                scheduled.Cancel();
                if (isGateOpen)
                {
                    scheduled.Dispose();
                }
            }
            if (isGateOpen)
            {
                scheduledRunning.Clear();
            }
            scheduledToAdd.Clear();
        }
        /// <summary>
        /// Pause all scheduled task
        /// </summary>
        public void PauseAll()
        {
            foreach (ScheduledItem scheduled in scheduledRunning)
            {
                scheduled.Value.Pause();
            }
        }
        /// <summary>
        /// Resume all scheduled task
        /// </summary>
        public void ResumeAll()
        {
            foreach (ScheduledItem scheduled in scheduledRunning)
            {
                scheduled.Value.Resume();
            }
        }

        private void Update()
        {
            isGateOpen = false;
            UpdateAll();
            isGateOpen = true;
        }
        private void OnDestroy()
        {
            isDestroyed = true;
            foreach (ScheduledItem scheduled in scheduledRunning)
            {
                scheduled.Cancel();
                scheduled.Dispose();
            }
            SchedulerRegistry.CleanListeners();
            managedScheduled.Clear();
            scheduledRunning.Clear();
            scheduledToAdd.Clear();
        }

        private void UpdateAll()
        {
            // Add
            if (scheduledToAdd.Count > 0)
            {
                foreach (var scheduled in scheduledToAdd)
                    scheduledRunning.Add(scheduled);
                scheduledToAdd.Clear();
            }
            // Update
            foreach (ScheduledItem item in scheduledRunning)
            {
                if (!item.Value.IsDone)
                    item.Value.Update();
            }
            // Release
            for (int i = scheduledRunning.Count - 1; i >= 0; i--)
            {
                if (!scheduledRunning[i].Value.IsDone) continue;
                scheduledRunning[i].Dispose();
                scheduledRunning.RemoveAt(i);
            }
        }
        public SchedulerHandle CreateHandle(IScheduled task)
        {
            return new SchedulerHandle(managedScheduled[task].Id);
        }
        /// <summary>
        /// Whether scheduled task is valid
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool IsValid(int taskId)
        {
            foreach (var value in managedScheduled.Values)
            {
                if (value.Id == taskId) return true;
            }
            return false;
        }
        /// <summary>
        /// Get internal scheduled task by <see cref="taskId"/>
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGet(int taskId, out IScheduled task)
        {
            foreach (var value in managedScheduled.Values)
            {
                if (value.Id == taskId)
                {
                    task = value.Value;
                    return true;
                }
            }
            task = null;
            return false;
        }
        private bool TryGetItem(int taskId, out ScheduledItem item)
        {
            foreach (var value in managedScheduled.Values)
            {
                if (value.Id == taskId)
                {
                    item = value;
                    return true;
                }
            }
            item = null;
            return false;
        }
        /// <summary>
        /// Cancel target scheduled task
        /// </summary>
        /// <param name="taskId"></param>
        public void Cancel(int taskId)
        {
            if (!TryGetItem(taskId, out ScheduledItem item)) return;
            item.Cancel();
            if (isGateOpen)
            {
                scheduledRunning.Remove(item);
                item.Dispose();
            }
        }
    }
}