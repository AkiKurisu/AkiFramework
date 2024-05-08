using UnityEditor;
using Kurisu.Framework.Tasks;
using UnityEngine;
using System.Linq;
namespace Kurisu.Framework.Editor
{
    [CustomEditor(typeof(TaskManager))]
    public class TaskManagerEditor : UnityEditor.Editor
    {
        private TaskManager Manager => target as TaskManager;
        private int CurrentTaskId => Manager.taskId;
        private int ManagedTaskCount => Manager.managedTaskIds.Count;
        private int UnManagedTaskCount
        {
            get
            {
                return Manager.managedTasks.Count(x => x.Value is not Timer);
            }
        }
        private int UpdatingTaskCount => Manager._tasks.Count;
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
                EditorGUILayout.HelpBox("Enter play mode to track jobs and tasks", MessageType.Info);
                return;
            }
            GUILayout.Label($"Current task Id : {CurrentTaskId}");
            GUILayout.Label($"Managed task count : {ManagedTaskCount}");
            GUILayout.Label($"Unmanaged task count : {UnManagedTaskCount}");
            int count;
            if ((count = UnManagedTaskCount - ManagedTaskCount) > 0)
            {
                EditorGUILayout.HelpBox($"Task leaking, {count} tasks can not be pooled", MessageType.Warning);
            }
            GUILayout.Label($"Updating task count : {UpdatingTaskCount}");
        }
    }
}
