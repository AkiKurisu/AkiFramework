using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
namespace Kurisu.Framework.Mod
{
    /// <summary>
    /// Mod manager class for AkiFramework
    /// </summary>
    public class ModManager : Singleton<ModManager>
    {
        private readonly List<ModInfo> modInfos = new();
        public Subject<Unit> OnModInit { get; } = new();
        public Subject<Unit> OnModRefresh { get; } = new();
        private ModSetting settingData;
        public bool IsModInit { get; private set; }
        private ModImporter modImporter;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            if (settingData == null)
            {
                settingData = SaveUtility.LoadOrNew<ModSetting>();
                modImporter = new(settingData);
            }
        }
        protected override void OnDestroy()
        {
            modImporter.Dispose();
            base.OnDestroy();
        }
        /// <summary>
        /// Load all mods
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> LoadAllMods()
        {
            if (settingData == null)
            {
                settingData = SaveUtility.LoadOrNew<ModSetting>();
                modImporter = new(settingData);
            }
            await modImporter.LoadAllModsAsync(modInfos);
            settingData.stateInfos.RemoveAll(x => !modInfos.Any(y => y.FullName == x.modFullName));
            SaveData();
            IsModInit = true;
            OnModInit.OnNext(Unit.Default);
            return true;
        }

        public bool IsModActivated(ModInfo modInfo)
        {
            if (!ModImporter.IsValidAPIVersion(modInfo.apiVersion)) return false;
            return settingData.IsModActivated(modInfo);
        }
        public void DeleteMod(ModInfo modInfo)
        {
            settingData.DelateMod(modInfo);
            SaveData();
            modInfos.Remove(modInfo);
            OnModRefresh.OnNext(Unit.Default);
        }
        public void EnabledMod(ModInfo modInfo, bool isEnabled)
        {
            settingData.SetModEnabled(modInfo, isEnabled);
            SaveData();
            OnModRefresh.OnNext(Unit.Default);
        }
        public List<ModInfo> GetModInfos()
        {
            return modInfos.ToList();
        }
        private void SaveData()
        {
            SaveUtility.Save(settingData);
        }
    }
}
