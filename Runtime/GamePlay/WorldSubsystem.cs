using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
namespace Kurisu.Framework
{
    internal class WorldSubsystemCollection : IDisposable
    {
        private readonly Dictionary<Type, WorldSubsystem> systems = new();
        private WorldSubsystem[] subsystems;
        public WorldSubsystemCollection(ActorWorld world)
        {
            var types = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(x => x.GetTypes())
                    .SelectMany(x => x)
                    .Where(x => x.IsSubclassOf(typeof(WorldSubsystem)) && !x.IsAbstract)
                    .ToList();
            systems = types.ToDictionary(x => x, x => Activator.CreateInstance(x) as WorldSubsystem);
            foreach (var type in types)
            {
                if (!systems[type].CanCreate(world)) systems.Remove(type);
                else systems[type].SetWorld(world);
            }
            subsystems = systems.Values.ToArray();
        }
        public T GetSubsystem<T>() where T : WorldSubsystem
        {
            return (T)systems[typeof(T)];
        }
        public WorldSubsystem GetSubsystem(Type type)
        {
            return systems[type];
        }
        public void Init()
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
                subsystems[i].Init();
                subsystems[i].InternalInit();
            }
        }
        public void Tick()
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
                subsystems[i].Tick();
            }
        }
        public void FixedTick()
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
                subsystems[i].FixedTick();
            }
        }
        public void Dispose()
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
                subsystems[i].Dispose();
                subsystems[i].InternalDispose();
            }
            subsystems = null;
        }
    }
    public abstract class WorldSubsystem : IDisposable
    {
        /// <summary>
        /// Buffered dirty flag when world actors changed
        /// </summary>
        protected bool IsActorsDirty { get; set; }
        private IDisposable actorsUpdateSubscription;
        private ActorWorld world;
        /// <summary>
        /// Whether system can create
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual bool CanCreate(ActorWorld world) => true;
        public virtual void Init()
        {

        }
        internal void InternalInit()
        {
            actorsUpdateSubscription = world.onActorsUpdate.Subscribe(OnActorsUpdate);
        }
        private void OnActorsUpdate(Unit _)
        {
            IsActorsDirty = true;
        }
        public virtual void Tick()
        {

        }
        public virtual void FixedTick()
        {

        }
        public virtual void Dispose()
        {

        }
        internal virtual void InternalDispose()
        {
            world = null;
            actorsUpdateSubscription.Dispose();
        }
        internal void SetWorld(ActorWorld world)
        {
            this.world = world;
        }
        /// <summary>
        /// Get attached world
        /// </summary>
        /// <returns></returns>
        public ActorWorld GetWorld() => world;
        /// <summary>
        /// Get all actors in world, readonly
        /// </summary>
        /// <returns></returns>
        protected ReadOnlySpan<Actor> GetActorsInWorld()
        {
            if (!world)
            {
                Debug.LogWarning("[World Subsystem] System not bound to an actor world.");
                return default;
            }
            return new(world.actorsInWorld);
        }
    }
}
