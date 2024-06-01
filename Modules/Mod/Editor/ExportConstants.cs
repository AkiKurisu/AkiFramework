using UnityEngine;
using System.IO;
namespace Kurisu.Framework.Mod.Editor
{
    public class ExportConstants
    {
        public static string ExportPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Export");
        /// <summary>
        /// A token to validate mod API version
        /// </summary>
        public const string APIVersion = "0.1";
        public const string DynamicLoadPath = "{LOCAL_MOD_PATH}";
    }
}
