using System;
namespace Kurisu.Framework
{
    /// <summary>
    /// Set framework stack trace skip frame from this method or constructor
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class StackTraceFrameAttribute : Attribute
    {
        public StackTraceFrameAttribute() { }
    }
}