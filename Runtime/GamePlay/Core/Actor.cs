using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Chris
{
    /// <summary>
    /// Actor is an MonoBehaviour identifier to place GameObject in framework's GamePlay level.
    /// </summary>
    public abstract class Actor : MonoBehaviour
    {
        private GameWorld _world;
        
        private ActorController _controller;
        
        private ActorHandle _handle;
        
        private readonly HashSet<ActorComponent> _actorComponents = new();
        
        protected virtual void Awake()
        {
            RegisterActor(this);
        }
        
        protected virtual void OnDestroy()
        {
            UnregisterActor(this);
            _actorComponents.Clear();
        }
        
        /// <summary>
        /// Get actor's world
        /// </summary>
        /// <returns></returns>
        public GameWorld GetWorld() => _world;
        
        /// <summary>
        /// Get actor's id according to actor's world
        /// </summary>
        /// <returns></returns>
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
        
        public TController GetTController<TController>() where TController : ActorController
        {
            return _controller as TController;
        }
        
        public ActorController GetController()
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
        
        internal void BindController(ActorController controller)
        {
            if (this._controller != null)
            {
                Debug.LogError("[Actor] Actor already bound to a controller!");
                return;
            }
            this._controller = controller;
        }
        
        internal void UnbindController(ActorController controller)
        {
            if (this._controller == controller)
            {
                this._controller = null;
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
        
        public void GetActorComponents<TComponent>(List<TComponent> components) where TComponent : ActorComponent
        {
            foreach (var component in _actorComponents)
            {
                if (component is TComponent tComponent) components.Add(tComponent);
            }
        }
    }
}
