using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Kurisu.Framework.AI.EQS
{
    /// <summary>
    /// API for query actors
    /// </summary>
    public static class EnvironmentQuery
    {
        [BurstCompile]
        public struct OverlapFieldViewJob : IJobParallelFor
        {
            [ReadOnly]
            public float3 center;
            [ReadOnly]
            public float3 forward;
            [ReadOnly]
            public LayerMask layerMask;
            [ReadOnly]
            public float radius;
            [ReadOnly]
            public float angle;
            [ReadOnly]
            public ActorHandle ignored;
            [ReadOnly]
            public NativeArray<ActorData> actors;
            [NativeDisableParallelForRestriction]
            public NativeList<ActorHandle> resultActors;
            [BurstCompile]
            public void Execute(int index)
            {
                ActorData actor = actors[index];
                if (MathUtils.IsInLayerMask(actor.layer, layerMask)
                && actor.handle != ignored
                && math.distance(center, actor.position) <= radius
                && MathUtils.InViewAngle(center, actor.position, forward, angle))
                {
                    resultActors.Add(actor.handle);
                }
            }
        }
        /// <summary>
        /// Query actors overlap in field of view immediately
        /// </summary>
        /// <param name="actors"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="radius"></param>
        /// <param name="angle"></param>
        /// <param name="targetMask"></param>
        /// <param name="ignoredActor"></param>
        public static void OverlapFieldView(List<Actor> actors, Vector3 position, Vector3 forward, float radius, float angle, LayerMask targetMask, Actor ignoredActor = null)
        {
            var resultActors = new NativeList<ActorHandle>(Allocator.TempJob);
            var actorData = ActorWorld.Current.GetSubsystem<ActorQuerySystem>().GetAllActors(Allocator.TempJob);
            var job = new OverlapFieldViewJob()
            {
                center = position,
                forward = forward,
                radius = radius,
                angle = angle,
                layerMask = targetMask,
                ignored = ignoredActor == null ? default : ignoredActor.GetActorHandle(),
                actors = actorData,
                resultActors = resultActors
            };
            job.Schedule(actorData.Length, 32).Complete();
            foreach (var id in resultActors)
            {
                actors.Add(ActorWorld.Current.GetActor(id));
            }
            actorData.Dispose();
            resultActors.Dispose();
        }
    }
}
