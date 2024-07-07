using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Kurisu.Framework.EQS
{
    /// <summary>
    /// Actor world data
    /// </summary>
    public struct ActorData
    {
        public int instanceId;
        public int layer;
        public float3 position;
    }
    /// <summary>
    /// Represent a DOP world query system.
    /// </summary>
    public class ActorWorld : MonoBehaviour
    {
        private Actor[] actorsInWorld;
        private NativeArray<ActorData> actorData;
        private static ActorWorld current;
        public static ActorWorld Current
        {
            get
            {
                if (!current)
                {
                    // Must add in scene
                    current = FindAnyObjectByType<ActorWorld>();
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
        }
        private void Start()
        {
            actorsInWorld = FindObjectsByType<Actor>(FindObjectsSortMode.InstanceID);
            actorData = new NativeArray<ActorData>(actorsInWorld.Length, Allocator.Persistent);
        }
        private void FixedUpdate()
        {
            for (int i = 0; i < actorData.Length; ++i)
            {
                actorData[i] = new ActorData()
                {
                    instanceId = actorsInWorld[i].GetInstanceID(),
                    layer = actorsInWorld[i].gameObject.layer,
                    position = actorsInWorld[i].transform.position
                };
            }
        }
        public NativeArray<ActorData> GetAllActors(Allocator allocator)
        {
            return new NativeArray<ActorData>(actorData, allocator);
        }
        public Actor GetActor(int index)
        {
            if (index >= 0 && index < actorsInWorld.Length)
                return actorsInWorld[index];
            return null;
        }
        private void OnDestroy()
        {
            if (current == this) current = null;
            if (actorData.IsCreated) actorData.Dispose();
        }
    }
    public static class EnvironmentQuery
    {
        [BurstCompile]
        public struct OverlapFieldViewJob : IJob
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
            public int ignoreInstanceId;
            [ReadOnly]
            public NativeArray<ActorData> actors;
            public NativeList<int> resultActors;
            [BurstCompile]
            public void Execute()
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    ActorData actor = actors[i];
                    if (IsInLayerMask(actor.layer, layerMask)
                    && actor.instanceId != ignoreInstanceId
                    && math.distance(center, actor.position) <= radius
                    && InViewAngle(center, actor.position, forward, angle))
                    {
                        resultActors.Add(i);
                    }
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
        /// Query actors overlap in fieldView
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
            var resultActors = new NativeList<int>(Allocator.TempJob);
            var actorData = ActorWorld.Current.GetAllActors(Allocator.TempJob);
            var job = new OverlapFieldViewJob()
            {
                center = position,
                forward = forward,
                radius = radius,
                angle = angle,
                layerMask = targetMask,
                ignoreInstanceId = ignoredActor == null ? -1 : ignoredActor.GetInstanceID(),
                actors = actorData,
                resultActors = resultActors
            };
            job.Schedule().Complete();
            foreach (var id in resultActors)
            {
                actors.Add(ActorWorld.Current.GetActor(id));
            }
            actorData.Dispose();
            resultActors.Dispose();
        }
    }
}
