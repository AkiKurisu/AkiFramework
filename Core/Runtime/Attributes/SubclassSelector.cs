using System;
using UnityEngine;
namespace Kurisu.Framework
{
    /// <summary>
    /// Attribute to specify the type of the field serialized by the SerializeReference attribute in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SubclassSelector : PropertyAttribute
    {

    }
}