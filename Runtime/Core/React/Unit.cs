using System;
namespace Kurisu.Framework.React
{
    [Serializable]
    public readonly struct Unit : IEquatable<Unit>
    {
        static readonly Unit @default = new();

        public static Unit Default { get { return @default; } }

        public static bool operator ==(Unit _, Unit __)
        {
            return true;
        }

        public static bool operator !=(Unit _, Unit __)
        {
            return false;
        }

        public bool Equals(Unit other)
        {
            return true;
        }
        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}