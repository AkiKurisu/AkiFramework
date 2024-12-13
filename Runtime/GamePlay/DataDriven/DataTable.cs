using System;
using System.Collections.Generic;
using System.Linq;
using Chris.Collections;
using Chris.Serialization;
using UnityEngine;
namespace Chris.DataDriven
{
    public interface IDataTableRow { }
    [Serializable]
    internal class DataTableRow
    {
        public string RowId;
        public SerializedObject<IDataTableRow> RowData;
        public DataTableRow(string rowId, IDataTableRow row)
        {
            RowId = rowId;
            RowData = SerializedObject<IDataTableRow>.FromObject(row);
        }
    }
    [CreateAssetMenu(fileName = "DataTable", menuName = "Chris/DataTable")]
    public class DataTable : ScriptableObject
    {
        [SerializeField]
        private SerializedType<IDataTableRow> m_rowType;
        [SerializeField]
        private DataTableRow[] m_rows;
        /// <summary>
        /// Get default row struct
        /// </summary>
        /// <returns></returns>
        public IDataTableRow GetRowStruct()
        {
            return m_rowType.GetObject();
        }
        /// <summary>
        /// Get row struct type
        /// </summary>
        /// <returns></returns>
        public Type GetRowStructType()
        {
            return m_rowType.GetObjectType();
        }
        /// <summary>
        /// Get data rows from table
        /// </summary>
        public T[] GetAllRows<T>() where T : class, IDataTableRow
        {
            return m_rows.Select(x => x.RowData.GetObject() as T).ToArray();
        }
        /// <summary>
        /// Get data rows from table
        /// </summary>
        /// <returns></returns>
        public IDataTableRow[] GetAllRows()
        {
            return m_rows.Select(x => x.RowData.GetObject()).ToArray();
        }
        /// <summary>
        /// Get data row from table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T GetRow<T>(int index) where T : class, IDataTableRow
        {
            return m_rows[index].RowData.GetObject() as T;
        }
        /// <summary>
        /// Get data row from table by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IDataTableRow GetRow(int index)
        {
            return m_rows[index].RowData.GetObject();
        }
        /// <summary>
        /// Get data row from table by RowId
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetRow<T>(string rowId) where T : class, IDataTableRow
        {
            foreach (var row in m_rows)
            {
                if (row.RowId == rowId)
                {
                    return row.RowData.GetObject() as T;
                }
            }
            return null;
        }
        /// <summary>
        /// Get data row from table by RowId
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IDataTableRow GetRow(string rowId)
        {
            foreach (var row in m_rows)
            {
                if (row.RowId == rowId)
                {
                    return row.RowData.GetObject();
                }
            }
            return null;
        }
        /// <summary>
        /// Add a data row to the table
        /// </summary>
        /// <param name="RowName"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool AddRow(string RowName, IDataTableRow row)
        {
            var rowKeys = m_rows.Select(x => x.RowId).ToList();
            if (rowKeys.Contains(RowName))
            {
                return false;
            }
            ArrayUtils.Add(ref m_rows, new DataTableRow(RowName, row));
            return true;
        }
        /// <summary>
        /// Update a data row from the table
        /// </summary>
        /// <param name="RowName"></param>
        /// <param name="newRow"></param>
        /// <returns></returns>
        public bool UpdateRow(string RowName, IDataTableRow newRow)
        {
            var internalMap = GetInternalRowMap();
            if (!internalMap.TryGetValue(RowName, out var row))
            {
                return false;
            }
            row.RowData = SerializedObject<IDataTableRow>.FromObject(newRow);
            return true;
        }
        /// <summary>
        /// Remove data rows from the table
        /// </summary>
        /// <param name="rowIndexs"></param>
        public void RemoveRow(List<int> rowIndexs)
        {
            m_rows = m_rows.Where((x, id) => !rowIndexs.Contains(id)).ToArray();
        }
        /// <summary>
        /// Remove all data rows from the table
        /// </summary>
        public void RemoveAllRows()
        {
            m_rows = new DataTableRow[0];
        }
        /// <summary>
        /// Insert a row to dataTable
        /// </summary>
        /// <param name="index"></param>
        /// <param name="RowName"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool InsertRow(int index, string RowName, IDataTableRow row)
        {
            var rowKeys = m_rows.Select(x => x.RowId).ToList();
            if (rowKeys.Contains(RowName))
            {
                return false;
            }
            ArrayUtils.Insert(ref m_rows, index, new DataTableRow(RowName, row));
            return true;
        }
        /// <summary>
        /// Reorder a row
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        public void ReorderRow(int fromIndex, int toIndex)
        {
            ArrayUtils.Reorder(ref m_rows, fromIndex, toIndex);
        }
        /// <summary>
        /// Get all data rows as map with RowId as key
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IDataTableRow> GetRowMap()
        {
            return m_rows.ToDictionary(x => x.RowId, x => x.RowData.GetObject());
        }
        #region Internal API
        /// <summary>
        /// Get editable row map
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, DataTableRow> GetInternalRowMap()
        {
            return m_rows.ToDictionary(x => x.RowId, x => x);
        }
        /// <summary>
        /// Internal use only, since we should ensure incomming row type is valid
        /// </summary>
        /// <param name="rowStructType"></param>
        internal void SetRowStruct(Type rowStructType)
        {
            m_rowType = SerializedType<IDataTableRow>.FromType(rowStructType);
        }
        /// <summary>
        /// Get a valid new row id
        /// </summary>
        /// <returns></returns>
        internal string NewRowId()
        {
            var map = GetRowMap();
            var id = "Row_0";
            int index = 1;
            while (map.ContainsKey(id))
            {
                id = $"Row_{index++}";
            }
            return id;
        }
        /// <summary>
        /// Update dataTable struct and rows
        /// </summary>
        internal void InternalUpdate()
        {
            m_rowType.InternalUpdate();
            for (int i = 0; i < m_rows.Length; ++i)
            {
                m_rows[i].RowData.serializedTypeString = m_rowType.serializedTypeString;
                m_rows[i].RowData.InternalUpdate();
            }
        }
        /// <summary>
        /// Clear editor object cache.
        /// </summary>
        internal void Cleanup()
        {
#if UNITY_EDITOR
            if (m_rows == null) return;
            for (int i = 0; i < m_rows.Length; ++i)
            {
                m_rows[i].RowData.objectHandle = 0;
            }
#endif
        }
        /// <summary>
        /// Get data rows from table without modify default object
        /// </summary>
        internal T[] GetAllRowsSafe<T>() where T : class, IDataTableRow
        {
            return m_rows.Select(x => x.RowData.NewObject() as T).ToArray();
        }
        /// <summary>
        /// Get data rows from table without modify default object
        /// </summary>
        /// <returns></returns>
        internal IDataTableRow[] GetAllRowsSafe()
        {
            return m_rows.Select(x => x.RowData.NewObject()).ToArray();
        }
        /// <summary>
        /// Get all data rows as map with RowId as key without modify default object
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, IDataTableRow> GetRowMapSafe()
        {
            return m_rows.ToDictionary(x => x.RowId, x => x.RowData.NewObject());
        }
        #endregion
    }
}