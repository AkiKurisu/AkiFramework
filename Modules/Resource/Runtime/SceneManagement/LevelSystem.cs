using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
namespace Kurisu.Framework.Resource
{
    public class LevelReference
    {
        public string Name => Scenes.Length > 0 ? Scenes[0].LevelName : string.Empty;
        public AddressableScene[] Scenes;
    }
    public static class LevelSystem
    {
        internal static HashSet<LevelConfig> managedConfigs = new();
        public static LevelReference EmptyLevel = new() { Scenes = new AddressableScene[0] };
        public static LevelReference LastLevel { get; private set; } = EmptyLevel;
        public static LevelReference CurrentLevel { get; private set; } = EmptyLevel;
        private static SceneInstance mainScene;
        public async static UniTask LoadAsync(LevelReference reference)
        {
            LastLevel = CurrentLevel;
            CurrentLevel = reference;
            // First check has single load scene
            var singleScene = reference.Scenes.FirstOrDefault(x => x.LoadMode == LoadLevelMode.Single);
            bool hasDynamicScene = reference.Scenes.Any(x => x.LoadMode == LoadLevelMode.Dynamic);
            if (singleScene == null)
            {
                // Unload current main scene if have no dynamic scene
                if (!hasDynamicScene && !mainScene.Equals(default))
                {
                    await Addressables.UnloadSceneAsync(mainScene).Task;
                }
            }
            else
            {
                mainScene = await Addressables.LoadSceneAsync(singleScene.Reference.Address, UnityEngine.SceneManagement.LoadSceneMode.Single).Task;
            }
            // Parallel for the others
            using var parallel = UniParallel.Get();
            foreach (var scene in reference.Scenes)
            {
                if (scene.LoadMode >= LoadLevelMode.Additive)
                {
                    parallel.Add(Addressables.LoadSceneAsync(scene.Reference.Address, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task.AsUniTask());
                }
            }
            await parallel;
        }
        internal static void RegisterConfig(LevelConfig config)
        {
            managedConfigs.Add(config);
        }
        public static LevelReference FindLevel(string levelName)
        {
            foreach (var config in managedConfigs)
            {
                foreach (var level in config.GetLevelReferences())
                {
                    if (level.Name == levelName)
                    {
                        return level;
                    }
                }
            }
            return EmptyLevel;
        }
    }
}
