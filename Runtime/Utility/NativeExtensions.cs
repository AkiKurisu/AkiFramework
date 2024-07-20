using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
namespace Kurisu.Framework
{
    public static class NativeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeSafe<T>(ref this NativeArray<T> array) where T : unmanaged
        {
            if (array.IsCreated)
                array.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeSafe<T>(ref this NativeList<T> array) where T : unmanaged
        {
            if (array.IsCreated)
                array.Dispose();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Resize<T>(ref this NativeArray<T> array, int size, Allocator allocator = Allocator.Persistent, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged
        {
            if (array.IsCreated == false || array.Length < size)
            {
                array.DisposeSafe();
                array = new NativeArray<T>(size, allocator, options);
            }
        }
        // thanks to https://discussions.unity.com/t/rotate-towards-c-jobs/778453/5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(this quaternion from, quaternion to, float maxDegreesDelta)
        {
            float num = Angle(from, to);
            return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this quaternion q1, quaternion q2)
        {
            var dot = math.dot(q1, q2);
            return !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }
    }
}