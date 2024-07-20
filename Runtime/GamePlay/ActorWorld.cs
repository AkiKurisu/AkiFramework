using System;
using R3;
using UnityEngine;
namespace Kurisu.Framework
{
    /// <summary>
    /// World for GamePlay actor level.
    /// </summary>
    public class ActorWorld : MonoBehaviour
    {
        internal Actor[] actorsInWorld = new Actor[0];
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
        internal void RegisterActor(Actor actor)
        {
            if (ArrayUtils.IndexOf(actorsInWorld, actor) >= 0)
            {
                Debug.LogError($"[ActorWorld] {actor.gameObject.name} is already registered to world!");
                return;
            }
            ArrayUtils.Add(ref actorsInWorld, actor);
            onActorsUpdate.OnNext(Unit.Default);
        }
        internal void UnregisterActor(Actor actor)
        {
            if (ArrayUtils.IndexOf(actorsInWorld, actor) >= 0)
            {
                ArrayUtils.Remove(ref actorsInWorld, actor);
                onActorsUpdate.OnNext(Unit.Default);
            }
        }
        public Actor GetActor(int id)
        {
            for (int i = 0; i < actorsInWorld.Length; ++i)
            {
                if (actorsInWorld[i].GetActorId() == id) return actorsInWorld[i];
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
