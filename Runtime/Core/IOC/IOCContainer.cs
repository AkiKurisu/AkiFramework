using System.Collections.Generic;
using System;
namespace Kurisu.Framework
{
    public class IOCContainer
    {
        private readonly Dictionary<Type, object> instances = new();
        /// <summary>
        /// Register instance
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(T instance)
        {
            var type = typeof(T);
            if (instances.ContainsKey(type))
            {
                instances[type] = instance;
            }
            else
            {
                instances.Add(type, instance);
            }
        }
        /// <summary>
        /// UnRegister instance
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        public void UnRegister<T>(T instance)
        {
            var type = typeof(T);
            if (instances.ContainsKey(type) && instances[type].Equals(instance))
            {
                instances.Remove(type);
            }
        }
        /// <summary>
        /// Get registered instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (instances.TryGetValue(type, out object obj))
            {
                return obj as T;
            }
            return null;
        }
    }
}
