using System;
using System.Collections.Generic;
using Kurisu.Framework.Tasks;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.Framework.AI
{
    public abstract class AIDirector : MonoBehaviour
    {
        public struct RequestContext
        {
            public AIController controller;
            public IReadOnlyList<TaskBase> tasks;
            public Action callBack;
        }
        [SerializeField]
        private UnityEvent OnPlay;
        [SerializeField]
        private UnityEvent OnStop;
        protected AIController controller;
        private SequenceTask sequenceTask;
        private IReadOnlyList<TaskBase> tasks;
        private Action callBack;
        public SequenceTask GetDirectorTask()
        {
            return sequenceTask;
        }
        public AIController GetController()
        {
            return controller;
        }
        public void Play(RequestContext proxyContext)
        {
            callBack = proxyContext.callBack;
            controller = proxyContext.controller;
            tasks = proxyContext.tasks;
#if UNITY_EDITOR
            Debug.Log($"Director play: {GetType().Name}");
#endif
            OnPlayDirector();
            OnPlay?.Invoke();
        }
        protected virtual void OnPlayDirector() { }
        protected void RunDirectorTasks()
        {
#if UNITY_EDITOR
            Debug.Log($"Create task sequence: {GetType().Name}");
#endif
            sequenceTask?.Dispose();
            sequenceTask = SequenceTask.GetPooled(tasks, OnPlayEnd);
            sequenceTask.Acquire();
            sequenceTask.Fire();
        }
        private void OnPlayEnd()
        {
            callBack?.Invoke();
            Stop();
        }
        public virtual void Stop()
        {
            sequenceTask?.Dispose();
            sequenceTask = null;
#if UNITY_EDITOR
            Debug.Log($"Director stop: {GetType().Name}");
#endif
            callBack = null;
            controller = null;
            OnStop?.Invoke();
        }
    }
}
