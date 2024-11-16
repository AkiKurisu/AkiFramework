using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Kurisu.Framework.Serialization.Editor
{
    [Serializable]
    public class SerializedObjectWrapper<T> : SerializedObjectWrapper
    {
        [SerializeField] 
        private T m_Value;

        public override object Value
        {
            get { return m_Value; }
            set { m_Value = (T)value; }
        }
    }
    /// <summary>
    /// Class to manage SerializedObjectWrapper
    /// </summary>
    public static class SerializedObjectWrapperManager
    {
        /// <summary>
        /// Create an editor wrapper for providing <see cref="Type"/> and track it by <see cref="SoftObjectHandle"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="softObjectHandle"></param>
        /// <returns></returns>
        public static SerializedObjectWrapper CreateWrapper(Type type, ref SoftObjectHandle softObjectHandle)
        {
            if (type == null) return null;

            var wrapper = softObjectHandle.GetObject() as SerializedObjectWrapper;
            // Validate wrapped type
            if (!wrapper || wrapper.Value.GetType() != type || wrapper.FieldInfo != null)
            {
                wrapper = Wrap(Activator.CreateInstance(type));
                GlobalObjectManager.UnregisterObject(softObjectHandle);
                GlobalObjectManager.RegisterObject(wrapper, ref softObjectHandle);
            }
            return wrapper;
        }
        /// <summary>
        /// Create an editor wrapper for providing <see cref="FieldInfo"/> and track it by <see cref="SoftObjectHandle"/>
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="softObjectHandle"></param>
        /// <returns></returns>
        public static SerializedObjectWrapper CreateFieldWrapper(FieldInfo fieldInfo, ref SoftObjectHandle softObjectHandle)
        {
            if (fieldInfo == null) return null;

            var wrapper = softObjectHandle.GetObject() as SerializedObjectWrapper;
            // Validate wrapped type
            if (!wrapper || wrapper.Value.GetType() != fieldInfo.FieldType || wrapper.FieldInfo != fieldInfo)
            {
                wrapper = Wrap(Activator.CreateInstance(fieldInfo.FieldType), fieldInfo);
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
        private static SerializedObjectWrapper Wrap(object value, FieldInfo fieldInfo = null)
        {
            Type type = value.GetType();
            Type genericType = typeof(SerializedObjectWrapper<>).MakeGenericType(type);
            Type dynamicType = DynamicTypeBuilder.MakeDerivedType(genericType, type);
            if (fieldInfo != null)
            {
                var valueFieldInfo = genericType.GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
                var attributes = fieldInfo.GetCustomAttributes().ToList();
                attributes.RemoveAll(x => x is SerializeField);
                TypeDescriptor.AddAttributes(valueFieldInfo! ,attributes.ToArray());
            }
            var dynamicTypeInstance = ScriptableObject.CreateInstance(dynamicType);
            if (dynamicTypeInstance is not SerializedObjectWrapper wrapper)
            {
                return null;
            }
            wrapper.Value = value;
            return wrapper;
        }
    }
}
