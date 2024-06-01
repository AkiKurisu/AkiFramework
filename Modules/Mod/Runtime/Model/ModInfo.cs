using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Kurisu.Framework.Mod
{
    public class ModInfo
    {
        #region Serialized Field
        public string apiVersion;
        public string authorName;
        public string modName;
        public string version;
        public string description;
        public byte[] modIconBytes;
        public Dictionary<string, string> metaData = new();
        #endregion
        [JsonIgnore]
        public string DownloadPath { get; set; }
        [JsonIgnore]
        public Sprite ModIcon { get; set; }
        [JsonIgnore]
        public string FullName => modName + '-' + version + '-' + apiVersion;
    }
}