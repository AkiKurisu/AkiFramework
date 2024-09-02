using UnityEngine;
using System.IO;
namespace Kurisu.Framework.Mod.Editor
{
    public class ExportConstants
    {
        private static readonly LazyDirectory exportPath = new(Path.Combine(Path.GetDirectoryName(Application.dataPath), "Export"));
        public static string ExportPath => exportPath.GetPath();
    }
}
