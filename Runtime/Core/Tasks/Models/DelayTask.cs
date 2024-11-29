using Chris.Schedulers;

namespace Chris.Tasks
{
    /// <summary>
    /// Represent a delay task use scheduler so that can be tracked
    /// </summary>
    public class DelayTask : PooledTaskBase<DelayTask>
    {
        private SchedulerHandle handle;
        [StackTraceFrame]
        public unsafe static DelayTask GetPooled(float delay)
        {
            var task = GetPooled();
            task.handle = Scheduler.DelayUnsafe(delay, new SchedulerUnsafeBinding(task, &StopDelayTask));
            return task;
        }
        protected override void Init()
        {
            base.Init();
            handle = default;
        }
        protected override void Reset()
        {
            base.Reset();
            handle.Dispose();
            handle = default;
        }
        private static void StopDelayTask(object instance)
        {
            ((DelayTask)instance).mStatus = TaskStatus.Completed;
        }
    }
}
