using System;
using System.Collections.Generic;
using System.Linq;
using Kurisu.Framework.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework.DataDriven
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
        public DataTableRow(string rowId, SerializedObject<IDataTableRow> rowData)
        {
            RowId = rowId;
            RowData = rowData;
        }
    }
    [CreateAssetMenu(fileName = "DataTable", menuName = "AkiFramework/DataTable")]
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
            Assert.IsTrue(m_rowType.GetObjectType() == typeof(T));
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
        /// Get data row from table
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IDataTableRow GetRow(int index)
        {
            return m_rows[index].RowData.GetObject();
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
        /// Remove data rows from the table
        /// </summary>
        /// <param name="rowIndexs"></param>
        public void RemoveRow(List<int> rowIndexs)
        {
            m_rows = m_rows.Where((x, id) => !rowIndexs.Contains(id)).ToArray();
        }
        /// <summary>
        /// Get all data rows as map with RowId as key
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IDataTableRow> GetRowMap()
        {
            return m_rows.ToDictionary(x => x.RowId, x => x.RowData.GetObject());
        }
        public string ExportJson()
        {
            return JsonUtility.ToJson(this);
        }
        public void ImportJson(string jsonData)
        {
            JsonUtility.FromJsonOverwrite(jsonData, this);
        }
        #region Internal API
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
        internal void InternalUpdate()
        {
            for (int i = 0; i < m_rows.Length; ++i)
            {
                m_rows[i].RowData.serializedTypeString = m_rowType.serializedTypeString;
            }
        }
        internal bool InsertRow(int index, string RowName, IDataTableRow row)
        {
            var rowKeys = m_rows.Select(x => x.RowId).ToList();
            if (rowKeys.Contains(RowName))
            {
                return false;
            }
            ArrayUtils.Insert(ref m_rows, index, new DataTableRow(RowName, row));
            return true;
        }
        internal void ReorderRow(int fromIndex, int toIndex)
        {
            ArrayUtils.Reorder(ref m_rows, fromIndex, toIndex);
        }
        #endregion
    }
}