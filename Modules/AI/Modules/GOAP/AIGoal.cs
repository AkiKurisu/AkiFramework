using Kurisu.GOAP;
namespace Kurisu.Framework.AI.GOAP
{
    public abstract class AIGoal<TPawn> : GOAPGoal where TPawn : Actor, IAIPawn
    {
        protected AIController<TPawn> Controller { get; private set; }
        public void Setup(AIController<TPawn> controller)
        {
            Controller = controller;
            OnSetup();
        }
        protected virtual void OnSetup() { }
    }
}