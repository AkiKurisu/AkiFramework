using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
namespace Kurisu.Framework
{
    /// <summary>
    /// Actor world basic data
    /// </summary>
    public struct ActorData
    {
        public ActorHandle handle;
        public int layer;
        public byte active;
        public quaternion rotation;
        public float3 position;
    }
    /// <summary>
    /// Represent a DOP actor query system.
    /// </summary>
    public class ActorQuerySystem : WorldSubsystem
    {
        private NativeArray<ActorData> _actorData = default;
        private readonly List<Actor> _actors = new();
        private static readonly ProfilerMarker TickPM = new("ActorQuerySystem.Tick");
        protected override void Initialize()
        {
            RebuildArray();
        }
        public override void Tick()
        {
            using (TickPM.Auto())
            {
                if (IsActorsDirty)
                {
                    RebuildArray();
                    IsActorsDirty = false;
                }
                unsafe
                {
                    void* arrayPtr = _actorData.GetUnsafePtr();
                    for (int i = 0; i < _actorData.Length; ++i)
                    {
                        _actors[i].transform.GetPositionAndRotation(out var pos, out var rot);
                        fixed (ActorData* ptr = &UnsafeUtility.ArrayElementAsRef<ActorData>(arrayPtr, i))
                        {
                            ptr->position = pos;
                            ptr->rotation = rot;
                            ptr->active = (byte)(_actors[i].isActiveAndEnabled ? 0 : 1);
                        }
                    }
                }
            }
        }
        private void RebuildArray()
        {
            _actors.Clear();
            GetActorsInWorld(_actors);
            _actorData.Resize(_actors.Count);
            for (int i = 0; i < _actorData.Length; ++i)
            {
                _actorData[i] = new ActorData()
                {
                    handle = _actors[i].GetActorHandle(),
                    // not update layer in tick to prevent allocation
                    // TODO: Move out of actor data, or use custom layer mask
                    layer = _actors[i].gameObject.layer
                };
            }
        }
        /// <summary>
        /// Allocate an actor data array for query
        /// </summary>
        /// <param name="allocator"></param>
        /// <returns></returns>
        public NativeArray<ActorData> GetAllActors(Allocator allocator)
        {
            if (!_actorData.IsCreated)
            {
                return new NativeArray<ActorData>(0, allocator);
            }
            return new NativeArray<ActorData>(_actorData, allocator);
        }
        protected override void Release()
        {
            _actorData.DisposeSafe();
        }
    }
}
