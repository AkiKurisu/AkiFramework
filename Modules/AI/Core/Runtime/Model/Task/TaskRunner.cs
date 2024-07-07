using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.AI
{
    internal class TaskRunner : MonoBehaviour
    {
        private readonly List<ITask> _tasks = new();
        private readonly List<ITask> _tasksToAdd = new();
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
        public static void RegisterTask(ITask task)
        {
            var instance = GetInstance();
            if (instance)
            {
                if (instance._tasks.Contains(task)) return;
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

            foreach (ITask task in _tasks)
            {
                if (task.Status != TaskStatus.Disabled)
                    task.Tick();
            }
        }
        private void ReleaseTasks()
        {
            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                if (_tasks[i].Status != TaskStatus.Disabled) continue;
                _tasks.Remove(_tasks[i]);
            }
        }
    }
}
