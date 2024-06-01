using System;
namespace Kurisu.Framework.Mod
{
    [Serializable]
    public class ModStateInfo
    {
        public enum ModState
        {
            /// <summary>
            /// Mod is installed and loaded
            /// </summary>
            Enabled,
            /// <summary>
            /// Mod is installed but not loaded
            /// </summary>
            Disabled,
            /// <summary>
            /// Mod is installed but wait to delate
            /// </summary>
            Delate
        }
        public string modFullName;
        public ModState modState;
    }
}