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
        private void FixedUpdate()
        {
            subsystemCollection.FixedTick();
        }
        private void OnDestroy()
        {
            if (current == this) current = null;
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
        public Actor GetActor(int index)
        {
            if (index >= 0 && index < actorsInWorld.Length)
                return actorsInWorld[index];
            return null;
        }
        public T GetSubsystem<T>() where T : WorldSubsystem
        {
            return subsystemCollection.GetSubsystem<T>();
        }
        public WorldSubsystem GetSubsystem(Type type)
        {
            return subsystemCollection.GetSubsystem(type);
        }
    }
}
