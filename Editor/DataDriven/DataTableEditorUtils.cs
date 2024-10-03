using System;
using UnityEditor;
using UnityEngine;
namespace Kurisu.Framework.DataDriven.Editor
{
    /// <summary>
    /// Utility for editing DataTable in editor script
    /// </summary>
    public static class DataTableEditorUtils
    {
        public static GUIStyle ToolBarButtonStyle => new("LargeButton");
        public static DataTableUpdateDelegate OnDataTablePreUpdate;
        public static DataTableUpdateDelegate OnDataTablePostUpdate;
        public static void SetRowStruct<T>(DataTable dataTable) where T : class, IDataTableRow
        {
            Undo.RecordObject(dataTable, "Update DataTable Row Struct");
            dataTable.SetRowStruct(typeof(T));
            EditorUtility.SetDirty(dataTable);
        }
        public static void SetRowStruct(DataTable dataTable, Type rowType)
        {
            Undo.RecordObject(dataTable, "Update DataTable Row Struct");
            dataTable.SetRowStruct(rowType);
            EditorUtility.SetDirty(dataTable);
        }
        public static string ExportJson(DataTable dataTable)
        {
            return JsonUtility.ToJson(dataTable);
        }
        public static void ImportJson(DataTable dataTable, string jsonData)
        {
            Undo.RecordObject(dataTable, "Overwrite DataTable from Json");
            JsonUtility.FromJsonOverwrite(jsonData, dataTable);
        }
        public static bool ValidNewRowId(DataTable dataTable, string oldRowId, string newRowId, out string result)
        {
            if (dataTable.GetRowMap().ContainsKey(newRowId))
            {
                result = oldRowId;
                return false;
            }
            result = newRowId;
            return true;
        }
        public static string GetNewRowId(DataTable dataTable)
        {
            return dataTable.NewRowId();
        }
        /// <summary>
        /// Get data rows from table without modify default object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T[] GetAllRowsSafe<T>(DataTable dataTable) where T : class, IDataTableRow
        {
            return dataTable.GetAllRowsSafe<T>();
        }
        /// <summary>
        /// Get data rows from table without modify default object
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IDataTableRow[] GetAllRowsSafe(DataTable dataTable)
        {
            return dataTable.GetAllRowsSafe();
        }
    }
}
