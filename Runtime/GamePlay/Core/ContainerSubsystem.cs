using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
namespace Kurisu.Framework
{
    /// <summary>
    /// World lifetime scope IOC subsystem
    /// </summary>
    [InitializeOnWorldCreate]
    public class ContainerSubsystem : WorldSubsystem
    {
        private readonly IOCContainer container = new();
        private readonly Dictionary<Type, Action<object>> typeCallBackMap = new();
        /// <summary>
        /// Register a callBack when target type instance is registered
        /// </summary>
        /// <param name="callBack"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterCallBack<T>(Action<T> callBack)
        {
            Assert.IsNotNull(callBack, "[ContainerSubsystem] Instance callback is null, which is not expected.");
            var type = typeof(T);
            if (!typeCallBackMap.ContainsKey(type))
            {
                typeCallBackMap[type] = (obj) => callBack((T)obj);
            }
            else
            {
                typeCallBackMap[type] += (obj) => callBack((T)obj);
            }
        }
        /// <summary>
        /// Register target type instance
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(T instance)
        {
            container.Register(instance);
            var type = typeof(T);
            if (typeCallBackMap.TryGetValue(type, out Action<object> callBack))
            {
                callBack?.Invoke(instance);
            }
        }
        /// <summary>
        /// Get target type instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T Resolve<T>() where T : class
        {
            return container.Resolve<T>();
        }
        protected override void Release()
        {
            container.Clear();
            typeCallBackMap.Clear();
        }
    }
}