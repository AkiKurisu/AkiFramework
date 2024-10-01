using UnityEditor;
using Kurisu.Framework.DataDriven;
using UnityEngine;
using Kurisu.Framework.Serialization.Editor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditorInternal;
using Kurisu.Framework.Serialization;
using System.IO;
namespace Kurisu.Framework.Editor
{
    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : UnityEditor.Editor
    {
        private const int ColumSpace = 5;
        private ReorderableList reorderableList;
        private DataTable Table => target as DataTable;
        public override void OnInspectorGUI()
        {
            GUILayout.Label("DataTable", new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            });
            DrawToolBar();
            GUILayout.Space(10);

            var typeProp = serializedObject.FindProperty("m_rowType");
            var rowsProp = serializedObject.FindProperty("m_rows");
            bool canEdit = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Row Type", "Set DataTable Row Type"));
            GUILayout.Space(5);

            var rowStructType = Table.GetRowStructType();
            if (rowStructType != null)
            {
                var header = GetFieldsName(rowStructType);
                header.Insert(0, "Row Id");
                var rows = Table.GetAllRows();
                reorderableList ??= new ReorderableList(Table.GetAllRows(), rowStructType, true, true, true, true);
                reorderableList.multiSelect = true;
                reorderableList.elementHeightCallback = (int index) =>
                {
                    var prop = rowsProp.GetArrayElementAtIndex(index);
                    return GetDataTableRowHeight(rowStructType, prop);
                };
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    DrawDataTableHeader(rect, header);
                };
                reorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    var prop = rowsProp.GetArrayElementAtIndex(index);
                    DrawDataTableRows(rect, header.Count, rowStructType, prop);
                };
                reorderableList.onAddCallback = list =>
                {
                    Table.AddRow(Table.NewRowId(), (IDataTableRow)Activator.CreateInstance(rowStructType));
                    canEdit = false;
                    RefreshTableView();
                };
                reorderableList.onRemoveCallback = list =>
                {
                    if (rows.Length == 0) return;

                    var indexsToRemove = list.selectedIndices.ToList();
                    if (indexsToRemove.Count == 0)
                    {
                        indexsToRemove.Add(rows.Length - 1);
                    }
                    Table.RemoveRow(indexsToRemove);
                    canEdit = false;
                    RefreshTableView();
                };
                reorderableList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    Table.ReorderRow(oldIndex, newIndex);
                    canEdit = false;
                    RefreshTableView();
                };
                reorderableList.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck() && canEdit)
            {
                serializedObject.ApplyModifiedProperties();
                RefreshTableView();
            }
        }
        #region Cleanup

        // DataTableEditor use global object manager to cache wrapper.
        // However, the soft object handle is not persistent.
        // We should ensure not to conflict with other modules.
        private void OnEnable()
        {
            GlobalObjectManager.Cleanup();
        }
        private void OnDisable()
        {
            GlobalObjectManager.Cleanup();
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
                    AssetDatabase.SaveAssets();
                    var data = File.ReadAllText(path);
                    Table.ImportJson(data);
                    RefreshTableView();
                }
            }
            GUILayout.EndHorizontal();
        }
        private void RefreshTableView()
        {
            Table.InternalUpdate();
            serializedObject.Update();
            // rebuild reorderable list next editor frame
            reorderableList = null;
        }
        private static List<string> GetFieldsName(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                            .Concat(GetAllFields(type))
                            .Where(field => field.IsInitOnly == false)
                            .Select(x => x.Name)
                            .ToList();
        }
        private static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.GetCustomAttribute<SerializeField>() != null).Concat(GetAllFields(t.BaseType));
        }
        private void DrawDataTableRows(Rect rect, int columNum, Type elementType, SerializedProperty property)
        {
            rect.width /= columNum;
            rect.width -= ColumSpace;
            var height = rect.height;
            rect.height = EditorGUIUtility.singleLineHeight;
            var rowNameProp = property.FindPropertyRelative("RowId");
            EditorGUI.PropertyField(rect, rowNameProp, GUIContent.none);
            // TODO: Add row id validation
            rect.x += rect.width + ColumSpace;
            rect.height = height;

            property = property.FindPropertyRelative("RowData");
            var jsonProp = property.FindPropertyRelative("jsonData");
            var objectHandleProp = property.FindPropertyRelative("objectHandle");
            var handle = new SoftObjectHandle(objectHandleProp.ulongValue);
            SerializedObjectWrapper wrapper = SerializationEditorManager.CreateWrapper(elementType, ref handle);
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
            SerializedObjectWrapperDrawer.DrawGUIHorizontal(rect, ColumSpace, wrapper);
            if (EditorGUI.EndChangeCheck())
            {
                jsonProp.stringValue = JsonUtility.ToJson(wrapper.Value);
            }
        }
        private void DrawDataTableHeader(Rect rect, List<string> header)
        {
            rect.width /= header.Count;
            rect.width -= ColumSpace;
            for (int i = 0; i < header.Count; ++i)
            {
                GUI.Label(rect, header[i]);
                rect.x += rect.width + ColumSpace;
            }
        }
        private float GetDataTableRowHeight(Type elementType, SerializedProperty property)
        {
            property = property.FindPropertyRelative("RowData");
            var objectHandleProp = property.FindPropertyRelative("objectHandle");
            var handle = new SoftObjectHandle(objectHandleProp.ulongValue);
            ScriptableObject wrapper = SerializationEditorManager.CreateWrapper(elementType, ref handle);
            if (objectHandleProp.ulongValue != handle.Handle)
            {
                objectHandleProp.ulongValue = handle.Handle;
                property.serializedObject.ApplyModifiedProperties();
            }
            return SerializedObjectWrapperDrawer.CalculatePropertyHeightLayout(wrapper);
        }
    }
}