using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Chris.AI.EQS
{
    /// <summary>
    /// Field view prime query data provider associated with an Actor as component
    /// </summary>
    public class FieldViewPrimeQueryComponent : FieldViewQueryComponentBase
    {
        [Header("Data")]
        public FieldViewPrime FieldView = new()
        {
            Radius = 20,
            Angle = 120,
            Sides = 8,
            Blend = 0.5f
        };
        public LayerMask QueryLayerMask;
        private FieldViewPrimeQuerySystem system;
        [Header("Gizmos")]
        public Vector3 Offset;
        private void Start()
        {
            system = WorldSubsystem.Get<FieldViewPrimeQuerySystem>();
            if (system == null)
            {
                Debug.LogError($"[FieldViewPrimeQueryComponent] Can not get FieldViewPrimeQuerySystem dynamically.");
            }
        }
        public override bool RequestFieldViewQuery()
        {
            if (system == null)
            {
                return false;
            }
            system.EnqueueCommand(new FieldViewPrimeQueryCommand()
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
            FieldView.DrawGizmos(transform.position + Offset, transform.rotation);
        }
    }
}
