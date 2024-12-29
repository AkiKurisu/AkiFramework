using System;
using UnityEngine;
namespace Chris.Gameplay
{
    public abstract class ActorComponent : MonoBehaviour
    {
        private Actor _actor;
        
        protected virtual void Awake()
        {
            RegisterActorComponent(this, GetComponent<Actor>());
        }
        
        public T GetTActor<T>() where T : Actor
        {
            return _actor as T;
        }
        
        public Actor GetActor()
        {
            return _actor;
        }
        
        protected static void RegisterActorComponent(ActorComponent component, Actor actor)
        {
            if (!actor)
            {
                throw new ArgumentNullException(nameof(actor));
            }
            component._actor = actor;
            Actor.RegisterActorComponent(actor, component);
        }
        
        protected static void UnregisterActor(ActorComponent component, Actor actor)
        {
            if (!actor)
            {
                throw new ArgumentNullException(nameof(actor));
            }
            Actor.RegisterActorComponent(actor, component);
            component._actor = null;
        }
    }
}
