using System;
namespace Chris.Gameplay
{
    /// <summary>
    /// Whether <see cref="WorldSubsystem"/> should be created and initialize when world create
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class InitializeOnWorldCreateAttribute : Attribute
    {

    }
}
