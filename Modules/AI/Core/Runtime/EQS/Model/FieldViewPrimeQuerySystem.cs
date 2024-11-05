using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kurisu.Framework.Schedulers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework.AI.EQS
{
    public struct FieldViewPrimeQueryCommand
    {
        public ActorHandle self;
        public FieldViewPrime fieldView;
        public LayerMask layerMask;
    }
    public class FieldViewPrimeQuerySystem : WorldSubsystem
    {
        /// <summary>
        /// Batch field view query, performe better than <see cref="EnvironmentQuery.OverlapFieldViewJob"/>
        /// </summary>
        [BurstCompile]
        private struct OverlapFieldViewBatchJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<FieldViewPrimeQueryCommand> datas;
            [ReadOnly]
            public NativeArray<ActorData> actors;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeParallelMultiHashMap<int, ActorHandle> resultActors;
            [BurstCompile]
            public void Execute(int index)
            {
                FieldViewPrimeQueryCommand source = datas[index];
                ActorData self = actors[source.self.GetIndex()];
                float3 forward = math.mul(self.rotation, new float3(0, 0, 1));
                for (int i = 0; i < actors.Length; i++)
                {
                    if (i == index) continue;
                    ActorData actor = actors[i];
                    if (!MathUtils.IsInLayerMask(actor.layer, source.layerMask)) continue;
                    float radius = source.fieldView.PolygonRadius;
                    float centerDistance = math.distance(self.position + forward * radius, actor.position);
                    // Inside
                    if (centerDistance >= radius)
                    {
                        using var polygons = AllocatePolygonCorners(source.fieldView, self.position, self.rotation, forward, Allocator.Temp);
                        if (!MathUtils.IsPointInPolygon(polygons, actor.position))
                        {
                            // When target is nearly on edge, detect whether target is in fov now
                            const float threshold = 0.9f;
                            if (centerDistance >= threshold * radius && MathUtils.InViewAngle(self.position, actor.position, forward, source.fieldView.Angle))
                            {
                                resultActors.Add(index, actor.handle);
                            }
                            continue;
                        }
                    }
                    // Outside
                    if (math.distance(self.position, actor.position) <= source.fieldView.Radius
                    && MathUtils.InViewAngle(self.position, actor.position, forward, source.fieldView.Angle))
                    {
                        resultActors.Add(index, actor.handle);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static NativeArray<float3> AllocatePolygonCorners(FieldViewPrime fieldViewPrime, float3 position, quaternion rotation, float3 forward, Allocator allocator)
            {
                float radius = fieldViewPrime.PolygonRadius;
                var frustumCorners = new NativeArray<float3>(fieldViewPrime.Sides, allocator);
                float angleStep = 360f / fieldViewPrime.Sides;

                for (int i = 0; i < fieldViewPrime.Sides; i++)
                {
                    float angle = math.degrees(i * angleStep);
                    frustumCorners[i] = new float3(math.cos(angle) * radius, 0, math.sin(angle) * radius);
                }
                for (int i = 0; i < frustumCorners.Length; i++)
                {
                    frustumCorners[i] = position + forward * radius + math.mul(rotation, frustumCorners[i]);
                }
                return frustumCorners;
            }
        }
        private SchedulerHandle updateTickHandle;
        private SchedulerHandle lateUpdateTickHandle;
        /// <summary>
        /// Set sysytem tick frame
        /// </summary>
        /// <value></value>
        public static int FramePerTick { get; set; } = DefaultFramePerTick;
        /// <summary>
        /// Default tick frame: 2 fps
        /// </summary>
        public const int DefaultFramePerTick = 25;
        private readonly Dictionary<ActorHandle, int> handleIndices = new();
        private NativeParallelMultiHashMap<int, ActorHandle> results;
        private NativeList<FieldViewPrimeQueryCommand> commands;
        private NativeArray<FieldViewPrimeQueryCommand> execution;
        private NativeParallelMultiHashMap<int, ActorHandle> cache;
        private NativeArray<ActorData> actorData;
        private JobHandle jobHandle;
        private static readonly ProfilerMarker ScheduleJobPM = new("FieldViewPrimeQuerySystem.ScheduleJob");
        private static readonly ProfilerMarker CompleteJobPM = new("FieldViewPrimeQuerySystem.CompleteJob");
        protected override void Initialize()
        {
            Assert.IsFalse(FramePerTick <= 3);
            commands = new NativeList<FieldViewPrimeQueryCommand>(100, Allocator.Persistent);
            Scheduler.WaitFrame(ref updateTickHandle, FramePerTick, ScheduleJob, TickFrame.FixedUpdate, isLooped: true);
            // Allow job scheduled in 3 frames
            Scheduler.WaitFrame(ref lateUpdateTickHandle, 3, CompleteJob, TickFrame.FixedUpdate, isLooped: true);
            lateUpdateTickHandle.Pause();
        }
        private void ScheduleJob(int _)
        {
            using (ScheduleJobPM.Auto())
            {

                if (commands.Length == 0) return;

                actorData = Get<ActorQuerySystem>().GetAllActors(Allocator.TempJob);
                results = new NativeParallelMultiHashMap<int, ActorHandle>(1024, Allocator.Persistent);
                execution = commands.ToArray(Allocator.TempJob);
                jobHandle = new OverlapFieldViewBatchJob()
                {
                    actors = actorData,
                    datas = execution,
                    resultActors = results
                }.Schedule(execution.Length, 32);
                lateUpdateTickHandle.Resume();
            }
        }
        private void CompleteJob(int _)
        {
            using (CompleteJobPM.Auto())
            {
                jobHandle.Complete();
                cache.DisposeSafe();
                cache = results;
                actorData.Dispose();
                execution.Dispose();
                lateUpdateTickHandle.Pause();
            }
        }
        protected override void Release()
        {
            jobHandle.Complete();
            commands.Dispose();
            execution.DisposeSafe();
            cache.DisposeSafe();
            results.DisposeSafe();
            actorData.DisposeSafe();
            lateUpdateTickHandle.Dispose();
            updateTickHandle.Dispose();
        }
        public void EnqueueCommand(FieldViewPrimeQueryCommand command)
        {
            if (handleIndices.TryGetValue(command.self, out var index))
            {
                commands[index] = command;
            }
            else
            {
                int length = commands.Length;
                handleIndices[command.self] = length;
                commands.Add(command);
            }
        }
        public void GetActorsInFieldView(ActorHandle handle, List<Actor> actors)
        {
            if (!handleIndices.TryGetValue(handle, out var index))
            {
                Debug.LogWarning($"[FieldViewPrimeQuerySystem] Actor {handle.Handle}'s field view has not been initialized");
                return;
            }
            if (!cache.IsCreated) return;

            var world = GetWorld();
            foreach (var id in cache.GetValuesForKey(index))
            {
                actors.Add(world.GetActor(id));
            }
        }
    }
}
