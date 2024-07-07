using System;
namespace Kurisu.Framework.AI
{
    public class CallBackTask : ITask
    {
        public TaskStatus Status { get; private set; }
        private readonly Action callBack;
        public CallBackTask(Action callBack)
        {
            this.callBack = callBack;
            Status = TaskStatus.Enabled;
        }
        public void Tick()
        {
            callBack?.Invoke();
            Status = TaskStatus.Disabled;
        }
    }
}
