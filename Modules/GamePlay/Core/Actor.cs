using System.Collections.Generic;
using Ceres.Annotations;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using Chris.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
namespace Chris.Gameplay
{
    /// <summary>
    /// Actor is a MonoBehaviour to place GameObject in GamePlay level.
    /// </summary>
    [GenerateFlow]
    public partial class Actor : MonoBehaviour, IFlowGraphContainer
    {
        private GameWorld _world;
        
        private PlayerController _controller;
        
        private ActorHandle _handle;
        
        private readonly HashSet<ActorComponent> _actorComponents = new();
        
        [ImplementableEvent]
        protected virtual void Awake()
        {
            _graph = GetFlowGraph();
            _graph.Compile();
            RegisterActor(this);
            ProcessEvent();
        }
        
        [ImplementableEvent]
        protected virtual void OnEnable()
        {
            ProcessEvent();
        }

        [ImplementableEvent]
        protected virtual void Start()
        {
            ProcessEvent();
        }
        
        [ImplementableEvent]
        protected virtual void OnDisable()
        {
            ProcessEvent();
        }
        
        [ImplementableEvent]
        protected virtual void Update()
        {
            ProcessEvent();
        }
        
        [ImplementableEvent]
        protected virtual void FixedUpdate()
        {
            ProcessEvent();
        }
        
        [ImplementableEvent]
        protected virtual void LateUpdate()
        {
            ProcessEvent();
        }

        [ImplementableEvent]
        protected virtual void OnDestroy()
        {
            ProcessEvent();
            UnregisterActor(this);
            _actorComponents.Clear();
        }
        
        /// <summary>
        /// Get actor's world
        /// </summary>
        /// <returns></returns>
        [ExecutableFunction]
        public GameWorld GetWorld() => _world;
        
        /// <summary>
        /// Get actor's id according to actor's world
        /// </summary>
        /// <returns></returns>
        [ExecutableFunction]
        public ActorHandle GetActorHandle() => _handle;
        
        /// <summary>
        /// Register an actor to world
        /// </summary>
        /// <param name="actor"></param>
        protected static void RegisterActor(Actor actor)
        {
            actor._world = GameWorld.Get();
            actor._world.RegisterActor(actor, ref actor._handle);
        }
        
        /// <summary>
        /// Unregister an actor from world
        /// </summary>
        /// <param name="actor"></param>
        protected static void UnregisterActor(Actor actor)
        {
            if (actor._world == null || actor._world != GameWorld.Get()) return;
            actor._world.UnregisterActor(actor);
            actor._world = null;
            actor._handle = default;
        }
        
        public TController GetTController<TController>() where TController : PlayerController
        {
            return _controller as TController;
        }
        
        [ExecutableFunction]
        public PlayerController GetController()
        {
            return _controller;
        }
        
        /// <summary>
        /// Register an actor component to actor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="component"></param>
        internal static void RegisterActorComponent(Actor actor, ActorComponent component)
        {
            Assert.IsNotNull(actor);
            Assert.IsNotNull(component);
            actor._actorComponents.Add(component);
        }
        
        /// <summary>
        /// Unregister an actor component from actor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="component"></param>
        internal static void UnregisterActor(Actor actor, ActorComponent component)
        {
            Assert.IsNotNull(actor);
            Assert.IsNotNull(component);
            actor._actorComponents.Remove(component);
        }
        
        internal void BindController(PlayerController controller)
        {
            if (_controller != null)
            {
                Debug.LogError("[Actor] Actor already bound to a controller!");
                return;
            }
            _controller = controller;
        }
        
        internal void UnbindController(PlayerController controller)
        {
            if (_controller == controller)
            {
                _controller = null;
            }
        }
        
        public TComponent GetActorComponent<TComponent>() where TComponent : ActorComponent
        {
            foreach (var component in _actorComponents)
            {
                if (component is TComponent tComponent) return tComponent;
            }
            return null;
        }
        
        [ExecutableFunction]
        public ActorComponent GetActorComponent(
            [CeresMetadata(ExecutableFunction.RESOLVE_RETURN)] SerializedType<ActorComponent> type)
        {
            foreach (var component in _actorComponents)
            {
                if (component.GetType()  == type) return component;
            }
            return null;
        }
        
        public void GetActorComponents<TComponent>(List<TComponent> components) where TComponent : ActorComponent
        {
            foreach (var component in _actorComponents)
            {
                if (component is TComponent tComponent) components.Add(tComponent);
            }
        }
    }
}
