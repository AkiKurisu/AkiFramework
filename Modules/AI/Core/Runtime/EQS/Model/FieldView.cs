using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.Framework.AI.EQS
{
    /// <summary>
    /// Represents a field of view for AI. 
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
        /// <summary>
        /// Detect whether can see the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fromPosition"></param>
        /// <param name="viewDirection"></param>
        /// <param name="layerMask"></param>
        /// <param name="filterTags"></param>
        /// <returns></returns>
        public readonly bool Detect(Vector3 target, Vector3 fromPosition, Quaternion fromRotation, LayerMask layerMask, string[] filterTags = null)
        {
            bool isVisible = true;
            Vector3 viewDirection = fromRotation * Vector3.forward;
            Vector3 directionToTarget = (target - fromPosition).normalized;

            if (Vector3.Angle(viewDirection, directionToTarget) > Angle / 2)
            {
                isVisible = false;
            }
            else
            {
                // Raycast detect, ignore height
                float normalDistance = Vector3.Distance(new Vector3(fromPosition.x, 0, fromPosition.z), new Vector3(target.x, 0, target.z));
                if (normalDistance > Radius)
                {
                    return false;
                }
                Physics.Linecast(fromPosition, target, out RaycastHit hit, layerMask);
                if (hit.collider != null)
                {
                    if (Utils.CompareTags(hit.collider, filterTags) == false)
                    {
                        Debug.DrawLine(hit.point, fromPosition, Color.cyan);
                        isVisible = false;
                    }
                    else
                    {
                        isVisible = true;
                    }
                }
            }
            return isVisible;
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