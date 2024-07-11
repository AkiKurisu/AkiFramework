using UnityEngine;
namespace Kurisu.Framework.Tasks
{
    public class WaitTask : PooledTaskBase<WaitTask>
    {
        private float waitTime;
        private float timer = 0;
        public static WaitTask GetPooled(float waitTime)
        {
            var task = GetPooled();
            task.waitTime = waitTime;
            return task;
        }
        protected override void Init()
        {
            base.Init();
            timer = 0;
            mStatus = TaskStatus.Enabled;
        }
        protected override void Reset()
        {
            base.Reset();
            timer = 0;
        }
        public override void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
                mStatus = TaskStatus.Disabled;
        }
    }
}
