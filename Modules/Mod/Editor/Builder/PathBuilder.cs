using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
namespace Kurisu.Framework.Mod.Editor
{
    public class PathBuilder : IModBuilder
    {
        private bool buildRemoteCatalog;
        public void Build(ModExportConfig exportConfig, string buildPath)
        {
            //Force enable remote catalog
            buildRemoteCatalog = AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog;
            AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog = true;
            AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.SetVariableByName(AddressableAssetSettingsDefaultObject.Settings, AddressableAssetSettings.kRemoteBuildPath);
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group.HasSchema<BundledAssetGroupSchema>())
                    group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = false;
            }
            {
                var group = exportConfig.Group;
                group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = true;
                group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "Remote.LoadPath", ExportConstants.DynamicLoadPath);
                group.Settings.profileSettings.SetValue(group.Settings.activeProfileId, "Remote.BuildPath", buildPath);
            }
        }
        public void Cleanup(ModExportConfig exportConfig)
        {
            //Reset build setting
            AddressableAssetSettingsDefaultObject.Settings.BuildRemoteCatalog = buildRemoteCatalog;
            //Exclude all mod groups
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group.HasSchema<BundledAssetGroupSchema>())
                    group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = !group.Name.StartsWith("Mod_");
            }
            {
                var group = exportConfig.Group;
                group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = false;
                var bundles = Directory.GetFiles(Addressables.BuildPath, "*.bundle", SearchOption.AllDirectories);
                var bundleNames = bundles.Select(x => Path.GetFileName(x)).ToList();
                //Copy default bundles to build path
                foreach (string bundleFilePath in bundles)
                {
                    string bundleFileName = Path.GetFileName(bundleFilePath);
                    string destinationFilePath = Path.Combine(exportConfig.lastExportPath, bundleFileName);
                    File.Copy(bundleFilePath, destinationFilePath, true);
                }
                var catalogPath = Directory.GetFiles(exportConfig.lastExportPath, "*.json")[0];
                var catalog = JsonUtility.FromJson<ContentCatalogData>(File.ReadAllText(catalogPath));
                for (int i = 0; i < catalog.InternalIds.Length; ++i)
                {
                    foreach (var bundleName in bundleNames)
                    {
                        if (catalog.InternalIds[i].Contains(bundleName))
                        {
                            catalog.InternalIds[i] = $"{ExportConstants.DynamicLoadPath}/{bundleName}";
                            break;
                        }
                    }
                }
                File.WriteAllText(catalogPath, JsonUtility.ToJson(catalog));
            }
        }
        public void Write(ref ModInfo modInfo) { }
    }
}
