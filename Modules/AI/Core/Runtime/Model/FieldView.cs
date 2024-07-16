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
    public struct FieldView
    {
        [Range(0, 500), Tooltip("Field of view radius, ai can only sensor new target within this radius")]
        public float Radius;
        [Range(0, 360), Tooltip("Field of view angle, ai can only see target within angle")]
        public float Angle;
        public FieldView(float radius, float angle)
        {
            Radius = radius;
            Angle = angle;
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
        // TODO: Need optimize
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