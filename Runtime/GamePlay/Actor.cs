using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework
{
    /// <summary>
    /// Actor is an MonoBehaviour identifier to place GameObject in framework's GamePlay level.
    /// </summary>
    public abstract class Actor : MonoBehaviour
    {
        private ActorWorld world;
        private ActorController controller;
        private ActorHandle handle;
        private readonly HashSet<ActorComponent> actorComponents = new();
        protected virtual void Awake()
        {
            RegisterActor(this);
        }
        protected virtual void OnDestroy()
        {
            UnregisterActor(this);
            actorComponents.Clear();
        }
        /// <summary>
        /// Get actor's world
        /// </summary>
        /// <returns></returns>
        public ActorWorld GetWorld() => world;
        /// <summary>
        /// Get actor's id according to actor's world
        /// </summary>
        /// <returns></returns>
        public ActorHandle GetActorHandle() => handle;
        /// <summary>
        /// Register an actor to world
        /// </summary>
        /// <param name="actor"></param>
        protected static void RegisterActor(Actor actor)
        {
            actor.world = ActorWorld.Current;
            actor.world.RegisterActor(actor, ref actor.handle);
        }
        /// <summary>
        /// Unregister an actor from world
        /// </summary>
        /// <param name="actor"></param>
        protected static void UnregisterActor(Actor actor)
        {
            if (actor.world == null || actor.world != ActorWorld.Current) return;
            actor.world.UnregisterActor(actor);
            actor.world = null;
            actor.handle = default;
        }
        public TController GetTController<TController>() where TController : ActorController
        {
            return controller as TController;
        }
        public ActorController GetController()
        {
            return controller;
        }
        /// <summary>
        /// Register an actor component to actor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="component"></param>
        internal static void RegisterActorComponent(Actor actor, ActorComponent component)
        {
#if UNITY_EDITOR
            Assert.IsNotNull(actor);
            Assert.IsNotNull(component);
#endif
            actor.actorComponents.Add(component);
        }
        /// <summary>
        /// Unregister an actor component from actor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="component"></param>
        internal static void UnregisterActor(Actor actor, ActorComponent component)
        {
#if UNITY_EDITOR
            Assert.IsNotNull(actor);
            Assert.IsNotNull(component);
#endif
            actor.actorComponents.Remove(component);
        }
        internal void BindController(ActorController controller)
        {
            if (this.controller != null)
            {
                Debug.LogError("[Actor] Actor already bound to a controller!");
                return;
            }
            this.controller = controller;
        }
        internal void UnbindController(ActorController controller)
        {
            if (this.controller == controller)
            {
                this.controller = null;
            }
        }
        public TComponent GetActorComponent<TComponent>() where TComponent : ActorComponent
        {
            foreach (var component in actorComponents)
            {
                if (component is TComponent tComponent) return tComponent;
            }
            return null;
        }
    }
}
