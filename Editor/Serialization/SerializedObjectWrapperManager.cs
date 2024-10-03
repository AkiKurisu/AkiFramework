using System;
using UnityEngine;
namespace Kurisu.Framework.Serialization.Editor
{
    /// <summary>
    /// Class to manage SerializedObjectWrapper
    /// </summary>
    public static class SerializedObjectWrapperManager
    {
        [Serializable]
        public class SerializedObjectWrapper<T> : SerializedObjectWrapper
        {
            [SerializeField]
            T m_Value;

            public override object Value
            {
                get { return m_Value; }
                set { m_Value = (T)value; }
            }
        }
        /// <summary>
        /// Create an editor wrapper for providing type and track it by <see cref="SoftObjectHandle"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="softObjectHandle"></param>
        /// <returns></returns>
        public static SerializedObjectWrapper CreateWrapper(Type type, ref SoftObjectHandle softObjectHandle)
        {
            if (type == null) return null;

            var wrapper = softObjectHandle.GetObject() as SerializedObjectWrapper;
            // Validate wrapped type
            if (!wrapper || wrapper.Value.GetType() != type)
            {
                wrapper = Wrap(Activator.CreateInstance(type));
                GlobalObjectManager.UnregisterObject(softObjectHandle);
                GlobalObjectManager.RegisterObject(wrapper, ref softObjectHandle);
            }
            return wrapper;
        }
        /// <summary>
        /// Manually destroy wrapper
        /// </summary>
        /// <param name="softObjectHandle"></param>
        public static void DestroyWrapper(SoftObjectHandle softObjectHandle)
        {
            GlobalObjectManager.UnregisterObject(softObjectHandle);
        }
        /// <summary>
        /// Get editor wrapper if exists
        /// </summary>
        /// <param name="type"></param>
        /// <param name="softObjectHandle"></param>
        /// <returns></returns>
        public static SerializedObjectWrapper GetWrapper(Type type, SoftObjectHandle softObjectHandle)
        {
            var wrapper = softObjectHandle.GetObject() as SerializedObjectWrapper;
            // Validate wrapped type
            if (wrapper && wrapper.Value.GetType() != type)
            {
                GlobalObjectManager.UnregisterObject(softObjectHandle);
                return null;
            }
            return wrapper;
        }
        private static SerializedObjectWrapper Wrap(object value = null)
        {
            Type type = value.GetType();
            Type genericType = typeof(SerializedObjectWrapper<>).MakeGenericType(type);
            Type dynamicType = DynamicTypeBuilder.MakeDerivedType(genericType, type);
            var dynamicTypeInstance = ScriptableObject.CreateInstance(dynamicType);
            if (dynamicTypeInstance is not SerializedObjectWrapper wrapper)
            {
                return null;
            }
            wrapper.Value = value ?? default;
            return (SerializedObjectWrapper)dynamicTypeInstance;
        }
    }
    public static class SerializedObjectEditorUtils
    {
        /// <summary>
        /// Cleanup a serializedObjectBase object reference
        /// </summary>
        /// <param name="serializedObjectBase"></param>
        public static void Cleanup(SerializedObjectBase serializedObjectBase)
        {
            serializedObjectBase.objectHandle = 0;
        }
    }
}
