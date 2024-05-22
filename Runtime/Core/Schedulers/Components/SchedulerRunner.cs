using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Schedulers
{
    /// <summary>
    /// Manages updating all the <see cref="IScheduler"/>s that are running in the scene.
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
        private readonly Dictionary<IScheduler, int> scheduler2Id = new(ManagedCapacity);
        internal readonly Dictionary<int, IScheduler> managedSchedulers = new(ManagedCapacity);
        internal List<IScheduler> _scheduler = new(RunningCapacity);
        //Start from id=1, should not be 0 since it roles as default/invalid job symbol
        internal int schedulerId = 1;
        // buffer adding timers so we don't edit a collection during iteration
        private readonly List<IScheduler> _schedulerToAdd = new(RunningCapacity);
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
        /// <param name="scheduler"></param>
        public void RegisterScheduler(IScheduler scheduler)
        {
            _schedulerToAdd.Add(scheduler);
            if (debugMode)
            {
                Debug.Log($"Scheduler {scheduler.GetHashCode():x4} started");
            }
        }
        /// <summary>
        ///  Unregister scheduler from managed
        /// </summary>
        /// <param name="scheduler"></param>
        public void UnregisterScheduler(IScheduler scheduler)
        {
            if (scheduler2Id.TryGetValue(scheduler, out int id))
            {
                managedSchedulers.Remove(id);
            }
        }
        public void CancelAllSchedulers()
        {
            foreach (IScheduler scheduler in _scheduler)
            {
                scheduler.Cancel();
                if (debugMode)
                {
                    Debug.Log($"Scheduler {scheduler.GetHashCode():x4} canceled");
                }
            }
            _scheduler.Clear();
            _schedulerToAdd.Clear();
        }

        public void PauseAllSchedulers()
        {
            foreach (IScheduler scheduler in _scheduler)
            {
                scheduler.Pause();
                if (debugMode)
                {
                    Debug.Log($"Scheduler {scheduler.GetHashCode():x4} paused");
                }
            }
        }

        public void ResumeAllSchedulers()
        {
            foreach (IScheduler scheduler in _scheduler)
            {
                scheduler.Resume();
                if (debugMode)
                {
                    Debug.Log($"Scheduler {scheduler.GetHashCode():x4} resumed");
                }
            }
        }

        private void Update()
        {
            UpdateAllSchedulers();
        }

        private void UpdateAllSchedulers()
        {
            // Add
            if (_schedulerToAdd.Count > 0)
            {
                _scheduler.AddRange(_schedulerToAdd);
                _schedulerToAdd.Clear();
            }
            // Update
            foreach (IScheduler scheduler in _scheduler)
            {
                scheduler.Update();
            }
            // Release
            for (int i = _scheduler.Count - 1; i >= 0; i--)
            {
                if (!_scheduler[i].IsDone) continue;
                _scheduler[i].Dispose();
                if (debugMode)
                {
                    Debug.Log($"Scheduler {_scheduler[i].GetHashCode():x4} ended");
                }
                _scheduler.Remove(_scheduler[i]);
            }
        }
        public SchedulerHandle CreateHandle(IScheduler task)
        {
            int id = schedulerId++;
            var handle = new SchedulerHandle(id);
            managedSchedulers.Add(id, task);
            return handle;
        }
        /// <summary>
        /// Whether scheduler is valid, false means scheduler can not be used any more
        /// </summary>
        /// <param name="schedulerId"></param>
        /// <returns></returns>
        public bool IsValid(int schedulerId)
        {
            return managedSchedulers.ContainsKey(schedulerId);
        }
        /// <summary>
        /// Find internal scheduler if schedulerId is valid
        /// </summary>
        /// <param name="schedulerId"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public bool TryGet(int schedulerId, out IScheduler scheduler)
        {
            return managedSchedulers.TryGetValue(schedulerId, out scheduler);
        }
        /// <summary>
        /// Cancel target scheduler
        /// </summary>
        /// <param name="schedulerId"></param>
        public void CancelScheduler(int schedulerId)
        {
            var scheduler = managedSchedulers[schedulerId];
            if (debugMode)
            {
                Debug.Log($"Scheduler {scheduler.GetHashCode():x4} canceled");
            }
            scheduler.Cancel();
            managedSchedulers.Remove(schedulerId);
        }
    }
}