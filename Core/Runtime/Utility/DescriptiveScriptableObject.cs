using UnityEngine;
namespace Kurisu.Framework.Utility.ManagedObjects
{
    /// <summary>
    /// Base class for all ScriptableObjects with a developer-facing description.
    /// </summary>
    public class DescriptiveScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        [TextArea(0, 3)]
        [Tooltip("Where this SO is used, in which scenes, who initializes it and who reads from it.")]
        public string _devDescription = "";
#endif
    }
}