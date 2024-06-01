using UnityEngine;
namespace Kurisu.Framework.Mod.Editor
{
    public abstract class CustomBuilder : ScriptableObject, IModBuilder
    {
        public virtual string Description { get; }
        public virtual void Build(ModExportConfig exportConfig, string buildPath)
        {

        }

        public virtual void Cleanup(ModExportConfig exportConfig)
        {

        }

        public void Write(ref ModInfo modInfo)
        {

        }
    }
}