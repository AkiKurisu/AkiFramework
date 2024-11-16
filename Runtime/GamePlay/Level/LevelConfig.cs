using System;
using System.Collections.Generic;
using System.Linq;
using Kurisu.Framework.Resource;
using UnityEngine;
namespace Kurisu.Framework.Level
{
    public enum LoadLevelMode
    {
        Single,
        Additive,
        Dynamic
    }
    [Flags]
    public enum LoadLevelPolicy
    {
        Never = 0,
        PC = 2,
        Mobile = 4,
        Console = 8,
        AllPlatform = PC | Mobile | Console
    }
    [Serializable]
    public class AddressableScene
    {
        [PopupSelector(typeof(LevelConfig))]
        public string LevelName;
        /// <summary>
        /// Soft reference (address) to the real scene asset
        /// </summary>
#if UNITY_EDITOR
        [AssetReferenceConstraint(typeof(UnityEditor.SceneAsset))]
#endif
        public SoftAssetReference Reference;
        public LoadLevelMode LoadMode;
        public LoadLevelPolicy LoadPolicy = LoadLevelPolicy.AllPlatform;
        internal bool ValidateLoadPolicy()
        {
            if (Application.isMobilePlatform) return LoadPolicy.HasFlag(LoadLevelPolicy.Mobile);
            if (Application.isConsolePlatform) return LoadPolicy.HasFlag(LoadLevelPolicy.Console);
            return LoadPolicy.HasFlag(LoadLevelPolicy.PC);
        }
    }
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "AkiFramework/Resource/LevelConfig")]
    public class LevelConfig : PopupSet
    {
        [SerializeField]
        private AddressableScene[] scenes;
        private LevelReference[] references;
        public LevelReference[] GetLevelReferences()
        {
            if (references != null) return references;
            var dict = new Dictionary<string, List<AddressableScene>>();
            foreach (var scene in scenes)
            {
                // Whether can load in current platform
                if (!scene.ValidateLoadPolicy()) continue;

                if (!dict.TryGetValue(scene.LevelName, out var cache))
                {
                    cache = dict[scene.LevelName] = new();
                }
                cache.Add(scene);
            }
            return references = dict.Select(x => new LevelReference()
            {
                Scenes = x.Value.ToArray()
            }).ToArray();
        }
        private void Awake()
        {
            LevelSystem.RegisterConfig(this);
        }
    }
}