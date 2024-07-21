using Kurisu.Framework.Tasks;
namespace Kurisu.Framework
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Run the task
        /// </summary>
        public static void Run(this TaskBase taskBase)
        {
            if (taskBase.GetStatus() == TaskStatus.Running)
            {
                return;
            }
            if (taskBase.HasPrerequistites()) return;
            taskBase.Start();
            TaskRunner.RegisterTask(taskBase);
        }
    }
}
