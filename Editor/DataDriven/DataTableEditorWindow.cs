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
        private static DataTableEditorWindow window;
        private object splitterState;
        private static readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        [MenuItem("Tools/AkiFramework/DataTable Editor")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }
            GetWindow<DataTableEditorWindow>("DataTable Editor").Show();
        }
        public static void OpenWindow(DataTable dataTable)
        {
            if (window != null)
            {
                window.Close();
            }
            window = GetWindow<DataTableEditorWindow>("DataTable Editor");
            window.Show();
            window.OpenDataTable(dataTable);
        }
        private void OnEnable()
        {
            window = this;
            splitterState = SplitterGUILayout.CreateSplitterState(new float[] { 50f, 50f }, new int[] { 32, 32 }, null);
        }
        private void OnDisable()
        {
            if (currentEditor)
            {
                DestroyImmediate(currentEditor);
            }
        }
        private void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(splitterState, EmptyLayoutOption);
            {
                RenderTable();

                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }
        private static readonly GUIContent OpenDataTableContent = EditorGUIUtility.TrTextContent("Open", "Open a DataTable", (Texture)null);
        private static readonly string PathCacheKey = "DataTableEditorWindow_LastPath";
        private DataTable currentTarget;
        private InlineDataTableEditor currentEditor;
        private string currentPath;
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
            GUILayout.Label(currentPath);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        private void OpenDataTable(DataTable dataTable)
        {
            currentTarget = dataTable;
            currentPath = AssetDatabase.GetAssetPath(dataTable);
            if (currentEditor)
            {
                DestroyImmediate(currentEditor);
            }
            currentEditor = (InlineDataTableEditor)UEditor.CreateEditor(currentTarget, typeof(InlineDataTableEditor));
        }

        private Vector2 tableScroll;
        private void RenderTable()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            if (currentEditor)
            {
                currentEditor.DrawToolBarInternal();
                GUILayout.Space(10);
            }
            tableScroll = EditorGUILayout.BeginScrollView(tableScroll, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)
            });
            if (currentEditor)
            {
                currentEditor.OnInspectorGUI();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        private static GUIStyle detailsStyle;
        private Vector2 detailsScroll;
        private void RenderDetailsPanel()
        {
            detailsStyle ??= new GUIStyle(GUI.skin.box);
            detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll, detailsStyle, EmptyLayoutOption);
            if (currentEditor)
            {
                currentEditor.DrawSelectedRowDetail();
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
        }
    }
}
