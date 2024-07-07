using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Kurisu.Framework.AI
{
    [CustomPropertyDrawer(typeof(TaskIDAttribute), true)]
    public class TaskIDDrawer : PropertyDrawer
    {
        private class AITasks
        {
            public readonly string[] Values;
            public AITasks()
            {
                var hosts = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(x => x.GetCustomAttribute<TaskIDHostAttribute>() != null);
                var list = new List<string>();
                foreach (var host in hosts)
                {
                    foreach (var fieldInfo in host.GetFields().Where(x => x.FieldType == typeof(string) && x.IsLiteral))
                    {
                        list.Add(fieldInfo.GetValue(null) as string);
                    }
                }
                Values = list.ToArray();
            }
        }
        private static readonly GUIContent k_IsNotStringLabel = new("The property type is not string.");
        private static AITasks tasks;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            tasks ??= new();
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                Rect popupPosition = new(position)
                {
                    height = EditorGUIUtility.singleLineHeight
                };
                int index = EditorGUI.Popup(position: popupPosition, label.text, selectedIndex: Array.IndexOf(tasks.Values, property.stringValue), displayedOptions: tasks.Values);
                if (index >= 0)
                {
                    property.stringValue = tasks.Values[index];
                }
            }
            else
            {
                EditorGUI.LabelField(position, label, k_IsNotStringLabel);
            }
            EditorGUI.EndProperty();
        }
    }
}
