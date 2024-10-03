using System.IO;
using UnityEngine;
using UnityEditor;
using Kurisu.Framework.Serialization;
using UEditor = UnityEditor.Editor;
namespace Kurisu.Framework.DataDriven.Editor
{
    public delegate void DrawToolBarDelegate(DataTableEditor tableEditor);
    public delegate void DataTableUpdateDelegate(DataTable tableEditor);

    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : UEditor
    {
        public DataTable Table => target as DataTable;
        private DataTableRowView dataTableRowView;
        public event DrawToolBarDelegate OnDrawLeftTooBar;
        public event DrawToolBarDelegate OnDrawRightTooBar;
        public DataTableRowView GetDataTableRowView()
        {
            dataTableRowView ??= new DataTableRowView(Table);
            return dataTableRowView;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultTitle();
            DrawToolBar();
            GUILayout.Space(10);
            DrawRowView();
        }
        protected void DrawDefaultTitle()
        {
            GUILayout.Label("DataTable", new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            });
        }
        protected virtual void DrawRowView()
        {
            dataTableRowView = GetDataTableRowView();
            var typeProp = serializedObject.FindProperty("m_rowType");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Row Type", "Set DataTable Row Type"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RequestDataTableUpdate();
            }
            GUILayout.Space(5);
            dataTableRowView.DrawGUI(serializedObject);
        }
        #region Cleanup

        // DataTableEditor use global object manager to cache wrapper.
        // However, the soft object handle is not persistent.
        // We should ensure not to conflict with other modules.


        protected virtual void OnEnable()
        {
            GlobalObjectManager.Cleanup();
            Undo.undoRedoEvent += OnUndo;
        }

        protected virtual void OnDisable()
        {
            GlobalObjectManager.Cleanup();
            Undo.undoRedoEvent -= OnUndo;
        }

        #endregion
        /// <summary>
        /// Draw editor toolbar
        /// </summary>
        protected virtual void DrawToolBar()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export to Json", DataTableEditorUtils.ToolBarButtonStyle))
            {
                var jsonData = DataTableEditorUtils.ExportJson(Table);
                string path = EditorUtility.SaveFilePanel("Select json file export path", Application.dataPath, Table.name, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllText(path, jsonData);
                    Debug.Log($"<color=#3aff48>DataTable</color>: Save to json file succeed!");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            OnDrawLeftTooBar?.Invoke(this);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Import from Json", DataTableEditorUtils.ToolBarButtonStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select json file to import", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var data = File.ReadAllText(path);
                    DataTableEditorUtils.ImportJson(Table, data);
                    EditorUtility.SetDirty(Table);
                    AssetDatabase.SaveAssets();
                    RequestDataTableUpdate();
                    GUIUtility.ExitGUI();
                }
            }
            OnDrawRightTooBar?.Invoke(this);
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// Request change DataTable in editor
        /// </summary>
        protected void RequestDataTableUpdate()
        {
            DataTableEditorUtils.OnDataTablePreUpdate?.Invoke(Table);
            RebuildEditorView();
            DataTableEditorUtils.OnDataTablePostUpdate?.Invoke(Table);
        }
        /// <summary>
        /// Rebuild editor gui view, called on DataTable changed
        /// </summary>
        protected virtual void RebuildEditorView()
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