using System;
using UnityEngine;
using System.Collections.Generic;
namespace Kurisu.Framework
{
    /// <summary>
    /// Scene scope IOC container, also init all children implement <see cref="IInitialize"/>
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot instance;
        private static GameRoot Instance => instance != null ? instance : GetInstance();
        private IOCContainer container;
        private readonly Dictionary<Type, Action<object>> typeCallBackMap = new();
        private static GameRoot GetInstance()
        {
            instance = FindObjectOfType<GameRoot>();
            if (instance == null)
            {
                Debug.Log("Can not find Game Root !");
            }
            return instance;
        }
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            InitChildren();
        }
        private void InitChildren()
        {
            IInitialize[] children = GetComponentsInChildren<IInitialize>();
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Init();
            }
        }
        /// <summary>
        /// Register a callBack when target type instance is registered
        /// </summary>
        /// <param name="callBack"></param>
        /// <typeparam name="T"></typeparam>
        public static void RegisterCallBack<T>(Action<T> callBack)
        {
            var type = typeof(T);
            if (!Instance.typeCallBackMap.ContainsKey(type))
            {
                Instance.typeCallBackMap[type] = (obj) => callBack?.Invoke((T)obj);
            }
            else
            {
                Instance.typeCallBackMap[type] += (obj) => callBack?.Invoke((T)obj);
            }
        }
        /// <summary>
        /// Register target type instance
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>(T instance)
        {
            Instance.container ??= new IOCContainer();
            Instance.container.Register(instance);
            var type = typeof(T);
            if (Instance.typeCallBackMap.TryGetValue(type, out Action<object> callBack))
            {
                callBack?.Invoke(instance);
            }
        }
        /// <summary>
        /// Get target type instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T Resolve<T>() where T : class
        {
            return Instance.container.Resolve<T>();
        }
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}