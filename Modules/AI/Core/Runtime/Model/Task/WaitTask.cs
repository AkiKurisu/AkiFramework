using UnityEngine;
namespace Kurisu.Framework.AI
{
    public class WaitTask : ITask
    {
        public TaskStatus Status { get; private set; } = TaskStatus.Enabled;
        private readonly float waitTime;
        private float timer = 0;
        public WaitTask(float waitTime)
        {
            this.waitTime = waitTime;
        }
        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
                Status = TaskStatus.Disabled;
        }
    }
}
