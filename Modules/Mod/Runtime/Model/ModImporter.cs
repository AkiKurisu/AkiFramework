using System.IO;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Text;
using Cysharp.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
namespace Chris.Mod
{
    public interface IModValidator
    {
        bool IsValidAPIVersion(ModInfo modInfo);
    }
    public interface IModImporter
    {
        UniTask<bool> LoadAllModsAsync(List<ModInfo> modInfos);
    }
    public class APIValidator : IModValidator
    {
        private readonly float apiVersion;
        public APIValidator(float apiVersion)
        {
            this.apiVersion = apiVersion;
        }
        public bool IsValidAPIVersion(ModInfo modInfo)
        {
            if (float.TryParse(modInfo.apiVersion, out var version2))
            {
                return version2 >= apiVersion;
            }
            return false;
        }
    }
    public class ModImporter : IModImporter
    {
        private readonly ModSettings modSettingData;
        private readonly IModValidator validator;
        public ModImporter(ModSettings modSettingData, IModValidator validator)
        {
            this.modSettingData = modSettingData;
            this.validator = validator;
        }
        public async UniTask<bool> LoadAllModsAsync(List<ModInfo> modInfos)
        {
            string modPath = modSettingData.LoadingPath;
            if (!Directory.Exists(modPath))
            {
                Directory.CreateDirectory(modPath);
                return true;
            }
            ModAPI.UnZipAll(modPath, true);
            var directories = Directory.GetDirectories(modPath, "*", SearchOption.AllDirectories);
            if (directories.Length == 0)
            {
                return true;
            }
            List<string> configPaths = new();
            List<string> directoryPaths = new();
            foreach (var directory in directories)
            {
                string[] files = Directory.GetFiles(directory, "*.cfg");
                if (files.Length != 0)
                {
                    configPaths.AddRange(files);
                    directoryPaths.Add(directory);
                }
            }
            if (configPaths.Count == 0)
            {
                return true;
            }
            for (int i = configPaths.Count - 1; i >= 0; i--)
            {
                var modInfo = await ModAPI.LoadModInfo(configPaths[i]);
                var state = modSettingData.GetModState(modInfo);
                if (state == ModState.Enabled)
                {
                    modInfos.Add(modInfo);
                }
                else if (state == ModState.Disabled)
                {
                    directoryPaths.RemoveAt(i);
                    modInfos.Add(modInfo);
                    continue;
                }
                else
                {
                    ModAPI.DeleteModFromDisk(modInfo);
                    directoryPaths.RemoveAt(i);
                    continue;
                }

            }
            foreach (var directory in directoryPaths)
            {
                await ModAPI.LoadModCatalogAsync(directory);
            }
            return true;
        }
        public async UniTask<ModInfo> LoadModAsync(ModSettings settingData, string path)
        {
            var configs = Directory.GetFiles(path, "*.cfg");
            if (configs.Length == 0) return null;
            string config = configs[0];
            var modInfo = await ModAPI.LoadModInfo(config);
            var state = settingData.GetModState(modInfo);
            if (state == ModState.Enabled)
            {
                if (!validator.IsValidAPIVersion(modInfo))
                {
                    return modInfo;
                }
            }
            else if (state == ModState.Disabled)
            {
                return modInfo;
            }
            else
            {
                ModAPI.DeleteModFromDisk(modInfo);
                return null;
            }
            await ModAPI.LoadModCatalogAsync(path);
            return modInfo;
        }
    }
}