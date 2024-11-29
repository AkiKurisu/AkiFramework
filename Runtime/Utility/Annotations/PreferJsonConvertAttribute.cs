using System;
namespace Chris
{
    /// <summary>
    /// Prefer to use <see cref="JsonConvert"/> instead of <see cref="JsonUtility"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PreferJsonConvertAttribute : Attribute { }
}