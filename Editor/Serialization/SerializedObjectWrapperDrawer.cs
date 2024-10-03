using UnityEngine;
using UnityEditor;
namespace Kurisu.Framework.Serialization.Editor
{
    // Modified from https://gist.github.com/tomkail/ba4136e6aa990f4dc94e0d39ec6a058c
    public class SerializedObjectWrapperDrawer
    {
        public static float CalculatePropertyHeight(ScriptableObject data)
        {
            if (data == null) return 0;
            SerializedObject serializedObject = new(data);
            try
            {
                SerializedProperty prop = serializedObject.FindProperty("m_Value");
                if (prop == null)
                {
                    return 0;
                }
                float totalHeight = 0;
                if (prop.NextVisible(true))
                {
                    do
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, null, true) + EditorGUIUtility.standardVerticalSpacing;
                        totalHeight += height;
                    }
                    while (prop.NextVisible(false));
                }
                return totalHeight;
            }
            finally
            {
                serializedObject.Dispose();
            }
        }
        public static float CalculatePropertyHeightLayout(ScriptableObject data)
        {
            if (data == null) return 0;
            SerializedObject serializedObject = new(data);
            try
            {
                SerializedProperty prop = serializedObject.FindProperty("m_Value");
                if (prop == null)
                {
                    return 0;
                }
                float totalHeight = 0;
                if (prop.NextVisible(true))
                {
                    do
                    {
                        float height = EditorGUI.GetPropertyHeight(prop, null, true);
                        totalHeight = Mathf.Max(height, totalHeight);
                    }
                    while (prop.NextVisible(false));
                }
                return totalHeight;
            }
            finally
            {
                serializedObject.Dispose();
            }
        }
        public static void DrawGUIHorizontal(Rect startRect, int space, ScriptableObject data)
        {
            var serializedObject = new SerializedObject(data);
            SerializedProperty property = serializedObject.FindProperty("m_Value");
            if (property != null && property.NextVisible(true))
            {
                do
                {
                    EditorGUI.PropertyField(startRect, property, GUIContent.none, true);
                    startRect.x += startRect.width + space;
                }
                while (property.NextVisible(false));
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
        }
        public static void DrawGUI(Rect position, ScriptableObject data)
        {
            var serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.FindProperty("m_Value");
            if (prop != null && prop.NextVisible(true))
            {
                do
                {
                    float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                    position.height = height;
                    EditorGUI.PropertyField(position, prop, true);
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                }
                while (prop.NextVisible(false));
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
        }
        private static bool AreAnySubPropertiesVisible(SerializedProperty property)
        {
            var data = (ScriptableObject)property.objectReferenceValue;
            SerializedObject serializedObject = new(data);
            SerializedProperty prop = serializedObject.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.name == "m_Script") continue;
                return true;
            }
            serializedObject.Dispose();
            return false;
        }
    }
}