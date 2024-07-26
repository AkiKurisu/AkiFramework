using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.Framework.AI.EQS
{
    /// <summary>
    /// Field view query data provider associated with an Actor as component
    /// </summary>
    public class FieldViewQueryComponent : ActorComponent
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
            system = DynamicSubsystem.Get<FieldViewQuerySystem>(ActorWorld.Current);
            if (system == null)
            {
                Debug.LogError($"[FieldViewQueryComponent] Can not get FieldViewQuerySystem dynamically.");
            }
        }
        /// <summary>
        /// Requst a new query from target's field view
        /// </summary>
        /// <returns></returns>
        public bool RequestFieldViewQuery()
        {
            if (system == null)
            {
                return false;
            }
            system.EnqueueCommand(new FieldViewCommand()
            {
                self = GetActor().GetActorHandle(),
                fieldView = FieldView,
                layerMask = QueryLayerMask
            });
            return true;
        }
        /// <summary>
        /// Query actors overlap in field of view from cache
        /// </summary>
        /// <param name="actors"></param>
        public void CollectViewActors(List<Actor> actors)
        {
            system.GetActorsInFieldView(GetActor().GetActorHandle(), actors);
        }
        /// <summary>
        /// Query actors overlap in field of view from cache
        /// </summary>
        /// <param name="actors"></param>
        public void CollectViewActors<T>(List<T> actors) where T : Actor
        {
            var list = ListPool<Actor>.Get();
            CollectViewActors(list);
            foreach (var actor in list)
            {
                if (actor is T tActor) actors.Add(tActor);
            }
            ListPool<Actor>.Release(list);
        }
        private void OnDrawGizmos()
        {
            FieldView.DrawGizmos(transform.position + Offset, transform.forward);
        }
    }
}
