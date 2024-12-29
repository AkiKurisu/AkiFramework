using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Chris.AI.EQS
{
    public interface IFieldViewQueryComponent
    {
        /// <summary>
        /// Requst a new query from target's field view
        /// </summary>
        /// <returns></returns>
        bool RequestFieldViewQuery();
        /// <summary>
        /// Detect whether can see the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fromPosition"></param>
        /// <param name="viewDirection"></param>
        /// <param name="filterTags"></param>
        /// <returns></returns>
        bool Detect(Vector3 target, Vector3 fromPosition, Quaternion fromRotation, string[] filterTags = null);
        /// <summary>
        /// Query actors overlap in field of view from cache
        /// </summary>
        /// <param name="actors"></param>
        void CollectViewActors(List<Actor> actors);
        /// <summary>
        /// Query actors overlap in field of view from cache
        /// </summary>
        /// <param name="actors"></param>
        void CollectViewActors<T>(List<T> actors) where T : Actor;
    }
    public abstract class FieldViewQueryComponentBase : ActorComponent, IFieldViewQueryComponent
    {
        public abstract void CollectViewActors(List<Actor> actors);
        public abstract void CollectViewActors<T>(List<T> actors) where T : Actor;
        public abstract bool Detect(Vector3 target, Vector3 fromPosition, Quaternion fromRotation, string[] filterTags = null);
        public abstract bool RequestFieldViewQuery();
    }
    /// <summary>
    /// Field view query data provider associated with an Actor as component
    /// </summary>
    public class FieldViewQueryComponent : FieldViewQueryComponentBase
    {
        [Header("Data")]
        public FieldView FieldView = new()
        {
            Radius = 20,
            Angle = 120
        };
        public LayerMask QueryLayerMask;
        private FieldViewQuerySystem system;
        [Header("Gizmos")]
        public Vector3 Offset;
        private void Start()
        {
            system = WorldSubsystem.GetOrCreate<FieldViewQuerySystem>();
            if (system == null)
            {
                Debug.LogError($"[FieldViewQueryComponent] Can not get FieldViewQuerySystem dynamically.");
            }
        }
        public override bool RequestFieldViewQuery()
        {
            if (system == null)
            {
                return false;
            }
            system.EnqueueCommand(new FieldViewQueryCommand()
            {
                self = GetActor().GetActorHandle(),
                fieldView = FieldView,
                layerMask = QueryLayerMask
            });
            return true;
        }
        public override void CollectViewActors(List<Actor> actors)
        {
            system.GetActorsInFieldView(GetActor().GetActorHandle(), actors);
        }
        public override void CollectViewActors<T>(List<T> actors)
        {
            var list = ListPool<Actor>.Get();
            CollectViewActors(list);
            foreach (var actor in list)
            {
                if (actor is T tActor) actors.Add(tActor);
            }
            ListPool<Actor>.Release(list);
        }
        public override bool Detect(Vector3 target, Vector3 fromPosition, Quaternion fromRotation, string[] filterTags = null)
        {
            return FieldView.Detect(target, fromPosition, fromRotation, QueryLayerMask, filterTags);
        }
        private void OnDrawGizmos()
        {
            FieldView.DrawGizmos(transform.position + Offset, transform.forward);
        }
    }
}
