using System;
using System.Reflection;
using Ceres.Annotations;
using Kurisu.Framework.Serialization;
using Kurisu.Framework.Serialization.Editor;
using UnityEditor;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    [Ordered]
    public class WrapFieldResolver<T> : FieldResolver<WrapField<T>, T>
    {
        public WrapFieldResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override WrapField<T> CreateEditorField(FieldInfo fieldInfo)
        {
            return new WrapField<T>(fieldInfo.Name, fieldInfo);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo fieldInfo)
        {
            return infoType == typeof(T) && fieldInfo.GetCustomAttribute<WrapFieldAttribute>() != null;
        }
    }
    public class WrapField<T> : BaseField<T>
    {
        private SerializedObjectWrapper<T> m_Instance;
        
        private SerializedObjectWrapper<T> Instance => m_Instance != null ? m_Instance : GetInstance();
        
        private SerializedObject m_SerializedObject;
        
        private SerializedProperty m_SerializedProperty;
        
        private SoftObjectHandle wrapperHandle;

        private readonly FieldInfo fieldInfo;
        public WrapField(string label, FieldInfo fieldInfo) : base(label, null)
        {
            this.fieldInfo = fieldInfo;
            var element = CreateView();
            Add(element);
        }
        private SerializedObjectWrapper<T> GetInstance()
        {
            m_Instance = (SerializedObjectWrapper<T>)SerializedObjectWrapperManager.CreateFieldWrapper(fieldInfo, ref wrapperHandle);
            m_Instance.Value = value;
            m_SerializedObject = new SerializedObject(m_Instance);
            m_SerializedProperty = m_SerializedObject.FindProperty("m_Value");
            return m_Instance;
        }

        private VisualElement CreateView()
        {
            return new IMGUIContainer(() =>
            {
                m_SerializedObject.Update();
                EditorGUILayout.PropertyField(m_SerializedProperty);
                if (m_SerializedObject.ApplyModifiedProperties())
                {
                    base.value = (T)Instance.Value;
                }
            });
        }
        public sealed override T value
        {
            get => base.value;
            set
            {
                if (value == null)
                {
                    Instance.Value = (T)Activator.CreateInstance(typeof(T));
                }
                else
                {
                    Instance.Value = ReflectionHelper.DeepCopy(value);
                }
                base.value = (T)Instance.Value;
            }
        }
    }
}
