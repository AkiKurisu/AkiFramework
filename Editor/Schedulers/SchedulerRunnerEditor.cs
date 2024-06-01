using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.Schedulers.Editor
{
    [CustomEditor(typeof(SchedulerRunner))]
    public class SchedulerRunnerEditor : UnityEditor.Editor
    {
        private SchedulerRunner Manager => target as SchedulerRunner;
        private int ManagedScheduledCount => Manager.managedScheduled.Count;
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
            GUILayout.Label($"Managed scheduled task count: {ManagedScheduledCount}");
            foreach (var scheduled in Manager.scheduledRunning)
            {
                double elapsedTime = (Scheduler.Now - scheduled.Timestamp).TotalSeconds;
                GUILayout.Label($"Task Id {scheduled.Id}, elapsed time: {elapsedTime}s.");
                if (SchedulerRegistry.TryGetListener(scheduled.Value, out var listener))
                {
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
