using UnityEngine;
namespace Kurisu.Framework.Serialization
{
    /// <summary>
    /// Serialized object wrapper for custom object.
    /// Should set public since emit code need access to constructor.
    /// </summary>
    public abstract class SerializedObjectWrapper : ScriptableObject
    {
        public abstract object Value
        {
            get;
            set;
        }
    }
}