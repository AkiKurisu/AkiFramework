using System.Collections.Generic;
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
        internal readonly HashSet<int> managedTaskIds = new(ManagedTaskCapacity);
        internal readonly Dictionary<int, ITask> managedTasks = new(ManagedTaskCapacity);
        internal List<ITask> _tasks = new(RunningTaskCapacity);
        //Start from id=1, should not be 0 since it roles as default/invalid job symbol
        internal int taskId = 1;
        // buffer adding timers so we don't edit a collection during iteration
        private List<ITask> _tasksToAdd = new(RunningTaskCapacity);
        public static TaskManager Instance => instance != null ? instance : GetInstance();
        public static bool IsInitialized => instance != null;
        private static TaskManager instance;
        private static TaskManager GetInstance()
        {
#if UNITY_EDITOR
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
                Debug.Log("Task started, task hash : " + task.GetHashCode());
            }
        }

        public void CancelAllTasks()
        {
            foreach (ITask task in _tasks)
            {
                task.Cancel();
                if (debugMode)
                {
                    Debug.Log("Tasked cancel, task hash : " + task.GetHashCode());
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
                    Debug.Log("Tasked pause, task hash : " + task.GetHashCode());
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
        }

        private void UpdateAllTasks()
        {
            if (_tasksToAdd.Count > 0)
            {
                _tasks.AddRange(_tasksToAdd);
                _tasksToAdd.Clear();
            }

            foreach (ITask task in _tasks)
            {
                task.Update();
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
                    Debug.Log("Task ended, task hash : " + _tasks[i].GetHashCode());
                }
                _tasks.Remove(_tasks[i]);
            }
        }
        public TaskHandle CreateTaskHandle(ITask task)
        {
            int id = taskId++;
            if (debugMode)
            {
                Debug.Log("Task created, task hash : " + task.GetHashCode());
            }
            var handle = new TaskHandle(id);
            managedTaskIds.Add(id);
            managedTasks.Add(id, task);
            return handle;
        }
        /// <summary>
        /// Whether task is valid, false means internal task is disposed and maybe recycled so you should not call it
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool IsValidTask(int taskId)
        {
            return managedTaskIds.Contains(taskId);
        }
        /// <summary>
        /// Find internal task if taskId is valid
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool TryGetTask(int taskId, out ITask task)
        {
            return managedTasks.TryGetValue(taskId, out task);
        }
        /// <summary>
        /// Cancel target task
        /// </summary>
        /// <param name="taskId"></param>
        public void CancelTask(int taskId)
        {
            var task = managedTasks[taskId];
            if (debugMode)
            {
                Debug.Log("Task canceled, task hash : " + task.GetHashCode());
            }
            task.Cancel();
            managedTasks.Remove(taskId);
            managedTaskIds.Remove(taskId);
        }
        /// <summary>
        /// Remove task from managed
        /// </summary>
        /// <param name="taskId"></param>
        public void ReleaseTask(int taskId)
        {
            managedTasks.Remove(taskId);
            managedTaskIds.Remove(taskId);
        }
    }
}