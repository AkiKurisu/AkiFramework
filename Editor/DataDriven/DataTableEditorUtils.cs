using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.DataDriven.Editor
{
    /// <summary>
    /// Utility for editing DataTable in editor script
    /// </summary>
    public static class DataTableEditorUtils
    {
        public static GUIStyle ToolBarButtonStyle => new("LargeButton");
        /// <summary>
        /// Call before dataTable internal update
        /// </summary>
        public static DataTableUpdateDelegate OnDataTablePreUpdate;
        /// <summary>
        /// Call after dataTable internal update
        /// </summary>
        public static DataTableUpdateDelegate OnDataTablePostUpdate;
        /// <summary>
        /// Set row struct type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetRowStruct<T>(DataTable dataTable) where T : class, IDataTableRow
        {
            Undo.RecordObject(dataTable, "Update DataTable Row Struct");
            dataTable.SetRowStruct(typeof(T));
            EditorUtility.SetDirty(dataTable);
        }
        /// <summary>
        /// Set row struct type
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="rowType"></param>
        public static void SetRowStruct(DataTable dataTable, Type rowType)
        {
            Undo.RecordObject(dataTable, "Update DataTable Row Struct");
            dataTable.SetRowStruct(rowType);
            EditorUtility.SetDirty(dataTable);
        }
        /// <summary>
        /// Export dataTable to json
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ExportJson(DataTable dataTable)
        {
            var instance = Object.Instantiate(dataTable);
            instance.InternalUpdate();
            instance.Cleanup();
            return JsonUtility.ToJson(instance);
        }
        /// <summary>
        /// Import json and overwrite dataTable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="jsonData"></param>
        public static void ImportJson(DataTable dataTable, string jsonData)
        {
            Undo.RecordObject(dataTable, "Overwrite DataTable from Json");
            JsonUtility.FromJsonOverwrite(jsonData, dataTable);
            dataTable.InternalUpdate();
            dataTable.Cleanup();
        }
        /// <summary>
        /// Validate input row id and out right one
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oldRowId"></param>
        /// <param name="newRowId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ValidateNewRowId(DataTable dataTable, string oldRowId, string newRowId, out string result)
        {
            if (dataTable.GetRowMap().ContainsKey(newRowId))
            {
                result = oldRowId;
                return false;
            }
            result = newRowId;
            return true;
        }
        /// <summary>
        /// Get a valid new row id
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get all data rows as map with RowId as key without modify default object
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, IDataTableRow> GetRowMapSafe(DataTable dataTable)
        {
            return dataTable.GetRowMapSafe();
        }
        /// <summary>
        /// Clear editor object cache.
        /// </summary>
        /// <remarks>
        /// When use version control, update object handle will let file checkout.
        /// So cleanup after editor completed use.
        /// </remarks>
        public static void Cleanup(DataTable dataTable)
        {
            dataTable.Cleanup();
        }
    }
}