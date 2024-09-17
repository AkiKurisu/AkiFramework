using System;
using UnityEngine;
namespace Kurisu.Framework.Editor
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
    internal class SerializedObjectEditorUtils
    {
        internal static SerializedObjectWrapper Wrap(object value = null)
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
