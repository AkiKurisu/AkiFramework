using UnityEngine;
using System.IO;
namespace Kurisu.Framework.Mod.Editor
{
    public class ExportConstants
    {
        public static string ExportPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Export");
    }
}
