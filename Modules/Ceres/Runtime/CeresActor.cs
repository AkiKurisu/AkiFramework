using System;
using Ceres.Graph;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using Chris.Events;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Chris.Ceres
{
    /// <summary>
    /// Base class for flow-executable actor
    /// </summary>
    public class CeresActor : Actor, IFlowGraphContainer
    {
        [NonSerialized]
        private FlowGraph _graph;
        
        [SerializeField]
        private FlowGraphData graphData;

        public Object Object => this;

        [ImplementableEvent]
        protected override void Awake()
        {
            _graph = GetFlowGraph();
            _graph.Compile();
            base.Awake();
            using var evt = ImplementableEvent.Create(nameof(Awake));
            ProcessEvent(evt); 
        }

        [ImplementableEvent]
        protected virtual void Start()
        {
            using var evt = ImplementableEvent.Create(nameof(Start));
            ProcessEvent(evt); 
        }

        [ImplementableEvent]
        protected virtual void OnEnable()
        {
            using var evt = ImplementableEvent.Create(nameof(OnEnable));
            ProcessEvent(evt); 
        }
        
                
        [ImplementableEvent]
        protected virtual void Update()
        {
            using var evt = ImplementableEvent.Create(nameof(Update));
            ProcessEvent(evt); 
        }
        
        [ImplementableEvent]
        protected virtual void FixedUpdate()
        {
            using var evt = ImplementableEvent.Create(nameof(FixedUpdate));
            ProcessEvent(evt); 
        }
        
        [ImplementableEvent]
        protected virtual void LateUpdate()
        {
            using var evt = ImplementableEvent.Create(nameof(LateUpdate));
            ProcessEvent(evt); 
        }

        [ImplementableEvent]
        protected virtual void OnDisable()
        {
            using var evt = ImplementableEvent.Create(nameof(OnDisable));
            ProcessEvent(evt); 
        }

        [ImplementableEvent]
        protected override void OnDestroy()
        {
            using var evt = ImplementableEvent.Create(nameof(OnDestroy));
            ProcessEvent(evt); 
            base.OnDestroy();
        }
        
        protected void ProcessEvent(string eventName, EventBase eventBase)
        {
            /* Execute event in quiet way */
            _graph.TryExecuteEvent(this, eventName, eventBase);
        }
        
        protected void ProcessEvent(ImplementableEvent eventBase)
        {
            /* Execute event in quiet way */
            _graph.TryExecuteEvent(this, eventBase.FunctionName, eventBase);
        }
        
        public CeresGraph GetGraph()
        {
            return GetFlowGraph();
        }

        public FlowGraph GetFlowGraph()
        {
            return new FlowGraph(graphData.CloneT<FlowGraphData>());
        }

        public void SetGraphData(CeresGraphData graph)
        {
            graphData = (FlowGraphData)graph;
        }
    }
}
