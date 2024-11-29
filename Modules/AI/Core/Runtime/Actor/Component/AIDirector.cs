using System;
using System.Collections.Generic;
using Chris.Tasks;
using UnityEngine;
using UnityEngine.Events;
namespace Chris.AI
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
            OnPlayDirector();
            OnPlay?.Invoke();
        }
        protected virtual void OnPlayDirector() { }
        protected void RunDirectorTasks()
        {
            sequenceTask?.Dispose();
            sequenceTask = SequenceTask.GetPooled(tasks, OnPlayEnd);
            sequenceTask.Acquire();
            sequenceTask.Run();
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
            callBack = null;
            controller = null;
            OnStop?.Invoke();
        }
    }
}
