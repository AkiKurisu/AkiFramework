using System.IO;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Text;
namespace Kurisu.Framework.Mod
{
    public class ModImporter : IDisposable
    {
        private readonly ModSetting modSettingData;
        public ModImporter(ModSetting modSettingData)
        {
            this.modSettingData = modSettingData;
        }
        private readonly List<Texture2D> tempTextures = new();
        public static bool IsValidAPIVersion(string version)
        {
            if (float.TryParse(version, out var version2))
            {
                return version2 >= ImportConstants.APIVersion;
            }
            return version == ImportConstants.APIVersion.ToString();
        }
        private static void GetAllDirectories(string rootFolder, List<string> directories)
        {
            string[] subDirectories = Directory.GetDirectories(rootFolder);
            directories.AddRange(subDirectories);
            foreach (var directory in subDirectories)
            {
                GetAllDirectories(directory, directories);
            }
        }
        public async Task<bool> LoadAllModsAsync(List<ModInfo> modInfos)
        {
            string modPath = modSettingData.LoadingPath;
            if (!File.Exists(modPath)) Directory.CreateDirectory(modPath);
            var directories = new List<string>();
            GetAllDirectories(modPath, directories);
            if (directories.Count == 0)
            {
                return false;
            }
            List<string> configPaths = new();
            List<string> directoryPaths = new();
            foreach (var directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".cfg")
                    {
                        configPaths.Add(file);
                        directoryPaths.Add(directory);
                        break;
                    }
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
            var orgFile = modInfo.DownloadPath + ".zip";
            if (File.Exists(orgFile))
            {
                File.Delete(orgFile);
            }
        }
        public async Task<ModInfo> LoadModAsync(ModSetting settingData, string path)
        {
            string config = null;
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".cfg")
                {
                    config = file;
                    break;
                }
            }
            if (config == null) return null;
            var modInfo = InitModInfo(await File.ReadAllTextAsync(config), path);
            var state = settingData.GetModState(modInfo);
            if (state == ModStateInfo.ModState.Enabled)
            {
                if (!IsValidAPIVersion(modInfo.apiVersion))
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
        public async static Task<bool> LoadModCatalogAsync(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".json")
                {
                    await LoadCatalogAsync(file, path);
                    break;
                }
            }
            return true;
        }
        private ModInfo InitModInfo(string stream, string path)
        {
            var modInfo = JsonUtility.FromJson<ModInfo>(stream);
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
        public static async Task LoadCatalogAsync(string catalogPath, string directoryPath)
        {
            catalogPath = catalogPath.Replace(@"\", "/");
            string contentCatalog = File.ReadAllText(catalogPath, Encoding.UTF8);
            File.WriteAllText(catalogPath, contentCatalog.Replace(ImportConstants.DynamicLoadPath, directoryPath.Replace(@"\", "/")), Encoding.UTF8);
            Debug.Log($"Load mod catalog {catalogPath}");
            await Addressables.LoadContentCatalogAsync(catalogPath).Task;
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