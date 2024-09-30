using System;
using UnityEngine;
namespace Kurisu.Framework.Serialization.Editor
{
    [Serializable]
    public class GenericSerializedObjectWrapper<T> : SerializedObjectWrapper
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
    /// Api for editor serialization
    /// </summary>
    public static class SerializationEditorManager
    {
        public static SerializedObjectWrapper CreateWrapper(Type type, ref SoftObjectHandle softObjectHandle)
        {
            if (type == null) return null;

            var wrapper = GlobalObjectManager.GetObject(softObjectHandle) as SerializedObjectWrapper;
            if (!wrapper)
            {
                wrapper = Wrap(Activator.CreateInstance(type));
                GlobalObjectManager.UnregisterObject(softObjectHandle);
                GlobalObjectManager.RegisterObject(wrapper, ref softObjectHandle);
            }
            return wrapper;
        }
        public static void DestroyWrapper(SoftObjectHandle softObjectHandle)
        {
            GlobalObjectManager.UnregisterObject(softObjectHandle);
        }
        public static SerializedObjectWrapper GetWrapper(SoftObjectHandle softObjectHandle)
        {
            return GlobalObjectManager.GetObject(softObjectHandle) as SerializedObjectWrapper;
        }
        private static SerializedObjectWrapper Wrap(object value = null)
        {
            Type type = value.GetType();
            Type genericType = typeof(GenericSerializedObjectWrapper<>).MakeGenericType(type);
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
}
