using System;
using Chris.Editor;
using Chris.Serialization;
using Chris.Serialization.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UEditor = UnityEditor.Editor;
namespace Chris.DataDriven.Editor
{
    /// <summary>
    /// Edit data table in additional EditorWindow instead of Inspector, useful when data row contains a lot of content
    /// </summary>
    public class DataTableEditorWindow : EditorWindow
    {
        private static DataTableEditorWindow _window;
        
        private object _splitterState;
        
        private static readonly GUILayoutOption[] EmptyLayoutOption = Array.Empty<GUILayoutOption>();
        
        private Vector2 _tableScroll;
        
        private static readonly GUIContent OpenDataTableContent = EditorGUIUtility.TrTextContent("Open", "Open a DataTable", (Texture)null);

        private const string PathCacheKey = "DataTableEditorWindow_LastPath";

        private DataTable _currentTarget;
        
        private InlineDataTableEditor _currentEditor;
        
        private string _currentPath;
        
        private static GUIStyle _detailsStyle;
        
        private Vector2 _detailsScroll;
        
        [MenuItem("Tools/Chris/DataTable Editor")]
        public static void OpenWindow()
        {
            if (_window != null)
            {
                _window.Close();
            }
            GetWindow<DataTableEditorWindow>("DataTable Editor").Show();
        }
        
        public static void OpenWindow(DataTable dataTable)
        {
            if (_window != null)
            {
                _window.Close();
            }
            _window = GetWindow<DataTableEditorWindow>("DataTable Editor");
            _window.Show();
            _window.OpenDataTable(dataTable);
        }
        
        private void OnEnable()
        {
            _window = this;
            _splitterState = SplitterGUILayout.CreateSplitterState(new float[] { 50f, 50f }, new int[] { 32, 32 }, null);
        }
        
        private void OnDisable()
        {
            if (!_currentEditor) return;
            
            /* Trigger save assets to force cleanup editor cache */
            EditorUtility.SetDirty(_currentEditor.Table);
            AssetDatabase.SaveAssetIfDirty(_currentEditor.Table);
            /* Auto register table if it has AddressableDataTableAttribute */
            DataTableEditorUtils.RegisterTableToAssetGroup(_currentEditor.Table);
            DestroyImmediate(_currentEditor);
        }
        
        private void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(_splitterState, EmptyLayoutOption);
            {
                RenderTable();

                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        private void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, EmptyLayoutOption);

            if (GUILayout.Button(OpenDataTableContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                string lastPath = EditorPrefs.GetString(PathCacheKey, Application.dataPath);
                string path = EditorUtility.OpenFilePanel("Select DataTable to edit", lastPath, "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    EditorPrefs.SetString(PathCacheKey, path);
                    var relativePath = path.Replace(Application.dataPath, "Assets/");
                    var dataTable = AssetDatabase.LoadAssetAtPath<DataTable>(relativePath);
                    OpenDataTable(dataTable);
                }
                GUIUtility.ExitGUI();
            }
            GUILayout.Label(_currentPath);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void OpenDataTable(DataTable dataTable)
        {
            _currentTarget = dataTable;
            _currentPath = AssetDatabase.GetAssetPath(dataTable);
            if (_currentEditor)
            {
                DestroyImmediate(_currentEditor);
            }
            _currentEditor = (InlineDataTableEditor)UEditor.CreateEditor(_currentTarget, typeof(InlineDataTableEditor));
        }
        
        private void RenderTable()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            if (_currentEditor)
            {
                _currentEditor.DrawToolBarInternal();
                GUILayout.Space(10);
            }
            _tableScroll = EditorGUILayout.BeginScrollView(_tableScroll, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)
            });
            if (_currentEditor)
            {
                _currentEditor.OnInspectorGUI();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void RenderDetailsPanel()
        {
            _detailsStyle ??= new GUIStyle(GUI.skin.box);
            _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll, _detailsStyle, EmptyLayoutOption);
            if (_currentEditor)
            {
                _currentEditor.DrawSelectedRowDetail();
            }
            EditorGUILayout.EndScrollView();
        }
        
#pragma warning disable IDE0051
        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int _)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset is DataTable dataTable)
            {
                OpenWindow(dataTable);
                Selection.SetActiveObjectWithContext(null, null);
                return true;
            }
            return false;
        }
#pragma warning restore IDE0051
        
        private class InlineDataTableEditor : DataTableEditor
        {
            public void DrawToolBarInternal()
            {
                DrawToolBar();
            }
            
            public override void OnInspectorGUI()
            {
                DrawRowView();
            }
            
            protected override DataTableRowView CreateDataTableRowView(DataTable table)
            {
                var rowView = base.CreateDataTableRowView(table);
                rowView.ReadOnly = true;
                return rowView;
            }
            
            public void DrawSelectedRowDetail()
            {
                if (Table.GetRowStructType() == null) return;

                var rowView = GetDataTableRowView();
                if (rowView.GetSelectedIndices().Count > 0)
                {
                    int index = rowView.GetSelectedIndices()[0];
                    var rowProp = rowView.GetSelectedRowProperties(serializedObject)[0];
                    EditorGUI.BeginChangeCheck();
                    DrawDataTableRowDetail(rowProp, Table.GetRowStructType());
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        RequestDataTableUpdate();
                        // Recover selected row after rebuild
                        rowView.SelectRow(index);
                    }
                }
            }
            
            private void DrawDataTableRowDetail(SerializedProperty property, Type elementType)
            {
                property = property.FindPropertyRelative("RowData");
                var jsonProp = property.FindPropertyRelative("jsonData");
                var objectHandleProp = property.FindPropertyRelative("objectHandle");
                var handle = new SoftObjectHandle(objectHandleProp.ulongValue);
                SerializedObjectWrapper wrapper = SerializedObjectWrapperManager.CreateWrapper(elementType, ref handle);
                if (objectHandleProp.ulongValue != handle.Handle)
                {
                    objectHandleProp.ulongValue = handle.Handle;
                    property.serializedObject.ApplyModifiedProperties();
                }
                if (!wrapper) return;

                if (!string.IsNullOrEmpty(jsonProp.stringValue))
                {
                    JsonUtility.FromJsonOverwrite(jsonProp.stringValue, wrapper.Value);
                }
                EditorGUI.BeginChangeCheck();
                SerializedObjectWrapperDrawer.DrawGUILayout(wrapper);
                if (EditorGUI.EndChangeCheck())
                {
                    jsonProp.stringValue = JsonUtility.ToJson(wrapper.Value);
                }
            }

            protected override void OnDisable()
            {
                GlobalObjectManager.Cleanup();
                Undo.undoRedoEvent -= OnUndo;
                Table.Cleanup();
                // Register DataTable to Addressable in EditorWindow::OnDisable
            }
        }
    }
}
