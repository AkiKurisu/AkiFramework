using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
namespace Kurisu.Framework.Mod.Editor
{
    [CreateAssetMenu(fileName = "MultiGroupBuilder", menuName = "Mod/Builder/MultiGroupBuilder")]
    public class MultiGroupBuilder : CustomBuilder
    {
        public override string Description => "Include multi groups into build. Additional groups' name should start with main group's name.";
        public override void Build(ModExportConfig exportConfig, string buildPath)
        {
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group.Name.StartsWith(exportConfig.Group.Name))
                    group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = true;
            }
        }

        public override void Cleanup(ModExportConfig exportConfig)
        {
            foreach (var group in AddressableAssetSettingsDefaultObject.Settings.groups)
            {
                if (group.Name.StartsWith(exportConfig.Group.Name))
                    group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = false;
            }
        }
    }
}