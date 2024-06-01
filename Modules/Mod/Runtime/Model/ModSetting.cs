using System;
using System.Collections.Generic;
namespace Kurisu.Framework.Mod
{
    /// <summary>
    /// Configuration of mod importer
    /// </summary>
    [Serializable]
    public class ModSetting
    {
        public string LoadingPath { get; set; } = ImportConstants.LoadingPath;
        public List<ModStateInfo> stateInfos = new();
        public ModStateInfo.ModState GetModState(ModInfo modInfo)
        {
            if (TryGetStateInfo(modInfo, out var modStateInfo))
            {
                if (modStateInfo.modState == ModStateInfo.ModState.Delate)
                {
                    stateInfos.Remove(modStateInfo);
                }
                return modStateInfo.modState;
            }
            stateInfos.Add(new ModStateInfo()
            {
                modFullName = modInfo.FullName,
                modState = ModStateInfo.ModState.Enabled
            });
            return ModStateInfo.ModState.Enabled;
        }
        public bool IsModActivated(ModInfo modInfo)
        {
            if (TryGetStateInfo(modInfo, out var modStateInfo))
                return modStateInfo.modState == ModStateInfo.ModState.Enabled;
            stateInfos.Add(new ModStateInfo()
            {
                modFullName = modInfo.FullName,
                modState = ModStateInfo.ModState.Enabled
            });
            return true;
        }
        public void DelateMod(ModInfo modInfo, bool force = false)
        {
            if (TryGetStateInfo(modInfo, out var modStateInfo))
            {
                if (force) stateInfos.Remove(modStateInfo);
                else modStateInfo.modState = ModStateInfo.ModState.Delate;
            }
        }
        public void SetModEnabled(ModInfo modInfo, bool isEnabled)
        {
            if (TryGetStateInfo(modInfo, out var modStateInfo))
                modStateInfo.modState = isEnabled ? ModStateInfo.ModState.Enabled : ModStateInfo.ModState.Disabled;
        }
        public bool TryGetStateInfo(ModInfo modInfo, out ModStateInfo modStateInfo)
        {
            foreach (var stateInfo in stateInfos)
            {
                if (stateInfo.modFullName == modInfo.FullName)
                {
                    modStateInfo = stateInfo;
                    return true;
                }
            }
            modStateInfo = null;
            return false;
        }
    }
}