using System;
using Unity.Collections;
using UnityEngine;
namespace Kurisu.Framework.AI.EQS
{
    /// <summary>
    /// Represents an advanced field of view for AI mentioned in "Naughty Dog: Human Enemy AI In Last of The Us". 
    /// </summary>
    [Serializable]
    public struct FieldViewPrime
    {
        [Range(0, 500), Tooltip("Field of view radius, ai can only sensor new target within this radius in far distance")]
        public float Radius;
        [Range(0, 360), Tooltip("Field of view angle, ai can only see target within angle in far distance")]
        public float Angle;
        [Range(3, 20), Tooltip("Field of view frustum sides, ai can only see target within frustum in close distance")]
        public int Sides;
        [Range(0.1f, 1f), Tooltip("Field of view blend weight")]
        public float Blend;
        public readonly float PolygonRadius => Radius * Blend * 0.5f;
        public FieldViewPrime(float radius, float angle, int sides, float blend)
        {
            Radius = radius;
            Angle = angle;
            Sides = sides;
            Blend = blend;
        }
        /// <summary>
        /// Detect whether can see the target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fromPosition"></param>
        /// <param name="fromRotation"></param>
        /// <param name="layerMask"></param>
        /// <param name="filterTags"></param>
        /// <returns></returns>
        public readonly bool Detect(Vector3 target, Vector3 fromPosition, Quaternion fromRotation, LayerMask layerMask, string[] filterTags = null)
        {
            // Primary detect
            bool isVisible = true;
            Vector3 forward = fromRotation * Vector3.forward;
            Vector3 directionToTarget = (target - fromPosition).normalized;
            float radius = PolygonRadius;
            float centerDistance = Vector3.Distance(fromPosition + forward * radius, target);
            if (centerDistance < radius)
            {
                if (!IsPointInPolygon(fromPosition, fromRotation, target))
                {
                    // When target is nearly on edge, detect whether target is in fov now
                    const float threshold = 0.9f;
                    if (centerDistance >= threshold * radius && Vector3.Angle(forward, directionToTarget) <= Angle / 2)
                    {
                        goto raycast;
                    }
                    return false;
                }
            }
            else
            {
                if (Vector3.Angle(forward, directionToTarget) > Angle / 2)
                {
                    return false;
                }
            }
        raycast:
            // Raycast detect, ignore height
            float normalDistance = Vector3.Distance(new Vector3(fromPosition.x, 0, fromPosition.z), new Vector3(target.x, 0, target.z));
            if (normalDistance > Radius)
            {
                return false;
            }
            Physics.Linecast(fromPosition, target, out RaycastHit hit, layerMask);
            if (hit.collider != null)
            {
                if (FrameworkUtils.CompareTags(hit.collider, filterTags) == false)
                {
                    Debug.DrawLine(hit.point, fromPosition, Color.cyan);
                    isVisible = false;
                }
                else
                {
                    isVisible = true;
                }
            }
            return isVisible;
        }
        public readonly NativeArray<Vector3> AllocatePolygonCorners(Vector3 position, Quaternion rotation, Allocator allocator)
        {
            Vector3 forward = rotation * Vector3.forward;
            float radius = PolygonRadius;
            var frustumCorners = new NativeArray<Vector3>(Sides, allocator);
            float angleStep = 360f / Sides;

            for (int i = 0; i < Sides; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                frustumCorners[i] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            }
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = position + forward * radius + rotation * frustumCorners[i];
            }
            return frustumCorners;
        }
        public readonly bool IsPointInPolygon(Vector3 position, Quaternion rotation, Vector3 p)
        {
            var polygonCorners = AllocatePolygonCorners(position, rotation, Allocator.Temp);
            var j = polygonCorners.Length - 1;
            var inside = false;
            for (int i = 0; i < polygonCorners.Length; j = i++)
            {
                var pi = polygonCorners[i];
                var pj = polygonCorners[j];
                if (((pi.z <= p.z && p.z < pj.z) || (pj.z <= p.z && p.z < pi.z)) &&
                    (p.x < (pj.x - pi.x) * (p.z - pi.z) / (pj.z - pi.z) + pi.x))
                    inside = !inside;
            }
            polygonCorners.Dispose();
            return inside;
        }
        public readonly void DrawGizmos(Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            // Draw fov
            Vector3 forward = rotation * Vector3.forward;
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, Vector3.up, Radius);

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireArc(position, Vector3.up, forward, Angle / 2, Radius - 0.1f);
            UnityEditor.Handles.DrawWireArc(position, Vector3.up, forward, -Angle / 2, Radius - 0.1f);

            UnityEditor.Handles.color = new Color(1, 0, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(position, Vector3.up, forward, Angle / 2, Radius - 0.2f);
            UnityEditor.Handles.DrawSolidArc(position, Vector3.up, forward, -Angle / 2, Radius - 0.2f);

            // Draw polygon
            var frustumCorners = AllocatePolygonCorners(position, rotation, Allocator.Temp);
            Gizmos.color = Color.red;
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                int nextIndex = (i + 1) % frustumCorners.Length;
                Gizmos.DrawLine(frustumCorners[i], frustumCorners[nextIndex]);
            }
            frustumCorners.Dispose();
#endif
        }
    }
}