namespace Kurisu.Framework
{
    /// <summary>
    /// Interface to initialize through <see cref="GameRoot"/>, implementation should be GameRoot's child 
    /// </summary>
    public interface IInitialize
    {
        /// <summary>
        /// Init before awake
        /// </summary>
        void Init();
    }
}