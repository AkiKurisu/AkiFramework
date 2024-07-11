using System;
using UnityEngine;
namespace Kurisu.Framework.AI
{
    public class TaskIDAttribute : PropertyAttribute { }
    /// <summary>
    /// Create bridge between popUpSet and custom task id
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TaskIDHostAttribute : Attribute { }
}
