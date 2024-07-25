using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Kurisu.Framework
{
    /// <summary>
    /// Utils for Mathematics
    /// </summary>
    public static class MathUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyVector(ref this float4x4 worldMatrix, in float3 point, ref float3 result)
        {
            result = math.mul(worldMatrix, new float4(point, 0.0f)).xyz;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyPoint3x4(ref this float4x4 worldMatrix, in float3 point, ref float3 result)
        {
            result = math.mul(worldMatrix, new float4(point, 1.0f)).xyz;
        }
        // thanks to https://discussions.unity.com/t/rotate-towards-c-jobs/778453/5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(in quaternion from, in quaternion to, in float maxDegreesDelta)
        {
            float num = Angle(from, to);
            return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in quaternion q1, in quaternion q2)
        {
            var dot = math.dot(q1, q2);
            return !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }
    }
}
