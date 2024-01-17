using UnityEngine;
namespace Kurisu.Framework
{
    public static class AkiMethod
    {
        public static Vector3 GetScreenPosition(float width, float height, Vector3 target)
        {
            return GetScreenPosition(Camera.main, width, height, target);
        }
        public static Vector3 GetScreenPosition(Camera camera, float width, float height, Vector3 target)
        {
            Vector3 pos = camera.WorldToScreenPoint(target);
            pos.x *= width / Screen.width;
            pos.y *= height / Screen.height;
            pos.x -= width * 0.5f;
            pos.y -= height * 0.5f;
            return pos;
        }
        /// <summary>
        ///Quadratic Bezier curve, dynamically draw a curve based on three points
        /// </summary>
        /// <param name="t">The arrival coefficient is 0 for the start and 1 for the arrival</param>
        /// <param name="p0">Starting point</param>
        /// <param name="p1">Middle point</param>
        /// <param name="p2">End point</param>
        /// <returns></returns>
        public static Vector2 GetQuadraticCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            return (uu * p0) + (2 * u * t * p1) + (tt * p2);
        }
    }
}
