using UnityEditor;
using Kurisu.Framework.Tasks;
using UnityEngine;
namespace Kurisu.Framework.Editor
{
    [CustomEditor(typeof(TaskManager))]
    public class TaskManagerEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            var manager = target as TaskManager;
            manager.EditorUpdate += Repaint;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            var manager = target as TaskManager;
            manager.EditorUpdate -= Repaint;
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
                EditorGUILayout.HelpBox("Enter play mode to track jobs and tasks", MessageType.Info);
                return;
            }
            var manager = target as TaskManager;
            GUILayout.Label($"Current Job ID : {manager.CurrentJobID}");
            GUILayout.Label($"Managed Job Count : {manager.ManagedJobCount}");
            GUILayout.Label($"Unmanaged Task Count : {manager.UnManagedTaskCount}");
            int count;
            if ((count = manager.UnManagedTaskCount - manager.ManagedJobCount) > 0)
            {
                EditorGUILayout.HelpBox($"Task leak, {count} tasks can not be recycled", MessageType.Warning);
            }
            GUILayout.Label($"Updating Task Count : {manager.UpdatingTaskCount}");
        }
    }
}
