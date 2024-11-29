using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using Newtonsoft.Json;
namespace Chris.Mod.Editor
{
    public class ModExporter
    {
        public readonly List<IModBuilder> builders;
        public readonly ModExportConfig exportConfig;
        public ModExporter(ModExportConfig exportConfig)
        {
            this.exportConfig = exportConfig;
            builders = new List<IModBuilder>
            {
                new PathBuilder(),
            };
            builders.AddRange(exportConfig.customBuilders);
        }
        private static string CreateBuildPath(string modName)
        {
            var targetPath = Path.Combine(ExportConstants.ExportPath, EditorUserBuildSettings.activeBuildTarget.ToString());
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            var buildPath = Path.Combine(targetPath, modName.Replace(" ", string.Empty));
            if (Directory.Exists(buildPath)) FileUtil.DeleteFileOrDirectory(buildPath);
            Directory.CreateDirectory(buildPath);
            return buildPath;
        }
        public bool Export()
        {
            string buildPath = exportConfig.lastExportPath = CreateBuildPath(exportConfig.modName);
            BuildPipeline(buildPath);
            WritePipeline(buildPath);
            if (BuildContent())
            {
                string achievePath = buildPath + ".zip";
                if (!ZipTogether(buildPath, achievePath))
                {
                    LogError($"Zip failed!");
                    return false;
                }
                Directory.Delete(buildPath, true);
                Log($"Export succeed, export path: {achievePath}");
                return true;
            }
            else
            {
                LogError($"Build pipeline failed!");
                return false;
            }
        }
        private static void LogError(string message)
        {
            Debug.LogError($"<color=#ff2f2f>Exporter</color>: {message}");
        }
        private static void Log(string message)
        {
            Debug.Log($"<color=#3aff48>Exporter</color>: {message}");
        }
        private static bool ZipTogether(string buildPath, string zipPath)
        {
            return ZipWrapper.Zip(new string[1] { buildPath }, zipPath);
        }
        private void WritePipeline(string buildPath)
        {
            var info = new ModInfo
            {
                authorName = exportConfig.authorName,
                description = exportConfig.description,
                modName = exportConfig.modName,
                version = exportConfig.version,
                modIconBytes = exportConfig.modIcon != null ? exportConfig.modIcon.EncodeToPNG() : new byte[0] { },
                apiVersion = ImportConstants.APIVersion.ToString()
            };
            foreach (var builder in builders)
            {
                builder.Write(ref info);
            }
            var stream = JsonConvert.SerializeObject(info);
            File.WriteAllText(buildPath + "/ModConfig.cfg", stream);
        }
        private bool BuildContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            CleanupPipeline();
            return string.IsNullOrEmpty(result.Error);
        }
        private void BuildPipeline(string dynamicBuildPath)
        {
            foreach (var builder in builders)
            {
                builder.Build(exportConfig, dynamicBuildPath);
            }
        }
        private void CleanupPipeline()
        {
            foreach (var builder in builders)
            {
                builder.Cleanup(exportConfig);
            }
        }
    }
}
