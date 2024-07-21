using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.Tasks.Editor
{
    [CustomEditor(typeof(TaskRunner))]
    public class TaskRunnerEditor : UnityEditor.Editor
    {
        private TaskRunner Manager => target as TaskRunner;
        private int ManagedTaskCount => Manager._tasks.Count;
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
            var style = new GUIStyle(GUI.skin.label) { richText = true };
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Managed task count: {ManagedTaskCount}");
            foreach (var task in Manager._tasks)
            {
                GUILayout.Label($"{task.GetType().Name} Status: {TaskEditorUtils.StatusToString(task.GetStatus())}", style);
            }
            GUILayout.EndVertical();
        }
    }
    public static class TaskEditorUtils
    {
        public static string StatusToString(TaskStatus status)
        {
            if (status == TaskStatus.Running)
            {
                return "<color=#92F2FF>Running</color>";
            }
            else if (status == TaskStatus.Paused)
            {
                return "<color=#FFF892>Paused</color>";
            }
            else if (status == TaskStatus.Completed)
            {
                return "<color=#FFF892>Completed</color>";
            }
            else
            {
                return "<color=#FF787E>Stopped</color>";
            }
        }
    }
}
