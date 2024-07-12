using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    /// <summary>
    /// Sequence to composite tasks
    /// </summary>
    public class SequenceTask : PooledTaskBase<SequenceTask>, IEnumerable<TaskBase>
    {
        public event Action OnCompleted;
        private readonly Queue<TaskBase> tasks = new();
        public static SequenceTask GetPooled(Action callBack)
        {
            var task = GetPooled();
            task.OnCompleted = callBack;
            return task;
        }
        public static SequenceTask GetPooled(TaskBase firstTask, Action callBack)
        {
            var task = GetPooled();
            task.OnCompleted = callBack;
            task.Append(firstTask);
            return task;
        }
        public static SequenceTask GetPooled(IReadOnlyList<TaskBase> sequence, Action callBack)
        {
            var task = GetPooled();
            task.OnCompleted = callBack;
            foreach (var tb in sequence)
                task.Append(tb);
            return task;
        }
        protected override void Init()
        {
            base.Init();
            mStatus = TaskStatus.Disabled;
        }
        protected override void Reset()
        {
            base.Reset();
            mStatus = TaskStatus.Disabled;
            OnCompleted = null;
            tasks.Clear();
        }
        /// <summary>
        /// Append a task to the end of sequence
        /// </summary>
        /// <param name="task"></param>
        public SequenceTask Append(TaskBase task)
        {
            tasks.Enqueue(task);
            task.Acquire();
            return this;
        }
        public SequenceTask AppendRange(IEnumerable<TaskBase> enumerable)
        {
            foreach (var task in enumerable)
                Append(task);
            return this;
        }
        public override void Tick()
        {
            if (tasks.TryPeek(out TaskBase first))
            {
                first.Tick();
                if (first.GetStatus() == TaskStatus.Disabled)
                {
                    tasks.Dequeue().Dispose();
                    if (tasks.Count == 0)
                    {
                        mStatus = TaskStatus.Disabled;
                        OnCompleted?.Invoke();
                        OnCompleted = null;
                    }
                    else
                    {
                        Tick();
                    }
                }
            }
            else
            {
                mStatus = TaskStatus.Disabled;
                OnCompleted?.Invoke();
                OnCompleted = null;
            }
        }
        public IEnumerator<TaskBase> GetEnumerator()
        {
            return tasks.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return tasks.GetEnumerator();
        }
        /// <summary>
        /// Append a call back after current last action in the sequence
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public SequenceTask AppendCallBack(Action callBack)
        {
            return Append(CallBackTask.GetPooled(callBack));
        }
        /// <summary>
        /// Fire this task to start and run
        /// </summary>
        public void Fire()
        {
            if (mStatus == TaskStatus.Enabled)
            {
                Debug.LogWarning("Task is already start");
                return;
            }
            Start();
            TaskRunner.RegisterTask(this);
        }
    }
}
