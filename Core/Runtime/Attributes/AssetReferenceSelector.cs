using System;
using UnityEngine;
namespace Kurisu.Framework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AssetReferenceSelector : PropertyAttribute
    {
        private readonly Type m_SelectAssetType;
        private readonly string m_ProcessMethod;
        /// <summary>
        /// Asset to select
        /// </summary>
        public Type SelectAssetType => m_SelectAssetType;
        /// <summary>
        /// Reference process method to get customized reference
        /// </summary>
        public string ProcessMethod => m_ProcessMethod;
        public AssetReferenceSelector(Type selectAssetType, string processMethod = null)
        {
            m_SelectAssetType = selectAssetType;
            m_ProcessMethod = processMethod;
        }
    }
}
