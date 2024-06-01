using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
namespace Kurisu.Framework.Mod
{
    public static class ModAPI
    {
        public static ReactiveProperty<bool> IsModInit { get; } = new(false);
        public static Subject<Unit> OnModRefresh { get; } = new();
        private static readonly List<ModInfo> modInfos = new();
        private static ModSetting setting;
        /// <summary>
        /// Initialize all mods
        /// </summary>
        /// <param name="modSetting"></param>
        /// <param name="modImporter"></param>
        /// <returns></returns>
        public static async UniTask<bool> Initialize(ModSetting modSetting, IModImporter modImporter = default)
        {
            if (IsModInit.Value)
            {
                Debug.LogError("[Mod API] Mod api is already initialized");
                return false;
            }
            Debug.Log("[Mod API] Initialize mod api...");
            modImporter ??= new ModImporter(modSetting, new ModValidator(ImportConstants.APIVersion));
            setting = modSetting;
            if (await modImporter.LoadAllModsAsync(modInfos))
            {
                setting.stateInfos.RemoveAll(x => !modInfos.Any(y => y.FullName == x.modFullName));
                IsModInit.Value = true;
                return true;
            }
            return false;
        }
        public static void DeleteMod(ModInfo modInfo)
        {
            if (!IsModInit.Value)
            {
                Debug.LogError("[Mod API] Mod api is not initialized");
                return;
            }
            setting.DelateMod(modInfo);
            modInfos.Remove(modInfo);
            OnModRefresh.OnNext(Unit.Default);
        }
        public static void EnabledMod(ModInfo modInfo, bool isEnabled)
        {
            if (!IsModInit.Value)
            {
                Debug.LogError("[Mod API] Mod api is not initialized");
                return;
            }
            setting.SetModEnabled(modInfo, isEnabled);
            OnModRefresh.OnNext(Unit.Default);
        }
        public static List<ModInfo> GetAllInfos()
        {
            if (!IsModInit.Value)
            {
                Debug.LogError("[Mod API] Mod api is not initialized");
                return new();
            }
            return modInfos.ToList();
        }
    }
}