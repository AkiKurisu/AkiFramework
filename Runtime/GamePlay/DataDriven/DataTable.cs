using System;
using System.Collections.Generic;
using System.Linq;
using Kurisu.Framework.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.Framework.DataDriven
{
    public interface IDataTableRow { }
    [CreateAssetMenu(fileName = "DataTable", menuName = "AkiFramework/DataTable")]
    public class DataTable : ScriptableObject
    {
        [SerializeField]
        private SerializedType<IDataTableRow> m_rowType;
        [SerializeField]
        private SerializedObject<IDataTableRow>[] m_rows;
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
            return m_rows.Select(x => x.GetObject() as T).ToArray();
        }
        /// <summary>
        /// Get data rows from table
        /// </summary>
        /// <returns></returns>
        public IDataTableRow[] GetAllRows()
        {
            return m_rows.Select(x => x.GetObject()).ToArray();
        }
        public T GetRow<T>(int index) where T : class, IDataTableRow
        {
            return m_rows[index].GetObject() as T;
        }
        public IDataTableRow GetRow(int index)
        {
            return m_rows[index].GetObject();
        }
        public void AddRow(IDataTableRow row)
        {
            var serializedRow = SerializedObject<IDataTableRow>.FromObject(row);
            ArrayUtils.Add(ref m_rows, serializedRow);
        }
        public void InsertRow(int index, IDataTableRow row)
        {
            var serializedRow = SerializedObject<IDataTableRow>.FromObject(row);
            ArrayUtils.Insert(ref m_rows, index, serializedRow);
        }
        public void SwarpRow(int fromIndex, int toIndex)
        {
            ArrayUtils.Swarp(ref m_rows, fromIndex, toIndex);
        }
        public void RemoveRow(List<int> rowIndexs)
        {
            m_rows = m_rows.Where((x, id) => !rowIndexs.Contains(id)).ToArray();
        }
        internal void InternalUpdate()
        {
            for (int i = 0; i < m_rows.Length; ++i)
            {
                m_rows[i].serializedTypeString = m_rowType.serializedTypeString;
            }
        }
    }
}