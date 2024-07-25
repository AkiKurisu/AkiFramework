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
                if (IsInLayerMask(actor.layer, layerMask)
                && actor.handle != ignored
                && math.distance(center, actor.position) <= radius
                && InViewAngle(center, actor.position, forward, angle))
                {
                    resultActors.Add(actor.handle);
                }
            }
            [BurstCompile]
            private static bool InViewAngle(in float3 center, in float3 position, in float3 forward, in float angle)
            {
                float3 targetPosition = position;
                targetPosition.y = center.y;

                float3 directionToTarget = math.normalize(targetPosition - center);

                return Angle(forward, directionToTarget) <= angle / 2;
            }
            [BurstCompile]
            private static float Angle(in float3 from, in float3 to)
            {
                float num = math.sqrt(math.sqrt(math.dot(from, from)) * math.sqrt(math.dot(to, to)));
                if (num < 1E-15f)
                {
                    return 0f;
                }
                float num2 = math.clamp(math.dot(from, to) / num, -1f, 1f);
                return (float)math.acos(num2) * 57.29578f;
            }
            [BurstCompile]
            private static bool IsInLayerMask(in int layer, in LayerMask mask)
            {
                return (mask.value & (1 << layer)) != 0;
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
