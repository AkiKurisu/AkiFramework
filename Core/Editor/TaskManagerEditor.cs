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
        private int CurrentJobID => Manager.jobID;
        private int ManagedJobCount => Manager.managedJobIDs.Count;
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
            Manager.EditorUpdate += Repaint;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            Manager.EditorUpdate -= Repaint;
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
            GUILayout.Label($"Current Job ID : {CurrentJobID}");
            GUILayout.Label($"Managed Job Count : {ManagedJobCount}");
            GUILayout.Label($"Unmanaged Task Count : {UnManagedTaskCount}");
            int count;
            if ((count = UnManagedTaskCount - ManagedJobCount) > 0)
            {
                EditorGUILayout.HelpBox($"Task leak, {count} tasks can not be recycled", MessageType.Warning);
            }
            GUILayout.Label($"Updating Task Count : {UpdatingTaskCount}");
        }
    }
}
