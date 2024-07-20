using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
namespace Kurisu.Framework
{
    internal class WorldSubsystemCollection : IDisposable
    {
        private readonly Dictionary<Type, SubsystemBase> systems = new();
        private SubsystemBase[] subsystems;
        public WorldSubsystemCollection(ActorWorld world)
        {
            var types = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(x => x.GetTypes())
                    .SelectMany(x => x)
                    .Where(x => x.IsSubclassOf(typeof(WorldSubsystem)) && !x.IsAbstract)
                    .ToList();
            systems = types.ToDictionary(x => x, x => Activator.CreateInstance(x) as SubsystemBase);
            foreach (var type in types)
            {
                if (!systems[type].CanCreate(world)) systems.Remove(type);
                else systems[type].SetWorld(world);
            }
            subsystems = systems.Values.ToArray();
        }
        internal void RegisterSubsystem<T>(T subsystem) where T : SubsystemBase
        {
            systems.Add(typeof(T), subsystem);
            subsystem.InternalInit();
        }
        internal void Rebuild()
        {
            subsystems = systems.Values.ToArray();
            Init();
        }
        public T GetSubsystem<T>() where T : SubsystemBase
        {
            if (systems.TryGetValue(typeof(T), out var subsystem))
                return (T)subsystem;
            return null;
        }
        public SubsystemBase GetSubsystem(Type type)
        {
            if (systems.TryGetValue(type, out var subsystem))
                return subsystem;
            return null;
        }
        public void Init()
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
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
                subsystems[i].InternalRelease();
            }
            subsystems = null;
        }
    }
    public abstract class SubsystemBase
    {

        /// <summary>
        /// Buffered dirty flag when world actors changed
        /// </summary>
        protected bool IsActorsDirty { get; set; }
        private bool isInitialized;
        private bool isDestroyed;
        private IDisposable actorsUpdateSubscription;
        private ActorWorld world;
        /// <summary>
        /// Whether system can create
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual bool CanCreate(ActorWorld world) => true;
        /// <summary>
        /// Subsystem initialize phase, should bind callbacks and collect references in this phase
        /// </summary>
        protected virtual void Initialize()
        {

        }
        internal virtual void InternalInit()
        {
            if (isInitialized) return;
            isInitialized = true;
            actorsUpdateSubscription = world.onActorsUpdate.Subscribe(OnActorsUpdate);
            Initialize();
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
        /// <summary>
        /// Subsystem release phase, should unbind callbacks and release references in this phase
        /// </summary>
        protected virtual void Release()
        {

        }
        internal virtual void InternalRelease()
        {
            if (isDestroyed) return;
            world = null;
            isDestroyed = true;
            actorsUpdateSubscription.Dispose();
            Release();
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
    /// <summary>
    /// Subsystem always bound to an actor world.
    /// </summary>
    public abstract class WorldSubsystem : SubsystemBase
    {
    }
    /// <summary>
    /// Subsystem dynamically created during game
    /// </summary>
    public abstract class DynamicSubsystem : SubsystemBase
    {
        /// <summary>
        /// Dynamically create system if not registered.
        /// </summary>
        /// <param name="world"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(ActorWorld world) where T : DynamicSubsystem, new()
        {
            var system = world.GetSubsystem<T>();
            if (system == null)
            {
                system = new();
                if (!system.CanCreate(world)) return null;
                system.SetWorld(world);
                world.RegisterSubsystem(system);
            }
            return system;
        }
    }
}
