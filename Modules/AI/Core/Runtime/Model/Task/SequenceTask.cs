using System;
using System.Collections;
using System.Collections.Generic;
namespace Kurisu.Framework.AI
{
    /// <summary>
    /// Sequence task running outside of agent
    /// </summary>
    public class SequenceTask : ITask, IEnumerable<ITask>
    {
        public event Action OnCompleted;
        private readonly Queue<ITask> tasks = new();
        public TaskStatus Status { get; private set; } = TaskStatus.Pending;
        public SequenceTask()
        {
            OnCompleted = null;
        }
        public SequenceTask(Action callBack)
        {
            OnCompleted = callBack;
        }
        public SequenceTask(ITask firstTask, Action callBack)
        {
            OnCompleted = callBack;
            tasks.Enqueue(firstTask);
        }
        public SequenceTask(IReadOnlyList<ITask> sequence, Action callBack)
        {
            OnCompleted = callBack;
            foreach (var task in sequence)
                tasks.Enqueue(task);
        }
        /// <summary>
        /// Append a task to the end of sequence
        /// </summary>
        /// <param name="task"></param>
        public SequenceTask Append(ITask task)
        {
            tasks.Enqueue(task);
            return this;
        }
        public SequenceTask AppendRange(IEnumerable<ITask> enumerable)
        {
            foreach (var task in enumerable)
                tasks.Enqueue(task);
            return this;
        }
        public void Tick()
        {
            if (tasks.TryPeek(out ITask first))
            {
                first.Tick();
                if (first.Status == TaskStatus.Disabled)
                {
                    tasks.Dequeue();
                    if (tasks.Count == 0)
                    {
                        Status = TaskStatus.Disabled;
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
                Status = TaskStatus.Disabled;
                OnCompleted?.Invoke();
                OnCompleted = null;
            }
        }
        public void Abort()
        {
            Status = TaskStatus.Disabled;
            OnCompleted = null;
        }
        public IEnumerator<ITask> GetEnumerator()
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
            return Append(new CallBackTask(callBack));
        }
        /// <summary>
        /// Call to run task sequence in a dedicate runner
        /// </summary>
        /// <returns></returns>
        public SequenceTask Run()
        {
            if (Status == TaskStatus.Pending)
            {
                Status = TaskStatus.Enabled;
                TaskRunner.RegisterTask(this);
            }
            return this;
        }
    }
}
