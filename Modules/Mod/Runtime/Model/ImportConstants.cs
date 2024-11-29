using System.IO;
using UnityEngine;
namespace Chris.Mod
{
        public class ImportConstants
        {
                /// <summary>
                /// API define, update this version to let old mod not be imported
                /// </summary>
                /// TODO: Currently hard code, should use a setting file instead
                public const float APIVersion = 0.1f;
                public const string DynamicLoadPath = "{LOCAL_MOD_PATH}";
#if !UNITY_EDITOR && UNITY_ANDROID
                public static string LoadingPath = Path.Combine(Application.persistentDataPath, "Mods");
#else
                public static string LoadingPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Mods");
#endif
        }
}
