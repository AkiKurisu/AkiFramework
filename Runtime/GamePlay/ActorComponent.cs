using System;
using UnityEngine;
namespace Kurisu.Framework
{
    public abstract class ActorComponent : MonoBehaviour
    {
        private Actor actor;
        protected virtual void Awake()
        {
            RegisterActorComponent(this, GetComponent<Actor>());
        }
        public T GetTActor<T>() where T : Actor
        {
            return actor as T;
        }
        public Actor GetActor()
        {
            return actor;
        }
        protected static void RegisterActorComponent(ActorComponent component, Actor actor)
        {
            if (!actor)
            {
                throw new ArgumentNullException(nameof(actor));
            }
            component.actor = actor;
            Actor.RegisterActorComponent(actor, component);
        }
        protected static void UnregisterActor(ActorComponent component, Actor actor)
        {
            if (!actor)
            {
                throw new ArgumentNullException(nameof(actor));
            }
            Actor.RegisterActorComponent(actor, component);
            component.actor = null;
        }
    }
}
