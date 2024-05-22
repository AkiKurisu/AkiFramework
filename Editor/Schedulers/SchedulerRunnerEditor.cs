using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.Schedulers.Editor
{
    [CustomEditor(typeof(SchedulerRunner))]
    public class SchedulerRunnerEditor : UnityEditor.Editor
    {
        private SchedulerRunner Manager => target as SchedulerRunner;
        private int ManagedSchedulersCount => Manager.managedSchedulers.Count;
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
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("debugMode"), new GUIContent("Debug Mode"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter play mode to track schedulers", MessageType.Info);
                return;
            }
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Managed scheduler count: {ManagedSchedulersCount}");
            GUILayout.EndVertical();
        }
    }
}
