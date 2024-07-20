using System;
using Unity.Mathematics;
using UnityEngine;
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
    public class PostQueryComponent : ActorComponent
    {
        public ReadOnlySpan<float3> GetPosts()
        {
            return system.GetPosts(GetActor().GetActorId());
        }
        public PostQuery PostQuery = new()
        {
            Angle = 120,
            Distance = 30,
            Step = 6
        };
        public LayerMask RaycastLayerMask;
        public Vector3 RaycastOffset;
        private PostQuerySystem system;
        private void Start()
        {
            system = DynamicSubsystem.Get<PostQuerySystem>(ActorWorld.Current);
            if (system == null)
            {
                Debug.LogError($"[PostQueryComponent] Can not get PostQuerySystem dynamically.");
            }
        }
        public bool RequestQueryPost(Actor target)
        {
            if (system == null)
            {
                return false;
            }
            if (!system.IsComplete(GetActor().GetActorId()))
            {
                return false;
            }
            var command = new PostQueryCommand()
            {
                selfId = GetActor().GetActorId(),
                targetId = target.GetActorId(),
                postQuery = PostQuery,
                offset = RaycastOffset,
                layerMask = RaycastLayerMask
            };
            system.EnqueueCommand(command);
            return true;
        }
    }
}
