using Cysharp.Threading.Tasks;
using R3;
namespace Kurisu.Framework.Mod
{
    /// <summary>
    /// AF's default mod manager
    /// </summary>
    public class ModManager : Singleton<ModManager>
    {
        private ModSetting settingData;
        private bool isInitialized;
        private ModImporter modImporter;
        private IModValidator modValidator;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            if (!isInitialized)
            {
                LocalInitialize();
            }
        }
        private void LocalInitialize()
        {
            ModAPI.OnModRefresh.Subscribe(_ => SaveData()).AddTo(destroyCancellationToken);
            ModAPI.IsModInit.Subscribe(_ => SaveData()).AddTo(destroyCancellationToken);
            settingData = SaveUtility.LoadOrNew<ModSetting>();
            modImporter = new(settingData, modValidator = new ModValidator(ImportConstants.APIVersion));
            isInitialized = true;
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
        public async UniTask<bool> Initialize()
        {
            if (!isInitialized)
            {
                LocalInitialize();
            }
            //Skip if is initialized, use single mod import instead.
            if (ModAPI.IsModInit.Value) return false;
            return await ModAPI.Initialize(settingData, modImporter);
        }
        public bool IsModActivated(ModInfo modInfo)
        {
            if (!modValidator.IsValidAPIVersion(modInfo.apiVersion)) return false;
            return settingData.IsModActivated(modInfo);
        }
        private void SaveData()
        {
            SaveUtility.Save(settingData);
        }
    }
}
