using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.AI.Editor
{
    [CustomEditor(typeof(AIController), true)]
    public class AIControllerEditor : UnityEditor.Editor
    {
        private AIController Controller => target as AIController;
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("AIController", new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter }, GUILayout.MinHeight(30));
            GUILayout.Label("AI Status       " + (Controller.enabled ?
             (Controller.IsAIEnabled ? "<color=#92F2FF>Running</color>" : "<color=#FFF892>Pending</color>")
             : "<color=#FF787E>Disabled</color>"), new GUIStyle(GUI.skin.label) { richText = true });
            if (Application.isPlaying)
            {
                var tasks = Controller.GetAllTasks();
                if (tasks.Any())
                {
                    GUILayout.Label($"Tasks:", new GUIStyle(GUI.skin.label) { fontSize = 15 });
                }
                foreach (var task in tasks)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{task.TaskID}");
                    var rect = GUILayoutUtility.GetLastRect();
                    rect.x += 200;
                    if (task.IsPersistent)
                    {
                        GUI.Label(rect, "Status    <color=#A35EFF>Persistent</color>", new GUIStyle(GUI.skin.label) { richText = true });
                    }
                    else
                    {
                        GUI.Label(rect, $"Status    {GetStatus(task.Status)}", new GUIStyle(GUI.skin.label) { richText = true });
                    }
                    GUILayout.EndHorizontal();
                }
            }
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
        private string GetStatus(TaskStatus status)
        {
            if (status == TaskStatus.Enabled)
            {
                return "<color=#92F2FF>Running</color>";
            }
            else if (status == TaskStatus.Pending)
            {
                return "<color=#FFF892>Pending</color>";
            }
            else
            {
                return "<color=#FF787E>Disabled</color>";
            }
        }
    }
}
