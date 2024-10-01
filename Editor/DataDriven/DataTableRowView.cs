using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Kurisu.Framework.Serialization;
using Kurisu.Framework.Serialization.Editor;
namespace Kurisu.Framework.DataDriven.Editor
{
    /// <summary>
    /// GUI class for drawing DataTable rows
    /// </summary>
    public class DataTableRowView
    {
        private const int ColumSpace = 5;
        private ReorderableList reorderableList;
        public DataTable Table { get; }
        public DataTableRowView(DataTable dataTable)
        {
            Table = dataTable;
        }
        public void DrawGUI()
        {
            using var serializedObject = new SerializedObject(Table);
            DrawGUI(serializedObject);
        }
        public void DrawGUI(SerializedObject serializedObject)
        {
            var rowsProp = serializedObject.FindProperty("m_rows");
            bool canEdit = true;
            EditorGUI.BeginChangeCheck();
            var rowStructType = Table.GetRowStructType();
            if (rowStructType != null)
            {
                var header = GetSerializedFieldsName(rowStructType);
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
                    Rebuild();
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
                    Rebuild();
                };
                reorderableList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    Table.ReorderRow(oldIndex, newIndex);
                    canEdit = false;
                    Rebuild();
                };
                reorderableList.DoLayoutList();
            }

            if (EditorGUI.EndChangeCheck() && canEdit)
            {
                serializedObject.ApplyModifiedProperties();
                Rebuild();
            }
        }
        /// <summary>
        /// Rebuild rows view
        /// </summary>
        public void Rebuild()
        {
            // ensure row type is matched with row struct before drawing
            Table.InternalUpdate();
            // rebuild reorderable list next editor frame
            reorderableList = null;
        }
        /// <summary>
        /// Get all fields of type that Unity can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<string> GetSerializedFieldsName(Type type)
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
            ScriptableObject wrapper = SerializedObjectWrapperManager.CreateWrapper(elementType, ref handle);
            if (objectHandleProp.ulongValue != handle.Handle)
            {
                objectHandleProp.ulongValue = handle.Handle;
                property.serializedObject.ApplyModifiedProperties();
            }
            return SerializedObjectWrapperDrawer.CalculatePropertyHeightLayout(wrapper);
        }
    }
}