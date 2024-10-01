using System;
using UnityEngine;
namespace Kurisu.Framework.Serialization
{
    /// <summary>
    /// Serialized object that will serialize metadata and fields of object implementing T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class SerializedObject<T>
    {
        /// <summary>
        /// Formatted type metadata, see <see cref="SerializedType"/>
        /// </summary>
        public string serializedTypeString;
        /// <summary>
        /// Serialized object data
        /// </summary>
        public string jsonData;
#pragma warning disable CS8632
        private T? value;
#pragma warning restore CS8632 
#if UNITY_EDITOR
        /// <summary>
        /// Editor wrapper, used in SerializedObjectDrawer
        /// </summary>
        [SerializeField]
        ulong objectHandle;
#endif
        /// <summary>
        /// Get default object from <see cref="SerializedObject{T}"/>
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            if (value == null)
            {
                var type = SerializedType.FromString(serializedTypeString);
                if (type == null)
                {
                    Debug.LogWarning($"Missing type {type} when deserialize {nameof(T)}");
                    return default;
                }
                value = (T)JsonUtility.FromJson(jsonData, type);
            }
            return value;
        }
        /// <summary>
        /// Create <see cref="SerializedObject{T}"/> from object
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static SerializedObject<T> FromObject(object @object)
        {
            return new SerializedObject<T>()
            {
                serializedTypeString = SerializedType.ToString(@object.GetType()),
                jsonData = JsonUtility.ToJson(@object)
            };
        }
    }
}
