using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.Schedulers.Editor
{
    [CustomEditor(typeof(SchedulerRunner))]
    public class SchedulerRunnerEditor : UnityEditor.Editor
    {
        private SchedulerRunner Manager => target as SchedulerRunner;
        private int ManagedScheduledCount => Manager.scheduledItems.Count;
        private int ManagedScheduledCapacity => Manager.scheduledItems.InternalCapacity;
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            EditorApplication.update += Repaint;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            EditorApplication.update -= Repaint;
        }
        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter play mode to track tasks", MessageType.Info);
                return;
            }
            GUIStyle stackTraceButtonStyle = new(GUI.skin.button)
            {
                wordWrap = true,
                fontSize = 12
            };
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Managed scheduled task count: {ManagedScheduledCount} capacity: {ManagedScheduledCapacity}");
            foreach (var scheduled in Manager.scheduledItems)
            {
                double elapsedTime = Time.timeSinceLevelLoadAsDouble - scheduled.Timestamp;
                if (SchedulerRegistry.TryGetListener(scheduled.Value, out var listener))
                {
                    GUILayout.Label($"Id {scheduled.Value.Handle.Handle}, {listener.name}, elapsed time: {elapsedTime}s.");
                    EditorGUI.indentLevel++;
                    if (GUILayout.Button($"{listener.fileName} {listener.lineNumber}", stackTraceButtonStyle))
                    {
                        CodeEditor.Editor.CurrentCodeEditor.OpenProject(listener.fileName, listener.lineNumber);
                    }
                    EditorGUI.indentLevel--;
                }
            }
            GUILayout.EndVertical();
        }
    }
}
