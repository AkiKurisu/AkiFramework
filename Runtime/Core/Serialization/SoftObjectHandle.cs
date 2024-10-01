using UnityEngine.Assertions;
namespace Kurisu.Framework.Serialization
{
    public readonly struct SoftObjectHandle
    {
        public readonly ulong Handle { get; }
        public const int IndexBits = 24;
        public const int SerialNumberBits = 40;
        public const int MaxIndex = 1 << IndexBits;
        public const ulong MaxSerialNumber = (ulong)1 << SerialNumberBits;
        public readonly int GetIndex() => (int)(Handle & MaxIndex - 1);
        public readonly ulong GetSerialNumber() => Handle >> IndexBits;
        public readonly bool IsValid()
        {
            return Handle != 0;
        }
        public SoftObjectHandle(ulong handle)
        {
            Handle = handle;
        }
        public SoftObjectHandle(ulong serialNum, int index)
        {
            Assert.IsTrue(index >= 0 && index < MaxIndex);
            Assert.IsTrue(serialNum < MaxSerialNumber);
#pragma warning disable CS0675
            Handle = (serialNum << IndexBits) | (ulong)index;
#pragma warning restore CS0675
        }

        public static bool operator ==(SoftObjectHandle left, SoftObjectHandle right)
        {
            return left.Handle == right.Handle;
        }
        public static bool operator !=(SoftObjectHandle left, SoftObjectHandle right)
        {
            return left.Handle != right.Handle;
        }
        public override bool Equals(object obj)
        {
            if (obj is not SoftObjectHandle handle) return false;
            return handle.Handle == Handle;
        }
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
    }
}