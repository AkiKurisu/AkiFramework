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
        private readonly IDisposable actorsUpdateSubscription;
        public WorldSubsystemCollection(GameWorld world)
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
            actorsUpdateSubscription = world.onActorsUpdate.Subscribe(OnActorsUpdate);
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
            actorsUpdateSubscription.Dispose();
        }
        private void OnActorsUpdate(Unit _)
        {
            for (int i = 0; i < subsystems.Length; ++i)
            {
                subsystems[i].IsActorsDirty = true;
            }
        }
    }
    public abstract class SubsystemBase
    {

        /// <summary>
        /// Buffered dirty flag when world actors changed
        /// </summary>
        protected internal bool IsActorsDirty { get; set; }
        /// <summary>
        /// Is system initialized
        /// </summary>
        /// <value></value>
        protected bool IsInitialized { get; private set; }
        /// <summary>
        /// Is system destroyed
        /// </summary>
        /// <value></value>
        protected bool IsDestroyed { get; private set; }
        private GameWorld world;
        /// <summary>
        /// Whether system can create
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual bool CanCreate(GameWorld world) => true;
        /// <summary>
        /// Subsystem initialize phase, should bind callbacks and collect references in this phase
        /// </summary>
        protected virtual void Initialize()
        {

        }
        internal virtual void InternalInit()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            Initialize();
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
            if (IsDestroyed) return;
            world = null;
            IsDestroyed = true;
            Release();
        }
        internal void SetWorld(GameWorld world)
        {
            this.world = world;
        }
        /// <summary>
        /// Get attached world
        /// </summary>
        /// <returns></returns>
        public GameWorld GetWorld() => world;
        /// <summary>
        /// Get all actors in world, readonly
        /// </summary>
        /// <returns></returns>
        protected void GetActorsInWorld(List<Actor> actors)
        {
            if (!world)
            {
                Debug.LogWarning("[World Subsystem] System not bound to an actor world.");
                return;
            }
            foreach (var actor in world.actorsInWorld)
            {
                actors.Add(actor);
            }
        }
        protected int GetActorsNum()
        {
            if (!world)
            {
                Debug.LogWarning("[World Subsystem] System not bound to an actor world.");
                return default;
            }
            return world.actorsInWorld.Count;
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
        public static T Get<T>(GameWorld world) where T : DynamicSubsystem, new()
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
        /// <summary>
        /// Dynamically create system if not registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : DynamicSubsystem, new()
        {
            return Get<T>(GameWorld.Get());
        }
    }
}
