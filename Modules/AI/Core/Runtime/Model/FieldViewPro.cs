using System;
using System.Collections.Generic;
using Kurisu.Framework.EQS;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.Framework.AI
{
    /// <summary>
    /// Represents an advanced field of view for AI. 
    /// Reference article: 《Naughty Dog-Human Enemy AI In Last of The Us》.
    /// </summary>
    [Serializable]
    public struct FieldViewPro
    {
        [Header("Fov")]
        [Range(0, 500), Tooltip("Field of view radius, ai can only sensor new target within this radius")]
        public float Radius;
        [Range(0, 360), Tooltip("Field of view angle, ai can only see target within angle")]
        public float Angle;
        [Header("Posts")]
        [Range(0, 500), Tooltip("Post query max distance")]
        public float Distance;
        [Range(2, 12), Tooltip("Post query iterate step, decrease step to increase performance but loss diversity")]
        public int Step;
        [Range(1, 6), Tooltip("Post query sampling depth, decrease depth to increase performance but loss precision")]
        public int Depth;
        public FieldViewPro(float radius, float angle)
        {
            Radius = radius;
            Angle = angle;
            Distance = 10f;
            Step = 6;
            Depth = 3;
        }
        public FieldViewPro(float radius, float angle, float distance)
        {
            Radius = radius;
            Angle = angle;
            Distance = distance;
            Step = 6;
            Depth = 3;
        }
        /// <summary>
        /// Collect actors in field of view
        /// </summary>
        /// <param name="actors"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="targetMask"></param>
        /// <param name="actorToIgnore"></param>
        public readonly void CollectViewActors(List<Actor> actors, Vector3 position, Vector3 forward, LayerMask targetMask, Actor actorToIgnore = null)
        {
            EnvironmentQuery.OverlapFieldView(actors, position, forward, Radius, Angle, targetMask, actorToIgnore);
        }
        /// <summary>
        /// Collect actors in field of view with generic type filter
        /// </summary>
        /// <param name="actors"></param>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="targetMask"></param>
        /// <param name="actorToIgnore"></param>
        public readonly void CollectViewActors<T>(List<T> actors, Vector3 position, Vector3 forward, LayerMask targetMask, T actorToIgnore = null) where T : Actor
        {
            var list = ListPool<Actor>.Get();
            EnvironmentQuery.OverlapFieldView(list, position, forward, Radius, Angle, targetMask, actorToIgnore);
            foreach (var actor in list)
            {
                if (actor is T tActor) actors.Add(tActor);
            }
            ListPool<Actor>.Release(list);
        }
        /// <summary>
        /// Whether can see the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fromPosition"></param>
        /// <param name="viewDirection"></param>
        /// <param name="layerMask"></param>
        /// <param name="filterTags"></param>
        /// <returns></returns>
        public readonly bool CanVisible(Transform target, Vector3 fromPosition, Vector3 viewDirection, LayerMask layerMask, string[] filterTags = null)
        {
            if (target == null) return false;

            bool CanSeeTarget = true;
            Vector3 directionToTarget = (target.position - fromPosition).normalized;

            if (Vector3.Angle(viewDirection, directionToTarget) > Angle / 2)
            {
                CanSeeTarget = false;
            }
            else
            {

                float normalDistance = Vector3.Distance(fromPosition, target.position);
                Vector3 lineCastEndPosition = fromPosition + directionToTarget * normalDistance;
                Physics.Linecast(fromPosition, lineCastEndPosition, out RaycastHit hit, layerMask);
                if (hit.collider != null)
                {
                    if (TagMatches(hit.collider.tag, filterTags) == false)
                    {
                        Debug.DrawLine(hit.point, fromPosition, Color.cyan);
                        CanSeeTarget = false;
                    }
                    else
                    {
                        CanSeeTarget = true;
                    }
                }
            }
            return CanSeeTarget;
        }
        private static bool TagMatches(string targetTag, string[] allowedTags)
        {
            if (targetTag == null || allowedTags == null) return false;

            bool match = false;
            foreach (string tag in allowedTags)
            {
                if (targetTag == tag) match = true;
            }
            return match;

        }
        private struct RaycastPair
        {
            public bool isHitL;
            public bool isHitR;
            public RaycastHit hitL;
            public RaycastHit hitR;
            public Vector3 left;
            public Vector3 right;
            public readonly Vector3 Half => (left + right) / 2;
        }
        /// <summary>
        /// Query posts in source view
        /// </summary>
        /// <param name="posts">Store results</param>
        /// <param name="source">View source transform</param>
        /// <param name="direction">View direction</param>
        /// <param name="layerMask"></param>
        /// <returns>Has post</returns>
        public readonly bool QueryPosts(List<RaycastHit> posts, Transform source, Vector3 direction, LayerMask layerMask)
        {
            float angleInRadians = Angle * Mathf.Deg2Rad;
            Vector3 left = Vector3.RotateTowards(direction, -source.right, angleInRadians / 2, float.MaxValue);
            Vector3 right = Vector3.RotateTowards(direction, source.right, angleInRadians / 2, float.MaxValue);
            float anglePerStep = angleInRadians / Step;
            for (int i = 0; i < Step - 1; ++i)
            {

                RaycastPair pair = new()
                {
                    left = Vector3.RotateTowards(left, right, i * anglePerStep, float.MaxValue),
                    right = Vector3.RotateTowards(left, right, (i + 1) * anglePerStep, float.MaxValue)
                };
                int d = 0;
                RaycastHit hit = default;
                while (d < Depth)
                {
                    int count = DoubleRaycast(source, layerMask, ref pair, out var newHit);
                    if (count == 1)
                    {
                        d++;
                        hit = newHit;
                        // use the most closest hit
                        if (Depth == d)
                        {
                            posts.Add(hit);
                            break;
                        }
                        continue;
                    }
                    // use last hit if possible
                    if (d != 0)
                        posts.Add(hit);
                    break;
                }
            }
            return posts.Count > 0;
        }
        private readonly int DoubleRaycast(Transform from, LayerMask layer, ref RaycastPair rayPair, out RaycastHit hit)
        {
            if (!rayPair.isHitL)
                rayPair.isHitL = Raycast(from, rayPair.left, layer, out rayPair.hitL);
            if (!rayPair.isHitR)
                rayPair.isHitR = Raycast(from, rayPair.right, layer, out rayPair.hitR);
            hit = default;
            if (rayPair.isHitL && rayPair.isHitR)
            {
                return 2;
            }
            if (rayPair.isHitL && !rayPair.isHitR)
            {
                hit = rayPair.hitL;
                rayPair.left = rayPair.Half;
                rayPair.isHitL = false;
                return 1;
            }
            if (!rayPair.isHitL && rayPair.isHitR)
            {
                hit = rayPair.hitR;
                rayPair.right = rayPair.Half;
                rayPair.isHitR = false;
                return 1;
            }
            return 0;
        }
        private readonly bool Raycast(Transform from, Vector3 direction, LayerMask layer, out RaycastHit hit)
        {
            return Physics.Raycast(from.position, direction, out hit, Distance, layer);
        }
        public readonly void DrawGizmos(Vector3 position, Vector3 forward)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, Vector3.up, Radius);

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireArc(position, Vector3.up, forward, Angle / 2, Radius - 0.1f);
            UnityEditor.Handles.DrawWireArc(position, Vector3.up, forward, -Angle / 2, Radius - 0.1f);

            UnityEditor.Handles.color = new Color(1, 0, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(position, Vector3.up, forward, Angle / 2, Radius - 0.2f);
            UnityEditor.Handles.DrawSolidArc(position, Vector3.up, forward, -Angle / 2, Radius - 0.2f);
#endif
        }
    }
}