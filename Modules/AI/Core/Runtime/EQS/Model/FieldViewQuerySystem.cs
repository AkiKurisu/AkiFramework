using System.Collections.Generic;
using Chris.Gameplay;
using Chris.Schedulers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
namespace Chris.AI.EQS
{
    public struct FieldViewQueryCommand
    {
        public ActorHandle self;
        public FieldView fieldView;
        public LayerMask layerMask;
    }
    
    public class FieldViewQuerySystem : WorldSubsystem
    {
        /// <summary>
        /// Batch field view query, perform better than <see cref="EnvironmentQuery.OverlapFieldViewJob"/>
        /// </summary>
        [BurstCompile]
        private struct OverlapFieldViewBatchJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<FieldViewQueryCommand> datas;
            [ReadOnly]
            public NativeArray<ActorData> actors;
            [WriteOnly, NativeDisableParallelForRestriction]
            public NativeParallelMultiHashMap<int, ActorHandle> resultActors;
            [BurstCompile]
            public void Execute(int index)
            {
                FieldViewQueryCommand source = datas[index];
                ActorData self = actors[source.self.GetIndex()];
                float3 forward = math.mul(self.Rotation, new float3(0, 0, 1));
                for (int i = 0; i < actors.Length; i++)
                {
                    if (i == index) continue;
                    ActorData actor = actors[i];
                    if (MathUtils.IsInLayerMask(actor.Layer, source.layerMask)
                    && math.distance(self.Position, actor.Position) <= source.fieldView.radius
                    && MathUtils.InViewAngle(self.Position, actor.Position, forward, source.fieldView.angle))
                    {
                        resultActors.Add(index, actor.Handle);
                    }
                }
            }
        }
        private SchedulerHandle _updateTickHandle;
        
        private SchedulerHandle _lateUpdateTickHandle;
        
        /// <summary>
        /// Set system tick frame
        /// </summary>
        /// <value></value>
        public static int FramePerTick { get; set; } = DefaultFramePerTick;
        
        /// <summary>
        /// Default tick frame: 2 fps
        /// </summary>
        public const int DefaultFramePerTick = 25;
        
        private readonly Dictionary<ActorHandle, int> _handleIndices = new();
        
        private NativeParallelMultiHashMap<int, ActorHandle> _results;
        
        private NativeList<FieldViewQueryCommand> _commands;
        
        private NativeArray<FieldViewQueryCommand> _execution;
        
        private NativeParallelMultiHashMap<int, ActorHandle> _cache;
        
        private NativeArray<ActorData> _actorData;
        
        private JobHandle _jobHandle;
        
        private static readonly ProfilerMarker ScheduleJobPM = new("FieldViewQuerySystem.ScheduleJob");
        
        private static readonly ProfilerMarker CompleteJobPM = new("FieldViewQuerySystem.CompleteJob");
        
        protected override void Initialize()
        {
            Assert.IsFalse(FramePerTick <= 3);
            _commands = new NativeList<FieldViewQueryCommand>(100, Allocator.Persistent);
            Scheduler.WaitFrame(ref _updateTickHandle, FramePerTick, ScheduleJob, TickFrame.FixedUpdate, isLooped: true);
            // Allow job scheduled in 3 frames
            Scheduler.WaitFrame(ref _lateUpdateTickHandle, 3, CompleteJob, TickFrame.FixedUpdate, isLooped: true);
            _lateUpdateTickHandle.Pause();
        }
        private void ScheduleJob(int _)
        {
            using (ScheduleJobPM.Auto())
            {

                if (_commands.Length == 0) return;

                _actorData = GetOrCreate<ActorQuerySystem>().GetAllActors(Allocator.TempJob);
                _results = new NativeParallelMultiHashMap<int, ActorHandle>(1024, Allocator.Persistent);
                _execution = _commands.ToArray(Allocator.TempJob);
                _jobHandle = new OverlapFieldViewBatchJob()
                {
                    actors = _actorData,
                    datas = _execution,
                    resultActors = _results
                }.Schedule(_execution.Length, 32);
                _lateUpdateTickHandle.Resume();
            }
        }
        private void CompleteJob(int _)
        {
            using (CompleteJobPM.Auto())
            {
                _jobHandle.Complete();
                _cache.DisposeSafe();
                _cache = _results;
                _actorData.Dispose();
                _execution.Dispose();
                _lateUpdateTickHandle.Pause();
            }
        }
        protected override void Release()
        {
            _jobHandle.Complete();
            _commands.Dispose();
            _execution.DisposeSafe();
            _cache.DisposeSafe();
            _results.DisposeSafe();
            _actorData.DisposeSafe();
            _lateUpdateTickHandle.Dispose();
            _updateTickHandle.Dispose();
        }
        public void EnqueueCommand(FieldViewQueryCommand command)
        {
            if (_handleIndices.TryGetValue(command.self, out var index))
            {
                _commands[index] = command;
            }
            else
            {
                int length = _commands.Length;
                _handleIndices[command.self] = length;
                _commands.Add(command);
            }
        }
        public void GetActorsInFieldView(ActorHandle handle, List<Actor> actors)
        {
            if (!_handleIndices.TryGetValue(handle, out var index))
            {
                Debug.LogWarning($"[FieldViewQuerySystem] Actor {handle.Handle}'s field view has not been initialized");
                return;
            }
            if (!_cache.IsCreated) return;

            var world = GetWorld();
            foreach (var id in _cache.GetValuesForKey(index))
            {
                actors.Add(world.GetActor(id));
            }
        }
    }
}
