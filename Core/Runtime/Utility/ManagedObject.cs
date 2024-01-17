#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Kurisu.Framework.Utility.ManagedObjects
{

    /// <summary>
    /// This base class serves as a trick to be able to reset some values on ScriptableObjects when the PlayState changes in the editor.
    /// The behaviour is called both when entering and exiting play mode.<br />
    /// This is not necessary in the build, in which the OnDisable is simply called.
    /// Notice that in the build we don't call it with OnEnable, to prevent timing issues with other OnEnables which might be adding to this SO.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public abstract class ManagedObject : DescriptiveScriptableObject
    {
        abstract protected void OnReset();

#if UNITY_EDITOR
        /// <summary>
        /// This will be called when the object is inspected for the first time
        /// or when the object is loaded for the first time when entering PlayMode.
        /// </summary>
        protected void Awake()
        {
            OnReset();
        }

        protected void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayStateChange;
        }

        protected void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayStateChange;
        }

        private void OnPlayStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
                OnReset();
        }
#else
        protected void OnDisable()
        {
            OnReset();
        }
#endif
    }
}