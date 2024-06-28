using System;
using UnityEngine;
namespace Kurisu.Framework
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AssetReferenceSelector : PropertyAttribute
    {
        /// <summary>
        /// Asset to select
        /// </summary>
        public Type SelectAssetType { get; private set; }
        /// <summary>
        /// Reference process method to get customized reference
        /// </summary>
        public string ProcessMethod { get; private set; }
        /// <summary>
        /// Allow assigning Scene objects
        /// </summary>
        /// <value></value>
        public bool AllowSceneObjects { get; private set; }
        public AssetReferenceSelector(Type selectAssetType, string processMethod = null, bool allowSceneObjects = false)
        {
            SelectAssetType = selectAssetType;
            ProcessMethod = processMethod;
            AllowSceneObjects = allowSceneObjects;
        }
    }
}
