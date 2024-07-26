using System;
using Kurisu.Framework.Collections;
using R3;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework
{
    /// <summary>
    /// Struct to represent an GamePlay actor level entity
    /// </summary>
    public readonly struct ActorHandle
    {
        public readonly ulong Handle { get; }
        public const int IndexBits = 24;
        public const int SerialNumberBits = 40;
        public const int MaxIndex = 1 << IndexBits;
        public const ulong MaxSerialNumber = (ulong)1 << SerialNumberBits;
        public readonly int GetIndex() => (int)(Handle & MaxIndex - 1);
        public readonly ulong GetSerialNumber() => Handle >> IndexBits;
        public readonly bool IsValid()
        {
            return Handle != 0;
        }
        public ActorHandle(ulong serialNum, int index)
        {
            Assert.IsTrue(index >= 0 && index < MaxIndex);
            Assert.IsTrue(serialNum < MaxSerialNumber);
#pragma warning disable CS0675
            Handle = (serialNum << IndexBits) | (ulong)index;
#pragma warning restore CS0675
        }

        public static bool operator ==(ActorHandle left, ActorHandle right)
        {
            return left.Handle == right.Handle;
        }
        public static bool operator !=(ActorHandle left, ActorHandle right)
        {
            return left.Handle != right.Handle;
        }
        public override bool Equals(object obj)
        {
            if (obj is not ActorHandle handle) return false;
            return handle.Handle == Handle;
        }
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
    }
    /// <summary>
    /// World for GamePlay actor level.
    /// </summary>
    public class ActorWorld : MonoBehaviour
    {
        private ulong serialNum = 1;
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
        internal void RegisterActor(Actor actor, ref ActorHandle handle)
        {
            if (GetActor(handle) != null)
            {
                Debug.LogError($"[ActorWorld] {actor.gameObject.name} is already registered to world!");
                return;
            }
            int index = actorsInWorld.Add(actor);
            handle = new ActorHandle(serialNum, index);
            onActorsUpdate.OnNext(Unit.Default);
        }
        internal void UnregisterActor(Actor actor)
        {
            var handle = actor.GetActorHandle();
            int index = handle.GetIndex();
            if (actorsInWorld.IsAllocated(index))
            {
                var current = actorsInWorld[index];
                if (current.GetActorHandle().GetSerialNumber() != handle.GetSerialNumber())
                {
                    Debug.LogWarning($"[ActorWorld] Actor {handle.Handle} has already been removed from world!");
                    return;
                }
                // increase serial num as version update
                ++serialNum;
                actorsInWorld.RemoveAt(index);
                onActorsUpdate.OnNext(Unit.Default);
            }
        }
        public Actor GetActor(ActorHandle handle)
        {
            int index = handle.GetIndex();
            if (handle.IsValid() && actorsInWorld.IsAllocated(index))
            {
                var actor = actorsInWorld[index];
                if (actor.GetActorHandle().GetSerialNumber() != handle.GetSerialNumber()) return null;
                return actor;
            }
            return null;
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
