using System;
using Kurisu.Framework.Collections;
using R3;
using UnityEngine;
namespace Kurisu.Framework
{
    /// <summary>
    /// World for GamePlay actor level.
    /// </summary>
    public class ActorWorld : MonoBehaviour
    {
        public static int MaxActorNum { get; set; } = DefaultMaxActorNum;
        public const int DefaultMaxActorNum = 5000;
        /// <summary>
        /// Use <see cref="SparseList{T}"/> for fast look up
        /// </summary>
        /// <returns></returns>
        internal SparseList<Actor> actorsInWorld = new(100, MaxActorNum);
        internal readonly Subject<Unit> onActorsUpdate = new();
        private WorldSubsystemCollection subsystemCollection;
        private static ActorWorld current;
        public static ActorWorld Current
        {
            get
            {
                if (!current)
                {
                    current = FindAnyObjectByType<ActorWorld>();
                    if (!current)
                    {

                        current = new GameObject(nameof(ActorWorld)).AddComponent<ActorWorld>();
                    }
                }
                return current;
            }
        }
        private bool isSystemDirty = false;
        private bool isDestroyed;
        private void Awake()
        {
            if (current != null && current != this)
            {
                Destroy(gameObject);
                return;
            }
            current = this;
            subsystemCollection = new WorldSubsystemCollection(this);
        }
        private void Start()
        {
            subsystemCollection.Init();
        }
        private void Update()
        {
            subsystemCollection.Tick();
            if (isSystemDirty)
            {
                subsystemCollection.Rebuild();
                isSystemDirty = false;
            }
        }
        private void FixedUpdate()
        {
            subsystemCollection.FixedTick();
            if (isSystemDirty)
            {
                subsystemCollection.Rebuild();
                isSystemDirty = false;
            }
        }
        private void OnDestroy()
        {
            if (current == this) current = null;
            isDestroyed = true;
            subsystemCollection.Dispose();
            onActorsUpdate.Dispose();
        }
        internal void RegisterActor(Actor actor, ref int actorId)
        {
            if (actorId >= 0 && actorsInWorld.IsValidIndex(actorId))
            {
                Debug.LogError($"[ActorWorld] {actor.gameObject.name} is already registered to world!");
                return;
            }
            actorId = actorsInWorld.Add(actor);
            onActorsUpdate.OnNext(Unit.Default);
        }
        internal void UnregisterActor(Actor actor)
        {
            int actorId = actor.GetActorId();
            if (actorsInWorld.IsValidIndex(actorId))
            {
                actorsInWorld.RemoveAt(actorId);
                onActorsUpdate.OnNext(Unit.Default);
            }
        }
        public Actor GetActor(int id)
        {
            // auto safe check by container
            return actorsInWorld[id];
        }
        public T GetSubsystem<T>() where T : SubsystemBase
        {
            return subsystemCollection.GetSubsystem<T>();
        }
        public SubsystemBase GetSubsystem(Type type)
        {
            return subsystemCollection.GetSubsystem(type);
        }
        internal void RegisterSubsystem<T>(T subsystem) where T : SubsystemBase
        {
            if (isDestroyed)
            {
                Debug.LogError($"[ActorWorld] World is already destroyed!");
                return;
            }
            subsystemCollection.RegisterSubsystem(subsystem);
            isSystemDirty = true;
        }
    }
}
