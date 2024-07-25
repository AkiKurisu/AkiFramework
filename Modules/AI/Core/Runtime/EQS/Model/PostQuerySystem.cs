using System;
using System.Collections.Generic;
using Kurisu.Framework.Schedulers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Profiling;
using UnityEngine.Assertions;
namespace Kurisu.Framework.AI.EQS
{
    /// <summary>
    /// Command for schedule post query job
    /// </summary>
    public struct PostQueryCommand
    {
        public ActorHandle self;
        public ActorHandle target;
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
            [ReadOnly]
            public int length;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeArray<RaycastCommand> raycastCommands;
            public void Execute(int index)
            {
                var direction = math.normalize(source.position - target.position);

                float angle = command.postQuery.Angle / 2;

                quaternion rot = quaternion.RotateY(math.radians(math.lerp(-angle, angle, (float)index / length)));

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
                int length = command.postQuery.Step * command.postQuery.Depth;
                raycastCommands.DisposeSafe();
                raycastCommands = new(length, Allocator.TempJob);
                hits.DisposeSafe();
                hits = new(length, Allocator.TempJob);
                var job = new PrepareCommandJob()
                {
                    command = command,
                    raycastCommands = raycastCommands,
                    length = length,
                    source = actorDatas[command.self.GetIndex()],
                    target = actorDatas[command.target.GetIndex()]
                };
                jobHandle = job.Schedule(length, 32, default);
                jobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, raycastCommands.Length, jobHandle);
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
        private NativeArray<ActorHandle> batchActors;
        private int batchLength;
        private readonly Dictionary<ActorHandle, PostQueryWorker> workerDic = new();
        /// <summary>
        /// Set system parallel workers count
        /// </summary>
        /// <value></value>
        public static int MaxWorkerCount { get; set; } = DefaultWorkerCount;
        /// <summary>
        /// Default parallel workers count: 5
        /// </summary>
        public const int DefaultWorkerCount = 5;
        /// <summary>
        /// Set sysytem tick frame
        /// </summary>
        /// <value></value>
        public static int FramePerTick { get; set; } = DefaultFramePerTick;
        /// <summary>
        /// Default tick frame: 2 fps
        /// </summary>
        public const int DefaultFramePerTick = 25;
        private static readonly ProfilerMarker ConsumeCommandsPM = new("PostQuerySystem.ConsumeCommands");
        private static readonly ProfilerMarker CompleteCommandsPM = new("PostQuerySystem.CompleteCommands");
        public void EnqueueCommand(PostQueryCommand command)
        {
            commandBuffer.Enqueue(command);
        }
        protected override void Initialize()
        {
            Assert.IsFalse(FramePerTick <= 3);
            Scheduler.WaitFrame(ref updateTickHandle, FramePerTick, ConsumeCommands, TickFrame.FixedUpdate, isLooped: true);
            // Allow job scheduled in 3 frames
            Scheduler.WaitFrame(ref lateUpdateTickHandle, 3, CompleteCommands, TickFrame.FixedUpdate, isLooped: true);
            lateUpdateTickHandle.Pause();
            batchActors = new NativeArray<ActorHandle>(MaxWorkerCount, Allocator.Persistent);
        }
        private void ConsumeCommands(int _)
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

                    if (!workerDic.TryGetValue(command.self, out var worker))
                    {
                        worker = workerDic[command.self] = new();
                    }
                    if (worker.IsRunning)
                        continue;

                    worker.ExecuteCommand(ref command, ref actorDatas);
                    batchActors[batchLength] = command.self;
                    batchLength++;
                }
                actorDatas.Dispose();
            }
            lateUpdateTickHandle.Resume();
        }
        private void CompleteCommands(int _)
        {
            using (CompleteCommandsPM.Auto())
            {
                for (int i = 0; i < batchLength; ++i)
                {
                    workerDic[batchActors[i]].Complete();
                }
            }
            lateUpdateTickHandle.Pause();
        }
        public ReadOnlySpan<float3> GetPosts(ActorHandle handle)
        {
            if (workerDic.TryGetValue(handle, out var worker))
                return worker.GetPosts();
            return default;
        }
        public bool IsComplete(ActorHandle handle)
        {
            if (workerDic.TryGetValue(handle, out var worker))
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
