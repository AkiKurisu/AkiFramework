using Kurisu.GOAP;
namespace Kurisu.Framework.AI.GOAP
{
    public abstract class AIAction<TContext> : GOAPAction where TContext : IAIContext
    {
        protected AIController<TContext> Host { get; private set; }
        public void Setup(AIController<TContext> host)
        {
            Host = host;
            OnSetup();
        }
        protected virtual void OnSetup() { }
    }
}