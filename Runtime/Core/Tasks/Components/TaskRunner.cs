using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.React;
using UnityEngine;
using R3;
namespace Kurisu.Framework.Tasks
{
    internal class TaskRunner : MonoBehaviour
    {
        internal readonly List<TaskBase> _tasks = new();
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
                if (instance._tasks.Contains(task))
                {
                    Debug.LogWarning($"[TaskRunner] Registered a Task {task.InternalGetTaskName()} that has already been registered!");
                    return;
                }
                task.Acquire();
                instance._tasksToAdd.Add(task);
            }
        }
        private void Awake()
        {
            EventSystem.EventHandler.AsObservable<TaskCompleteEvent>()
                                    .SubscribeSafe(OnTaskComplete)
                                    .RegisterTo(destroyCancellationToken);
        }
        private void Update()
        {
            UpdateAllTasks();
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
                if (task.GetStatus() == TaskStatus.Running)
                {
                    task.Tick();
                }
            }

            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                var status = _tasks[i].GetStatus();
                if (status is TaskStatus.Completed or TaskStatus.Stopped)
                {
                    if (status == TaskStatus.Completed)
                        _tasks[i].PostComplete();
                    _tasks[i].Dispose();
                    _tasks.RemoveAt(i);
                }
            }
        }
        private void OnTaskComplete(TaskCompleteEvent evt)
        {
            foreach (var task in evt.Listeners)
            {
                if (task.ReleasePrerequistite(evt) && !task.HasPrerequistites())
                {
                    task.Run();
                }
            }
        }
    }
}
