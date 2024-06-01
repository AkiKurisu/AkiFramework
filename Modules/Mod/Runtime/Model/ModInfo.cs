using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Kurisu.Framework.Mod
{
    [Serializable]
    public class ModInfo : ISerializationCallbackReceiver
    {
        #region Serialized Field
        public string apiVersion;
        public string authorName;
        public string modName;
        public string version;
        public string description;
        public byte[] modIconBytes;
        public string[] metaDataIndex;
        public string[] metaDataContent;
        #endregion
        public string DownloadPath { get; set; }
        public Sprite ModIcon { get; set; }
        public string FullName => modName + '-' + version + '-' + apiVersion;
        private Dictionary<string, string> metaData;
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            metaData ??= new();
            metaDataIndex = metaData.Keys.ToArray();
            metaDataContent = metaData.Values.ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            metaData = new();
            metaDataIndex ??= new string[0];
            metaDataContent ??= new string[0];
            for (int i = 0; i < Mathf.Min(metaDataIndex.Length, metaDataContent.Length); ++i)
            {
                metaData.Add(metaDataIndex[i], metaDataContent[i]);
            }
        }
        public void SetMetaData(string index, string content)
        {
            metaData[index] = content;
        }
        public bool TryGetMetaData(string index, out string content)
        {
            return metaData.TryGetValue(index, out content);
        }
    }
}