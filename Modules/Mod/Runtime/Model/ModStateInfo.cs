using System;
namespace Chris.Mod
{
    /// <summary>
    /// Status of installed mod
    /// </summary>
    public enum ModState
    {
        /// <summary>
        /// Mod is loaded
        /// </summary>
        Enabled,
        /// <summary>
        /// Mod is not loaded
        /// </summary>
        Disabled,
        /// <summary>
        /// Mod waits to be deleted (will be deleted on next launch)
        /// </summary>
        Delate
    }
    [Serializable]
    public class ModStateInfo
    {
        public string modFullName;
        public ModState modState;
    }
}