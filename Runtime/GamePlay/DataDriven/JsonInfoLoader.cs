using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Chris.Resource;
using Newtonsoft.Json;
using UnityEngine;
namespace Chris.DataDriven
{
    /// <summary>
    /// Load text info in json format
    /// </summary>
    public static class JsonInfoLoader
    {
        public static async UniTask<List<T>> LoadInfosAsync<T>(string label)
        {
            using var handle = ResourceSystem.LoadAssetsAsync<TextAsset>(label);
            IList<TextAsset> assets = await handle;
            List<T> infos = new();
            foreach (var asset in assets)
            {
                infos.Add(JsonConvert.DeserializeObject<T>(asset.text));
            }
            return infos;
        }
        
        public static List<T> LoadInfos<T>(string label)
        {
            using var handle = ResourceSystem.LoadAssetsAsync<TextAsset>(label);
            IList<TextAsset> assets = handle.WaitForCompletion();
            List<T> infos = new();
            foreach (var asset in assets)
            {
                infos.Add(JsonConvert.DeserializeObject<T>(asset.text));
            }
            return infos;
        }

        public static async UniTask<List<T>> LoadInfosAsync<T>(IEnumerable labels, ResourceSystem.MergeMode mergeMode = ResourceSystem.MergeMode.Intersection)
        {
            using var handle = ResourceSystem.LoadAssetsAsync<TextAsset>(labels, mergeMode);
            IList<TextAsset> assets = await handle;
            List<T> infos = new();
            foreach (var asset in assets)
            {
                infos.Add(JsonConvert.DeserializeObject<T>(asset.text));
            }
            return infos;
        }
        
        public static List<T> LoadInfos<T>(IEnumerable labels, ResourceSystem.MergeMode mergeMode = ResourceSystem.MergeMode.Intersection)
        {
            using var handle = ResourceSystem.LoadAssetsAsync<TextAsset>(labels, mergeMode);
            IList<TextAsset> assets = handle.WaitForCompletion();
            List<T> infos = new();
            foreach (var asset in assets)
            {
                infos.Add(JsonConvert.DeserializeObject<T>(asset.text));
            }
            return infos;
        }
    }
}
