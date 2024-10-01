using System.IO;
using UnityEngine;
using UnityEditor;
using Kurisu.Framework.Serialization;
namespace Kurisu.Framework.DataDriven.Editor
{
    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : UnityEditor.Editor
    {
        private DataTable Table => target as DataTable;
        private DataTableRowView dataTableRowView;
        public override void OnInspectorGUI()
        {
            DrawTitle();
            DrawToolBar();
            GUILayout.Space(10);
            DrawRowView();
        }
        private void DrawTitle()
        {
            GUILayout.Label("DataTable", new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            });
        }
        private void DrawRowView()
        {
            dataTableRowView ??= new DataTableRowView(Table);
            var typeProp = serializedObject.FindProperty("m_rowType");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Row Type", "Set DataTable Row Type"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RebuildEditorView();
            }
            GUILayout.Space(5);
            dataTableRowView.DrawGUI(serializedObject);
        }
        #region Cleanup

        // DataTableEditor use global object manager to cache wrapper.
        // However, the soft object handle is not persistent.
        // We should ensure not to conflict with other modules.
        private void OnEnable()
        {
            GlobalObjectManager.Cleanup();
            Undo.undoRedoEvent += OnUndo;
        }
        private void OnDisable()
        {
            GlobalObjectManager.Cleanup();
            Undo.undoRedoEvent -= OnUndo;
        }

        #endregion
        private void DrawToolBar()
        {
            var ToolBarStyle = new GUIStyle("LargeButton");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export to Json", ToolBarStyle))
            {
                var jsonData = Table.ExportJson();
                string path = EditorUtility.SaveFilePanel("Select json file export path", Application.dataPath, Table.name, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, jsonData);
                    Debug.Log($"<color=#3aff48>DataTable</color>: Save to json file succeed!");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Import from Json", ToolBarStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select json file to import", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var data = File.ReadAllText(path);
                    Table.ImportJson(data);
                    EditorUtility.SetDirty(Table);
                    AssetDatabase.SaveAssets();
                    RebuildEditorView();
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void RebuildEditorView()
        {
            dataTableRowView.Rebuild();
            serializedObject.Update();
        }
        private void OnUndo(in UndoRedoInfo undo)
        {
            // Manually fresh row view after undo
            RebuildEditorView();
        }
    }
}