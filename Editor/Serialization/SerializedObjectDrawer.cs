using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.Framework.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(SerializedObject<>))]
    public class SerializedObjectDrawer : PropertyDrawer
    {
        private const string NullType = "Null";
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight
            + GenericSerializedObjectWrapperDrawer.CalculatePropertyHeight(property.FindPropertyRelative("container"))
            + EditorGUIUtility.standardVerticalSpacing;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var totalHeight = position.height;
            position.height = EditorGUIUtility.singleLineHeight;

            var reference = property.FindPropertyRelative("serializedTypeString");
            var json = property.FindPropertyRelative("jsonData");
            var container = property.FindPropertyRelative("container");

            var type = SerializedType.FromString(reference.stringValue);
            string id = type != null ? type.Name : NullType;
            if (type != null && container.objectReferenceValue == null)
            {
                container.objectReferenceValue = SerializedObjectEditorUtils.Wrap(Activator.CreateInstance(type));
                property.serializedObject.ApplyModifiedProperties();
            }

            float width = position.width;
            float x = position.x;
            position.width *= 0.25f;
            GUI.Label(position, label);
            position.x += position.width + 10;
            position.width = width - position.width - 10;
            if (EditorGUI.DropdownButton(position, new GUIContent(id), FocusType.Keyboard))
            {
                var provider = ScriptableObject.CreateInstance<TypeSearchWindow>();
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsArray)
                {
                    fieldType = fieldType.GetElementType();
                }
                provider.Initialize(fieldType.GetGenericArguments()[0], (selectType) =>
                {
                    reference.stringValue = selectType != null ? SerializedType.ToString(selectType) : NullType;
                    if (selectType != null)
                    {
                        container.objectReferenceValue = SerializedObjectEditorUtils.Wrap(Activator.CreateInstance(selectType));
                    }
                    else
                    {
                        container.objectReferenceValue = null;
                    }
                    property.serializedObject.ApplyModifiedProperties();
                });
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }
            if (container.objectReferenceValue != null)
            {
                JsonConvert.PopulateObject(json.stringValue, container.objectReferenceValue);
            }

            position.x = x;
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = totalHeight - position.height - EditorGUIUtility.standardVerticalSpacing;
            position.width = width;
            GUI.Box(position, "", "Box");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, container, true);
            if (EditorGUI.EndChangeCheck())
            {
                json.stringValue = JsonConvert.SerializeObject(container.objectReferenceValue);
            }
            EditorGUI.EndProperty();
        }
    }
}