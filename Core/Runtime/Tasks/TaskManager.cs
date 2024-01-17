using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    /// <summary>
    /// Manages updating all the <see cref="ITask"/>s that are running in the scene.
    /// This will be instantiated the first time you create a task.
    /// You do not need to add it into the scene manually.
    /// </summary>
    internal class TaskManager : MonoBehaviour
    {
        [SerializeField]
        private bool debugMode = false;
        public bool DebugMode
        {
            get => debugMode;
            set => debugMode = value;
        }
        private const int ManagedTaskCapacity = 200;
        private const int RunningTaskCapacity = 100;
        private readonly HashSet<int> managedJobIDs = new(ManagedTaskCapacity);
        private readonly Dictionary<int, ITask> managedTasks = new(ManagedTaskCapacity);
        private List<ITask> _tasks = new(RunningTaskCapacity);
        //Start from id=1, should not be 0 since it roles as default/invalid job symbol
        private int jobID = 1;
        // buffer adding timers so we don't edit a collection during iteration
        private List<ITask> _tasksToAdd = new(RunningTaskCapacity);
        public static TaskManager Instance => instance != null ? instance : GetInstance();
        public static bool IsInitialized => instance != null;
        private static TaskManager instance;
#if UNITY_EDITOR
        public int CurrentJobID => jobID;
        public int ManagedJobCount => managedJobIDs.Count;
        public int UnManagedTaskCount
        {
            get
            {
                return managedTasks.Count(x => x.Value is AkiTask);
            }
        }
        public int UpdatingTaskCount => _tasks.Count;
        public event Action EditorUpdate;
#endif
        private static TaskManager GetInstance()
        {
#if UNITY_EDITOR_WIN
            if (!Application.isPlaying) return null;
#endif
            if (instance == null)
            {
                TaskManager managerInScene = FindObjectOfType<TaskManager>();
                if (managerInScene != null)
                {
                    instance = managerInScene;
                }
                else
                {
                    GameObject managerObject = new() { name = nameof(TaskManager) };
                    instance = managerObject.AddComponent<TaskManager>();
                }
            }
            return instance;
        }
        public void RegisterTask(ITask task)
        {
            _tasksToAdd.Add(task);
            if (debugMode)
            {
                Debug.Log("Task start, task hash : " + task.GetHashCode());
            }
        }

        public void CancelAllTasks()
        {
            foreach (ITask task in _tasks)
            {
                task.Cancel();
                if (debugMode)
                {
                    Debug.Log("Task cancel, task hash : " + task.GetHashCode());
                }
            }
            _tasks = new List<ITask>();
            _tasksToAdd = new List<ITask>();
        }

        public void PauseAllTasks()
        {
            foreach (ITask task in _tasks)
            {
                task.Pause();
                if (debugMode)
                {
                    Debug.Log("Task pause, task hash : " + task.GetHashCode());
                }
            }
        }

        public void ResumeAllTasks()
        {
            foreach (ITask task in _tasks)
            {
                task.Resume();
                if (debugMode)
                {
                    Debug.Log("Task resume, task hash : " + task.GetHashCode());
                }
            }
        }

        private void Update()
        {
            UpdateAllTasks();
            ReleaseAndRecycleTask();
#if UNITY_EDITOR
            EditorUpdate?.Invoke();
#endif
        }

        private void UpdateAllTasks()
        {
            if (_tasksToAdd.Count > 0)
            {
                _tasks.AddRange(_tasksToAdd);
                _tasksToAdd.Clear();
            }

            foreach (ITask timer in _tasks)
            {
                timer.Update();
            }
        }
        private void ReleaseAndRecycleTask()
        {
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                if (!_tasks[i].IsDone) continue;
                _tasks[i].Dispose();
                if (debugMode)
                {
                    Debug.Log("Task end, task hash : " + _tasks[i].GetHashCode());
                }
                _tasks.Remove(_tasks[i]);
            }
        }
        public JobHandle CreateJobHandle(ITask task)
        {
            int id = jobID++;
            if (debugMode)
            {
                Debug.Log("Job create, task hash : " + task.GetHashCode());
            }
            var handle = new JobHandle(id);
            managedJobIDs.Add(id);
            managedTasks.Add(id, task);
            return handle;
        }
        /// <summary>
        /// Whether job is valid, false means internal task is disposed and maybe recyled so you should not call it
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public bool IsValidJob(int jobID)
        {
            return managedJobIDs.Contains(jobID);
        }
        /// <summary>
        /// Find internal task if job is valid
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetTask(int jobID, out ITask task)
        {
            return managedTasks.TryGetValue(jobID, out task);
        }
        /// <summary>
        /// Cancel target job
        /// </summary>
        /// <param name="jobID"></param>
        public void CancelJob(int jobID)
        {
            var task = managedTasks[jobID];
            if (debugMode)
            {
                Debug.Log("Job cancel, task hash : " + task.GetHashCode());
            }
            task.Cancel();
            managedTasks.Remove(jobID);
            managedJobIDs.Remove(jobID);
        }
        /// <summary>
        /// Remove job from managed
        /// </summary>
        /// <param name="jobID"></param>
        public void ReleaseJob(int jobID)
        {
            managedTasks.Remove(jobID);
            managedJobIDs.Remove(jobID);
        }
    }
}