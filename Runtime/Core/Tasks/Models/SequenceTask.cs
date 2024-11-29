using System;
using System.Collections;
using System.Collections.Generic;
namespace Chris.Tasks
{
    /// <summary>
    /// Sequence to composite tasks
    /// </summary>
    public class SequenceTask : PooledTaskBase<SequenceTask>, IEnumerable<TaskBase>
    {
        public event Action OnCompleted;
        private readonly Queue<TaskBase> tasks = new();
        private TaskBase runningTask;
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
        protected override void Reset()
        {
            base.Reset();
            runningTask = null;
            mStatus = TaskStatus.Stopped;
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
            if (runningTask == null)
            {
                tasks.TryPeek(out runningTask);
                runningTask.Start();
            }

            if (runningTask != null)
            {
                runningTask.Tick();
                var status = runningTask.GetStatus();
                if (status is TaskStatus.Completed or TaskStatus.Stopped)
                {
                    if (status == TaskStatus.Completed)
                    {
                        runningTask.PostComplete();
                    }
                    tasks.Dequeue().Dispose();
                    runningTask = null;

                    if (tasks.Count == 0)
                    {
                        mStatus = TaskStatus.Completed;
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
                mStatus = TaskStatus.Completed;
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
    }
}
