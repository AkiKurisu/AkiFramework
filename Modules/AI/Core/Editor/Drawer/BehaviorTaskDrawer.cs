using Kurisu.AkiBT.Editor;
using Kurisu.Framework.Editor;
using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.AI.Editor
{
    [CustomPropertyDrawer(typeof(BehaviorTask))]
    public class BehaviorTaskDrawer : PropertyDrawer
    {
        private static Color AkiGreen = new(170 / 255f, 255 / 255f, 97 / 255f);
        private static Color AkiBlue = new(140 / 255f, 160 / 255f, 250 / 255f);
        private static Color AkiYellow = new(255 / 255f, 244 / 255f, 94 / 255f);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect rect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };
            var isPersistent = property.FindPropertyRelative("isPersistent");
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("isPersistent"));
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            if (!isPersistent.boolValue)
            {
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("taskID"));
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
#if AKIAI_TASK_JSON_SERIALIZATION
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("behaviorTreeSerializeData"));
#else
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("behaviorTree"));
#endif
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            GUI.enabled = Application.isPlaying;
            var task = ReflectionUtility.GetTargetObjectWithProperty(property) as BehaviorTask;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = GetStatusColor(task.Status);
            if (GUI.Button(rect, "Debug Task Behavior"))
            {
                GraphEditorWindow.Show(task);
            }
            GUI.backgroundColor = color;
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            GUI.enabled = true;
            EditorGUI.EndProperty();
        }
        private Color GetStatusColor(TaskStatus taskStatus)
        {
            if (taskStatus == TaskStatus.Enabled) return AkiGreen;
            if (taskStatus == TaskStatus.Disabled) return AkiBlue;
            else return AkiYellow;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isPersistent = property.FindPropertyRelative("isPersistent").boolValue;
            return EditorGUIUtility.singleLineHeight * (isPersistent ? 3 : 4) + EditorGUIUtility.standardVerticalSpacing * (isPersistent ? 2 : 3);
        }
    }
}
