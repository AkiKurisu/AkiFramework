using System;
using Chris.Gameplay;
using Unity.Mathematics;
using UnityEngine;
namespace Chris.AI.EQS
{
    /// <summary>
    /// Post Query data provider associated with an Actor as component
    /// </summary>
    public class PostQueryComponent : ActorComponent
    {
        [Header("Data")]
        public PostQueryParameters postQuery = new()
        {
            Angle = 120,
            Distance = 30,
            Step = 6,
            Depth = 3
        };
        
        public LayerMask raycastLayerMask;
        
        public Vector3 raycastOffset
            ;
        private PostQuerySystem _system;
        
        private void Start()
        {
            _system = WorldSubsystem.GetOrCreate<PostQuerySystem>();
            if (_system == null)
            {
                Debug.LogError($"[PostQueryComponent] Can not get PostQuerySystem dynamically.");
            }
        }
        
        /// <summary>
        /// Request a new post query from target's view
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool RequestPostQuery(Actor target)
        {
            if (_system == null)
            {
                return false;
            }
            if (target == GetActor())
            {
                Debug.LogWarning($"[PostQueryComponent] Can not request post query from self view.");
                return false;
            }
            if (!_system.IsFree(GetActor().GetActorHandle()))
            {
                return false;
            }
            PostQueryCommand command = new()
            {
                Self = GetActor().GetActorHandle(),
                Target = target.GetActorHandle(),
                Parameters = postQuery,
                Offset = raycastOffset,
                LayerMask = raycastLayerMask
            };
            _system.EnqueueCommand(command);
            return true;
        }
        
        /// <summary>
        /// Get current posts
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<float3> GetPosts()
        {
            return _system.GetPosts(GetActor().GetActorHandle()).AsReadOnlySpan();
        }
    }
}
