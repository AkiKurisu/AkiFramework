using Unity.Collections;
using Unity.Mathematics;
namespace Kurisu.Framework
{
    /// <summary>
    /// Actor world basic data
    /// </summary>
    public struct ActorData
    {
        public int instanceId;
        public int layer;
        public byte active;
        public float3 position;
    }
    /// <summary>
    /// Represent a DOP actor query system.
    /// </summary>
    public class ActorQuerySystem : WorldSubsystem
    {
        private NativeArray<ActorData> _actorData = default;
        public override void FixedTick()
        {
            var actorsInWorld = GetActorsInWorld();
            if (IsActorsDirty)
            {
                _actorData.Resize(actorsInWorld.Length);
                IsActorsDirty = false;
            }
            for (int i = 0; i < _actorData.Length; ++i)
            {
                _actorData[i] = new ActorData()
                {
                    instanceId = actorsInWorld[i].GetInstanceID(),
                    active = (byte)(actorsInWorld[i].isActiveAndEnabled ? 0 : 1),
                    layer = actorsInWorld[i].gameObject.layer,
                    position = actorsInWorld[i].transform.position
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
        public override void Dispose()
        {
            _actorData.DisposeSafe();
        }
    }
}
