using System.Collections.Generic;
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
    public struct FieldViewCommand
    {
        public ActorHandle self;
        public FieldView fieldView;
        public LayerMask layerMask;
    }
    public class FieldViewQuerySystem : DynamicSubsystem
    {
        /// <summary>
        /// Batch field view query, performe better than <see cref="EnvironmentQuery.OverlapFieldViewJob"/>
        /// </summary>
        [BurstCompile]
        private struct OverlapFieldViewBatchJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<FieldViewCommand> datas;
            [ReadOnly]
            public NativeArray<ActorData> actors;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeParallelMultiHashMap<int, ActorHandle> resultActors;
            [BurstCompile]
            public void Execute(int index)
            {
                FieldViewCommand source = datas[index];
                ActorData self = actors[datas[index].self.GetIndex()];
                float3 forward = math.mul(self.rotation, new float3(0, 0, 1));
                for (int i = 0; i < actors.Length; i++)
                {
                    if (i == index) continue;
                    ActorData actor = actors[i];
                    if (IsInLayerMask(actor.layer, source.layerMask)
                    && math.distance(self.position, actor.position) <= source.fieldView.Radius
                    && InViewAngle(self.position, actor.position, forward, source.fieldView.Angle))
                    {
                        resultActors.Add(index, actor.handle);
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
        private NativeList<FieldViewCommand> commands;
        private NativeArray<FieldViewCommand> execution;
        private NativeParallelMultiHashMap<int, ActorHandle> cache;
        private NativeArray<ActorData> actorData;
        private JobHandle jobHandle;
        private static readonly ProfilerMarker ScheduleJobPM = new("FieldViewQuerySystem.ScheduleJob");
        private static readonly ProfilerMarker CompleteJobPM = new("FieldViewQuerySystem.CompleteJob");
        protected override void Initialize()
        {
            Assert.IsFalse(FramePerTick <= 3);
            commands = new NativeList<FieldViewCommand>(100, Allocator.Persistent);
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

                actorData = ActorWorld.Current.GetSubsystem<ActorQuerySystem>().GetAllActors(Allocator.TempJob);
                results = new NativeParallelMultiHashMap<int, ActorHandle>(1024, Allocator.Persistent);
                execution = commands.ToArray(Allocator.TempJob);
                jobHandle = new OverlapFieldViewBatchJob()
                {
                    actors = actorData,
                    datas = execution,
                    resultActors = results
                }.Schedule(commands.Length, 32);
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
        public void EnqueueCommand(FieldViewCommand command)
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
                Debug.LogWarning($"Actor {handle.Handle}'s field view has not been initialized");
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
