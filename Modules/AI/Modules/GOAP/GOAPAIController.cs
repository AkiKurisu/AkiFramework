using UnityEngine;
using System.Linq;
using Kurisu.GOAP;
namespace Kurisu.Framework.AI.GOAP
{
    [RequireComponent(typeof(WorldState))]
    [RequireComponent(typeof(GOAPPlanner))]
    public abstract class GOAPAIController<T> : AIController<T> where T : IAIContext
    {
        [SerializeField]
        private GOAPSet dataSet;
        protected GOAPSet DataSet => dataSet;
        private GOAPPlanner planner;
        public GOAPPlanner Planner => planner;
        public WorldState WorldState { get; private set; }
        protected override void Awake()
        {
            WorldState = GetComponent<WorldState>();
            planner = GetComponent<GOAPPlanner>();
            base.Awake();
        }
        protected override void Start()
        {
            SetupGOAP();
            base.Start();
        }
        protected override void OnEnable()
        {
            planner.enabled = true;
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            planner.enabled = false;
            base.OnDisable();
        }
        private void SetupGOAP()
        {
            var goals = DataSet.GetGoals();
            foreach (var goal in goals.OfType<AIGoal<T>>())
            {
                goal.Setup(this);
            }
            var actions = DataSet.GetActions();
            foreach (var action in actions.OfType<AIAction<T>>())
            {
                action.Setup(this);
            }
            Planner.SetGoalsAndActions(goals, actions);
        }
    }
}
