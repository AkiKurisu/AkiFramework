using UnityEngine;
namespace Kurisu.Framework
{
    /// <summary>
    /// Generic singleton, use as little as possible
    /// If you want to create a manager only exists in specific scenarios, i recommend you using <see cref="GameRoot"/>
    /// Else if the manager is always exist during lifeTime, you can try use <see cref="RuntimeAnchorBase{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<T>();
                return instance;
            }
        }
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
                Destroy(gameObject);
            else
                instance = (T)this;
        }
        public static bool IsInitialized
        {
            get { return instance != null; }
        }
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}