using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    internal class TaskRunner : MonoBehaviour
    {
        private readonly List<TaskBase> _tasks = new();
        private readonly List<TaskBase> _tasksToAdd = new();
        private static TaskRunner instance;
        private static TaskRunner GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif
            if (instance == null)
            {
                GameObject managerObject = new() { name = nameof(TaskRunner) };
                instance = managerObject.AddComponent<TaskRunner>();
            }
            return instance;
        }
        public static void RegisterTask(TaskBase task)
        {
            var instance = GetInstance();
            if (instance)
            {
                if (instance._tasks.Contains(task)) return;
                task.Acquire();
                instance._tasksToAdd.Add(task);
            }
        }

        private void Update()
        {
            UpdateAllTasks();
            ReleaseTasks();
        }

        private void UpdateAllTasks()
        {
            if (_tasksToAdd.Count > 0)
            {
                _tasks.AddRange(_tasksToAdd);
                _tasksToAdd.Clear();
            }

            foreach (TaskBase task in _tasks)
            {
                if (task.GetStatus() == TaskStatus.Enabled)
                    task.Tick();
            }
        }
        private void ReleaseTasks()
        {
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                if (_tasks[i].GetStatus() != TaskStatus.Disabled) continue;
                _tasks[i].Dispose();
                _tasks.RemoveAt(i);
            }
        }
    }
}
