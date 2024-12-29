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
            public AIController Controller;
            
            public IReadOnlyList<TaskBase> Tasks;
            
            public Action CallBack;
        }
        
        [SerializeField]
        private UnityEvent OnPlay;
        
        [SerializeField]
        private UnityEvent OnStop
            ;
        protected AIController Controller;
        
        private SequenceTask _sequenceTask;
        
        private IReadOnlyList<TaskBase> _tasks;
        
        private Action _callBack;
        
        public SequenceTask GetDirectorTask()
        {
            return _sequenceTask;
        }
        
        public AIController GetController()
        {
            return Controller;
        }
        public void Play(RequestContext proxyContext)
        {
            _callBack = proxyContext.CallBack;
            Controller = proxyContext.Controller;
            _tasks = proxyContext.Tasks;
            OnPlayDirector();
            OnPlay?.Invoke();
        }
        
        protected virtual void OnPlayDirector() { }
        
        protected void RunDirectorTasks()
        {
            _sequenceTask?.Dispose();
            _sequenceTask = SequenceTask.GetPooled(_tasks, OnPlayEnd);
            _sequenceTask.Acquire();
            _sequenceTask.Run();
        }
        
        private void OnPlayEnd()
        {
            _callBack?.Invoke();
            Stop();
        }
        
        public virtual void Stop()
        {
            _sequenceTask?.Dispose();
            _sequenceTask = null;
            _callBack = null;
            Controller = null;
            OnStop?.Invoke();
        }
    }
}
