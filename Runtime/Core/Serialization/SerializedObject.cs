using System;
using UnityEngine;
namespace Kurisu.Framework.Serialization
{
    public class SerializedObjectBase
    {
        /// <summary>
        /// Formatted type metadata, see <see cref="SerializedType"/>
        /// </summary>
        public string serializedTypeString;
        /// <summary>
        /// Serialized object data
        /// </summary>
        public string jsonData;
#if UNITY_EDITOR
        /// <summary>
        /// Editor wrapper, used in SerializedObjectDrawer
        /// </summary>
        [SerializeField]
        internal ulong objectHandle;
#endif
    }
    /// <summary>
    /// Serialized object that will serialize metadata and fields of object implementing T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class SerializedObject<T> : SerializedObjectBase
    {
#pragma warning disable CS8632
        private T? value;
#pragma warning restore CS8632 
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
                    Debug.LogWarning($"Missing type {serializedTypeString} when deserialize {nameof(T)}");
                    return default;
                }
                value = (T)JsonUtility.FromJson(jsonData, type);
            }
            return value;
        }
        /// <summary>
        /// Instantiate new object from <see cref="SerializedObject{T}"/>
        /// </summary>
        /// <returns></returns>
        public T NewObject()
        {
            var type = SerializedType.FromString(serializedTypeString);
            if (type == null)
            {
                Debug.LogWarning($"Missing type {serializedTypeString} when deserialize {nameof(T)}");
                return default;
            }
            return (T)JsonUtility.FromJson(jsonData, type);
        }
        /// <summary>
        /// Get object type from <see cref="SerializedObject{T}"/>
        /// </summary>
        /// <returns></returns>
        public Type GetObjectType()
        {
            if (value != null)
            {
                return value.GetType();
            }
            return SerializedType.FromString(serializedTypeString);
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
        internal void InternalUpdate()
        {
            if (value != null && SerializedType.ToString(value.GetType()) != serializedTypeString)
            {
                value = default;
            }
        }
    }
}
