using Kurisu.GOAP;
namespace Kurisu.Framework.AI.GOAP
{
    public abstract class AIAction<TPawn> : GOAPAction where TPawn : Actor, IAIPawn
    {
        protected AIController<TPawn> Pawn { get; private set; }
        public void Setup(AIController<TPawn> pawn)
        {
            Pawn = pawn;
            OnSetup();
        }
        protected virtual void OnSetup() { }
    }
}