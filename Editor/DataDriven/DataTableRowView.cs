using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Kurisu.Framework.Serialization;
using Kurisu.Framework.Serialization.Editor;
using System.Collections.ObjectModel;
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
        private SerializedObject serializedObject;
        private int selectIndex;
        public bool ReadOnly { get; set; } = false;
        public DataTableRowView(DataTable dataTable)
        {
            Table = dataTable;
        }
        public void DrawGUI()
        {
            serializedObject = new SerializedObject(Table);
            DrawGUI(serializedObject);
            serializedObject.Dispose();
        }
        public void DrawGUI(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;
            var rowsProp = serializedObject.FindProperty("m_rows");
            bool canEdit = true;
            EditorGUI.BeginChangeCheck();
            var rowStructType = Table.GetRowStructType();
            if (rowStructType != null)
            {
                var header = GetSerializedFieldsName(rowStructType);
                header.Insert(0, "Row Id");
                var rows = Table.GetAllRows();
                reorderableList ??= new ReorderableList(rows, rowStructType, true, true, true, true);
                if (selectIndex >= 0)
                {
                    SelectRow(selectIndex);
                    selectIndex = -1;
                }
                reorderableList.multiSelect = true;
                reorderableList.elementHeightCallback = (int index) =>
                {
                    var prop = rowsProp.GetArrayElementAtIndex(index);
                    if (ReadOnly)
                    {
                        return EditorGUIUtility.singleLineHeight;
                    }
                    return GetDataTableRowHeight(rowStructType, prop);
                };
                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    DrawDataTableHeader(rect, header);
                };
                reorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    var prop = rowsProp.GetArrayElementAtIndex(index);
                    if (ReadOnly)
                    {
                        DrawReadOnlyDataTableRow(rect, header.Count, rowStructType, prop);
                    }
                    else
                    {
                        DrawDataTableRow(rect, header.Count, rowStructType, prop);
                    }
                };
                reorderableList.onAddCallback = list =>
                {
                    Table.AddRow(Table.NewRowId(), (IDataTableRow)Activator.CreateInstance(rowStructType));
                    canEdit = false;
                    RequestDataTableUpdate();
                    GUIUtility.ExitGUI();
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
                    RequestDataTableUpdate();
                    GUIUtility.ExitGUI();
                };
                reorderableList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    Table.ReorderRow(oldIndex, newIndex);
                    canEdit = false;
                    RequestDataTableUpdate();
                };
                reorderableList.DoLayoutList();
            }

            Event evt = Event.current;
            if (evt.type == EventType.ContextClick && GUILayoutUtility.GetLastRect().Contains(evt.mousePosition))
            {
                GenericMenu menu = new();
                menu.AddItem(new GUIContent("Clear"), false, () =>
                {
                    Table.RemoveAllRows();
                    canEdit = false;
                    RequestDataTableUpdate();
                });
                menu.ShowAsContext();
            }


            if (EditorGUI.EndChangeCheck() && canEdit)
            {
                serializedObject.ApplyModifiedProperties();
                RequestDataTableUpdate();
            }
        }
        private static readonly int[] defaultIndices = new int[0];
        /// <summary>
        /// Get row view selected indices
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<int> GetSelectedIndices()
        {
            if (reorderableList == null) return new ReadOnlyCollection<int>(defaultIndices);
            return reorderableList.selectedIndices;
        }
        /// <summary>
        /// Get row view selected rows
        /// </summary>
        /// <returns></returns>
        public IDataTableRow[] GetSelectedRows()
        {
            if (reorderableList == null) return new IDataTableRow[0];
            var rows = Table.GetAllRows();
            return reorderableList.selectedIndices.Select(x => rows[x]).ToArray();
        }
        /// <summary>
        /// Get row view selected rows in serialized property format
        /// </summary>
        /// <returns></returns>
        public SerializedProperty[] GetSelectedRowProperties(SerializedObject serializedObject)
        {
            var rows = Table.GetAllRows();
            var rowsProp = serializedObject.FindProperty("m_rows");
            return reorderableList.selectedIndices.Select(x => rowsProp.GetArrayElementAtIndex(x)).ToArray();
        }
        protected void RequestDataTableUpdate()
        {
            DataTableEditorUtils.OnDataTablePreUpdate?.Invoke(Table);
            Rebuild();
            DataTableEditorUtils.OnDataTablePostUpdate?.Invoke(Table);
        }
        /// <summary>
        /// Select target row in view
        /// </summary>
        /// <param name="index"></param>
        public void SelectRow(int index)
        {
            if (reorderableList == null)
            {
                // Cache pre-select index when list view is not prepared
                selectIndex = index;
                return;
            }
            if (selectIndex < reorderableList.count)
                reorderableList.Select(index);
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
            serializedObject.Update();
        }
        /// <summary>
        /// Get all fields of type that Unity can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<string> GetSerializedFieldsName(Type type)
        {
            List<FieldInfo> fieldInfos = new();
            GetAllSerializableFields(type, fieldInfos);
            return fieldInfos.Where(field => field.IsInitOnly == false)
                            .Select(x => x.Name)
                            .ToList();
        }
        private static void GetAllSerializableFields(Type type, List<FieldInfo> fieldInfos)
        {
            if (type.BaseType != null)
            {
                GetAllSerializableFields(type.BaseType, fieldInfos);
            }


            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue;

                if (!field.FieldType.IsSerializable && !typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType) && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum
                    && !typeof(List<>).IsAssignableFrom(field.FieldType) && !field.FieldType.IsArray
                    && !field.FieldType.IsGenericType
                    && !Attribute.IsDefined(field, typeof(SerializableAttribute), false))
                    continue;

                if (!field.IsPublic && !Attribute.IsDefined(field, typeof(SerializeField), false)) continue;

                fieldInfos.Add(field);
            }
        }

        private void DrawDataTableRow(Rect rect, int columNum, Type elementType, SerializedProperty property)
        {
            rect.width /= columNum;
            rect.width -= ColumSpace;
            var height = rect.height;
            rect.height = EditorGUIUtility.singleLineHeight;
            var rowIdProp = property.FindPropertyRelative("RowId");
            string rowId = rowIdProp.stringValue;
            EditorGUI.PropertyField(rect, rowIdProp, GUIContent.none);
            if (rowId != rowIdProp.stringValue)
            {
                DataTableEditorUtils.ValidateNewRowId(Table, rowId, rowIdProp.stringValue, out string validId);
                rowIdProp.stringValue = validId;
            }
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
        private void DrawReadOnlyDataTableRow(Rect rect, int columNum, Type elementType, SerializedProperty property)
        {
            rect.width /= columNum;
            rect.width -= ColumSpace;
            var height = rect.height;
            rect.height = EditorGUIUtility.singleLineHeight;
            var rowIdProp = property.FindPropertyRelative("RowId");
            string rowId = rowIdProp.stringValue;
            EditorGUI.PropertyField(rect, rowIdProp, GUIContent.none);
            if (rowId != rowIdProp.stringValue)
            {
                DataTableEditorUtils.ValidateNewRowId(Table, rowId, rowIdProp.stringValue, out string validId);
                rowIdProp.stringValue = validId;
            }
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
            SerializedObjectWrapperDrawer.DrawReadOnlyGUIHorizontal(rect, ColumSpace, wrapper);
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