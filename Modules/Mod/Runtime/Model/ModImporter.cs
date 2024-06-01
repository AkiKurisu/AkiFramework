using System.IO;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Text;
using Cysharp.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
namespace Kurisu.Framework.Mod
{
    public interface IModValidator
    {
        bool IsValidAPIVersion(string version);
    }
    public interface IModImporter
    {
        UniTask<bool> LoadAllModsAsync(List<ModInfo> modInfos);
    }
    public class ModValidator : IModValidator
    {
        private readonly float apiVersion;
        public ModValidator(float apiVersion)
        {
            this.apiVersion = apiVersion;
        }
        public bool IsValidAPIVersion(string version)
        {
            if (float.TryParse(version, out var version2))
            {
                return version2 >= apiVersion;
            }
            return version == apiVersion.ToString();
        }
    }
    public class ModImporter : IModImporter, IDisposable
    {
        private readonly ModSetting modSettingData;
        private readonly IModValidator validator;
        public ModImporter(ModSetting modSettingData, IModValidator validator)
        {
            this.modSettingData = modSettingData;
            this.validator = validator;
        }
        private readonly List<Texture2D> tempTextures = new();
        private static void GetAllDirectories(string rootFolder, List<string> directories)
        {
            string[] subDirectories = Directory.GetDirectories(rootFolder);
            directories.AddRange(subDirectories);
            foreach (var directory in subDirectories)
            {
                GetAllDirectories(directory, directories);
            }
        }
        private static void UnZipAll(string modPath)
        {
            var zips = Directory.GetFiles(modPath, "*.zip", SearchOption.AllDirectories).ToList();
            foreach (var zip in zips)
            {
                ZipWrapper.UnzipFile(zip, Path.GetDirectoryName(zip));
                File.Delete(zip);
            }
        }
        public async UniTask<bool> LoadAllModsAsync(List<ModInfo> modInfos)
        {
            string modPath = modSettingData.LoadingPath;
            if (!Directory.Exists(modPath))
            {
                Directory.CreateDirectory(modPath);
                return true;
            }
            UnZipAll(modPath);
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
                return false;
            }
            for (int i = configPaths.Count - 1; i >= 0; i--)
            {
                var stream = await File.ReadAllTextAsync(configPaths[i]);
                var modInfo = InitModInfo(stream, directoryPaths[i]);
                var state = modSettingData.GetModState(modInfo);
                if (state == ModStateInfo.ModState.Enabled)
                {
                    modInfos.Add(modInfo);
                }
                else if (state == ModStateInfo.ModState.Disabled)
                {
                    directoryPaths.RemoveAt(i);
                    modInfos.Add(modInfo);
                    continue;
                }
                else
                {
                    DelateMod(modInfo);
                    directoryPaths.RemoveAt(i);
                    continue;
                }

            }
            foreach (var directory in directoryPaths)
            {
                await LoadModCatalogAsync(directory);
            }
            return true;
        }
        public static void DelateMod(ModInfo modInfo)
        {
            Directory.Delete(modInfo.DownloadPath, true);
        }
        public async UniTask<ModInfo> LoadModAsync(ModSetting settingData, string path)
        {
            var configs = Directory.GetFiles(path, "*.cfg");
            if (configs.Length == 0) return null;
            string config = configs[0];
            var modInfo = InitModInfo(await File.ReadAllTextAsync(config), path);
            var state = settingData.GetModState(modInfo);
            if (state == ModStateInfo.ModState.Enabled)
            {
                if (!validator.IsValidAPIVersion(modInfo.apiVersion))
                {
                    return modInfo;
                }
            }
            else if (state == ModStateInfo.ModState.Disabled)
            {
                return modInfo;
            }
            else
            {
                DelateMod(modInfo);
                return null;
            }
            await LoadModCatalogAsync(path);
            return modInfo;
        }
        public async static UniTask<bool> LoadModCatalogAsync(string path)
        {
            string catalogPath = Path.Combine(path, "catalog.json");
            if (File.Exists(catalogPath))
            {
                await LoadCatalogAsync(catalogPath, path);
                return true;
            }
            return false;
        }
        private ModInfo InitModInfo(string stream, string path)
        {
            var modInfo = JsonConvert.DeserializeObject<ModInfo>(stream);
            modInfo.DownloadPath = path.Replace(@"\", "/");
            if (modInfo.modIconBytes.Length != 0)
                modInfo.ModIcon = CreateSpriteFromBytes(modInfo.modIconBytes);
            return modInfo;
        }
        private Sprite CreateSpriteFromBytes(byte[] bytes)
        {
            Texture2D texture = new(2, 2);
            texture.LoadImage(bytes);
            tempTextures.Add(texture);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        public static async UniTask LoadCatalogAsync(string catalogPath, string directoryPath)
        {
            catalogPath = catalogPath.Replace(@"\", "/");
            string contentCatalog = File.ReadAllText(catalogPath, Encoding.UTF8);
            File.WriteAllText(catalogPath, contentCatalog.Replace(ImportConstants.DynamicLoadPath, directoryPath.Replace(@"\", "/")), Encoding.UTF8);
            Debug.Log($"Load mod catalog {catalogPath}");
            await Addressables.LoadContentCatalogAsync(catalogPath).ToUniTask();
            File.WriteAllText(catalogPath, contentCatalog, Encoding.UTF8);
        }
        private void ClearAllTempTexture()
        {
            foreach (var texture in tempTextures)
            {
                Object.Destroy(texture);
            }
            tempTextures.Clear();
        }

        public void Dispose()
        {
            ClearAllTempTexture();
        }
    }
}