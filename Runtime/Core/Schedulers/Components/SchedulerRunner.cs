using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Manages updating all the <see cref="IScheduled"/> tasks that are running in the scene.
    /// This will be instantiated the first time you create a task.
    /// You do not need to add it into the scene manually.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    internal class SchedulerRunner : MonoBehaviour
    {
        [SerializeField]
        private bool debugMode = false;
        public bool DebugMode
        {
            get => debugMode;
            set => debugMode = value;
        }
        private const int ManagedCapacity = 200;
        private const int RunningCapacity = 100;
        private readonly Dictionary<IScheduled, int> scheduled2Id = new(ManagedCapacity);
        internal readonly Dictionary<int, IScheduled> managedScheduled = new(ManagedCapacity);
        internal List<IScheduled> _scheduled = new(RunningCapacity);
        //Start from id=1, should not be 0 since it roles as default/invalid task symbol
        internal int taskId = 1;
        // buffer adding tasks so we don't edit a collection during iteration
        private readonly List<IScheduled> _scheduledToAdd = new(RunningCapacity);
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
        /// Register scheduler to managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Register(IScheduled scheduled)
        {
            _scheduledToAdd.Add(scheduled);
            if (debugMode)
            {
                Debug.Log($"Scheduled {scheduled.GetHashCode():x4} started");
            }
        }
        /// <summary>
        ///  Unregister scheduler from managed
        /// </summary>
        /// <param name="scheduled"></param>
        public void Unregister(IScheduled scheduled)
        {
            if (scheduled2Id.TryGetValue(scheduled, out int id))
            {
                managedScheduled.Remove(id);
            }
        }
        public void CancelAll()
        {
            foreach (IScheduled scheduled in _scheduled)
            {
                scheduled.Cancel();
                if (debugMode)
                {
                    Debug.Log($"Scheduled {scheduled.GetHashCode():x4} canceled");
                }
            }
            _scheduled.Clear();
            _scheduledToAdd.Clear();
        }

        public void PauseAll()
        {
            foreach (IScheduled scheduled in _scheduled)
            {
                scheduled.Pause();
                if (debugMode)
                {
                    Debug.Log($"Scheduled {scheduled.GetHashCode():x4} paused");
                }
            }
        }

        public void ResumeAll()
        {
            foreach (IScheduled scheduled in _scheduled)
            {
                scheduled.Resume();
                if (debugMode)
                {
                    Debug.Log($"Scheduled {scheduled.GetHashCode():x4} resumed");
                }
            }
        }

        private void Update()
        {
            UpdateAll();
        }

        private void UpdateAll()
        {
            // Add
            if (_scheduledToAdd.Count > 0)
            {
                _scheduled.AddRange(_scheduledToAdd);
                _scheduledToAdd.Clear();
            }
            // Update
            foreach (IScheduled scheduled in _scheduled)
            {
                scheduled.Update();
            }
            // Release
            for (int i = _scheduled.Count - 1; i >= 0; i--)
            {
                if (!_scheduled[i].IsDone) continue;
                _scheduled[i].Dispose();
                if (debugMode)
                {
                    Debug.Log($"Scheduled {_scheduled[i].GetHashCode():x4} ended");
                }
                _scheduled.Remove(_scheduled[i]);
            }
        }
        public SchedulerHandle CreateHandle(IScheduled task)
        {
            int id = taskId++;
            var handle = new SchedulerHandle(id);
            scheduled2Id[task] = id;
            managedScheduled.Add(id, task);
            return handle;
        }
        /// <summary>
        /// Whether scheduled task is valid
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool IsValid(int taskId)
        {
            return managedScheduled.ContainsKey(taskId);
        }
        /// <summary>
        /// Get internal scheduled task by <see cref="taskId"/>
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGet(int taskId, out IScheduled task)
        {
            return managedScheduled.TryGetValue(taskId, out task);
        }
        /// <summary>
        /// Cancel target scheduled task
        /// </summary>
        /// <param name="taskId"></param>
        public void Cancel(int taskId)
        {
            var scheduler = managedScheduled[taskId];
            if (debugMode)
            {
                Debug.Log($"Scheduled {scheduler.GetHashCode():x4} canceled");
            }
            scheduler.Cancel();
            managedScheduled.Remove(taskId);
        }
    }
}