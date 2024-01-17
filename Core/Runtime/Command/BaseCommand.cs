namespace Kurisu.Framework
{
    public abstract class BaseCommand : ICommand, IPooled
    {
        public void Execute()
        {
            OnExecute();
            Destroy();
        }
        protected virtual void OnExecute() { }
        private void Destroy()
        {
            this.ObjectPushPool();
        }
    }
}