using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Chris.Mod
{
    /// <summary>
    /// Class defines mod's information
    /// </summary>
    public class ModInfo : IDisposable
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
        public string FilePath { get; set; }
        private Texture2D iconTexture;
        private Sprite iconSprite;
        [JsonIgnore]
        public Sprite ModIcon => iconSprite = iconSprite != null ? iconSprite : CreateSpriteFromBytes(modIconBytes);
        [JsonIgnore]
        public string FullName => modName + '-' + version + '-' + apiVersion;
        private Sprite CreateSpriteFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            iconTexture = new(2, 2);
            iconTexture.LoadImage(bytes);
            return Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.zero);
        }

        public void Dispose()
        {
            if (iconSprite) Object.Destroy(iconSprite);
            if (iconTexture) Object.Destroy(iconTexture);
        }
    }
}