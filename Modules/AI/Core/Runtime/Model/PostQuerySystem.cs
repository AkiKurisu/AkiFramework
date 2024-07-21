using System;
using System.Collections.Generic;
using Kurisu.Framework.Schedulers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Profiling;
namespace Kurisu.Framework.AI
{
    /// <summary>
    /// Command for schedule post query job
    /// </summary>
    public struct PostQueryCommand
    {
        public int selfId;
        public int targetId;
        public float3 offset;
        public int layerMask;
        public PostQuery postQuery;
    }
    public class PostQuerySystem : DynamicSubsystem
    {
        [BurstCompile]
        public struct PrepareCommandJob : IJobParallelFor
        {
            [ReadOnly]
            public PostQueryCommand command;
            [ReadOnly]
            public ActorData source;
            [ReadOnly]
            public ActorData target;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<RaycastCommand> raycastCommands;
            public void Execute(int index)
            {
                var direction = math.normalize(source.position - target.position);

                float angle = command.postQuery.Angle;
                quaternion rot = quaternion.RotateY(math.radians(math.lerp(-angle, angle, (float)index / command.postQuery.Step)));

                rot = math.mul(rot, quaternion.AxisAngle(math.up(), math.radians(angle)));

                raycastCommands[index] = new RaycastCommand()
                {
                    from = target.position + command.offset,
                    direction = math.rotate(rot, direction),
                    distance = command.postQuery.Distance,
                    queryParameters = new QueryParameters() { layerMask = command.layerMask }
                };
            }
        }
        /// <summary>
        /// Worker per actor
        /// </summary>
        private class PostQueryWorker
        {
            public PostQueryCommand executeCommand;
            public NativeList<float3> posts;
            public NativeArray<RaycastHit> hits;
            public NativeArray<RaycastCommand> raycastCommands;
            public JobHandle jobHandle;
            public bool IsRunning { get; private set; }
            public PostQueryWorker()
            {
                posts = new(Allocator.Persistent);
            }
            public ReadOnlySpan<float3> GetPosts()
            {
                return posts.AsReadOnly();
            }
            public void ExecuteCommand(ref PostQueryCommand command, ref NativeArray<ActorData> actorDatas)
            {
                IsRunning = true;
                executeCommand = command;
                int length = command.postQuery.Step * 2;
                raycastCommands.DisposeSafe();
                raycastCommands = new(length, Allocator.TempJob);
                hits.DisposeSafe();
                hits = new(length, Allocator.TempJob);
                var job = new PrepareCommandJob()
                {
                    command = command,
                    raycastCommands = raycastCommands,
                    source = FindActor(ref actorDatas, command.selfId),
                    target = FindActor(ref actorDatas, command.targetId)
                };
                jobHandle = job.Schedule(length, 32, default);
                jobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, raycastCommands.Length, jobHandle);
            }
            private static ActorData FindActor(ref NativeArray<ActorData> actorDatas, int id)
            {
                for (int i = 0; i < actorDatas.Length; ++i)
                {
                    if (actorDatas[i].id == id) return actorDatas[i];
                }
                return default;
            }
            public void Complete()
            {
                IsRunning = false;
                jobHandle.Complete();
                posts.Clear();
                bool hasHit = false;
                foreach (var hit in hits)
                {
                    bool isHit = hit.point != default;
                    if (!hasHit && isHit)
                    {
                        posts.Add(hit.point);
                    }
                    hasHit = isHit;
                }
                raycastCommands.Dispose();
                hits.Dispose();
            }
            public void Dispose()
            {
                posts.Dispose();
                hits.DisposeSafe();
                raycastCommands.DisposeSafe();
            }
        }
        private readonly Queue<PostQueryCommand> commandBuffer = new();
        private SchedulerHandle updateTickHandle;
        private SchedulerHandle lateUpdateTickHandle;
        private NativeArray<int> batchActors;
        private int batchLength;
        private readonly Dictionary<int, PostQueryWorker> workerDic = new();
        /// <summary>
        /// Set system parallel workers count
        /// </summary>
        /// <value></value>
        public static int MaxWorkerCount { get; set; } = DefaultWorkerCount;
        /// <summary>
        /// Default parallel workers count: 10
        /// </summary>
        public const int DefaultWorkerCount = 10;
        /// <summary>
        /// Set sysytem tick rate
        /// </summary>
        /// <value></value>
        public static float TickRate { get; set; } = DefaultTickRate;
        /// <summary>
        /// Default rate: 25 fps
        /// </summary>
        public const float DefaultTickRate = 0.04f;
        private static readonly ProfilerMarker ConsumeCommandsPM = new("PostQuerySystem.ConsumeCommands");
        private static readonly ProfilerMarker CompleteCommandsPM = new("PostQuerySystem.CompleteCommands");
        public void EnqueueCommand(PostQueryCommand command)
        {
            commandBuffer.Enqueue(command);
        }
        protected override void Initialize()
        {
            Scheduler.Delay(ref updateTickHandle, TickRate, ConsumeCommands, TickFrame.Update, isLooped: true);
            Scheduler.Delay(ref lateUpdateTickHandle, TickRate, CompleteCommands, TickFrame.LateUpdate, isLooped: true);
            batchActors = new NativeArray<int>(MaxWorkerCount, Allocator.Persistent);
        }
        private void ConsumeCommands(float deltaTime)
        {
            using (ConsumeCommandsPM.Auto())
            {
                batchLength = 0;
                var actorDatas = GetWorld().GetSubsystem<ActorQuerySystem>().GetAllActors(Allocator.Temp);
                while (batchLength < MaxWorkerCount)
                {
                    if (!commandBuffer.TryDequeue(out var command))
                    {
                        break;
                    }

                    if (!workerDic.TryGetValue(command.selfId, out var worker))
                    {
                        worker = workerDic[command.selfId] = new();
                    }
                    if (worker.IsRunning)
                        break;
                    worker.ExecuteCommand(ref command, ref actorDatas);
                    batchActors[batchLength] = command.selfId;
                    batchLength++;
                }
                actorDatas.Dispose();
            }
        }
        private void CompleteCommands(float deltaTime)
        {
            using (CompleteCommandsPM.Auto())
            {
                for (int i = 0; i < batchLength; ++i)
                {
                    workerDic[batchActors[i]].Complete();
                }
            }
        }
        public ReadOnlySpan<float3> GetPosts(int actorId)
        {
            if (workerDic.TryGetValue(actorId, out var worker))
                return worker.GetPosts();
            return default;
        }
        public bool IsComplete(int actorId)
        {
            if (workerDic.TryGetValue(actorId, out var worker))
                return !worker.IsRunning;
            return true;
        }

        protected override void Release()
        {
            batchActors.Dispose();
            updateTickHandle.Dispose();
            lateUpdateTickHandle.Dispose();
            foreach (var worker in workerDic.Values)
            {
                worker.Dispose();
            }
            workerDic.Clear();
        }
    }
}
