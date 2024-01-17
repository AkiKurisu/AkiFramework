using System;
namespace Kurisu.Framework
{
    /// <summary>
    /// Behavior for entity using Flyweight pattern.
    /// Can be used with <see cref="SubclassSelector"/>
    /// </summary>
    [Serializable]
    public abstract class EntityBehavior
    {
        protected int entityID;
        public void Run(IEntity entity)
        {
            entityID = entity.EntityID;
            OnRun(entity);
        }
        protected abstract void OnRun(IEntity obj);
    }
}