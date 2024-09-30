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
namespace Kurisu.Framework.Editor
{
    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : UnityEditor.Editor
    {
        private ReorderableList reorderableList;
        private DataTable Table => target as DataTable;
        public override void OnInspectorGUI()
        {
            var typeProp = serializedObject.FindProperty("m_rowType");
            var rowsProp = serializedObject.FindProperty("m_rows");
            bool canEdit = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(typeProp);

            var rowStructType = Table.GetRowStructType();
            if (rowStructType != null)
            {
                var header = GetFieldsName(rowStructType);
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
                    Table.AddRow((IDataTableRow)Activator.CreateInstance(rowStructType));
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
                    Table.SwarpRow(oldIndex, newIndex);
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
            var jsonProp = property.FindPropertyRelative("jsonData");
            var objectHandleProp = property.FindPropertyRelative("objectHandle");
            var handle = new SoftObjectHandle(objectHandleProp.ulongValue);
            ScriptableObject wrapper = SerializationEditorManager.CreateWrapper(elementType, ref handle);
            if (objectHandleProp.ulongValue != handle.Handle)
            {
                objectHandleProp.ulongValue = handle.Handle;
                property.serializedObject.ApplyModifiedProperties();
            }
            if (!wrapper) return;

            if (!string.IsNullOrEmpty(jsonProp.stringValue))
            {
                JsonUtility.FromJsonOverwrite(jsonProp.stringValue, wrapper);
            }
            EditorGUI.BeginChangeCheck();
            SerializedObjectWrapperDrawer.DrawGUIHorizontal(rect, columNum, wrapper);
            if (EditorGUI.EndChangeCheck())
            {
                jsonProp.stringValue = JsonUtility.ToJson(wrapper);
            }
        }
        private void DrawDataTableHeader(Rect rect, List<string> header)
        {
            rect.width /= header.Count;
            rect.width -= 5;
            for (int i = 0; i < header.Count; ++i)
            {
                GUI.Label(rect, header[i]);
                rect.x += rect.width + 5;
            }
        }
        private float GetDataTableRowHeight(Type elementType, SerializedProperty property)
        {
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