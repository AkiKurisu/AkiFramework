namespace Kurisu.Framework.IOC
{
    /// <summary>
    /// Interface to initialize through <see cref="SceneScopeContainer"/>, implementation should be GameRoot's child 
    /// </summary>
    public interface IInitialize
    {
        /// <summary>
        /// Init before awake
        /// </summary>
        void Init();
    }
}