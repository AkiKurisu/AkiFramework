using System;
using Cysharp.Threading.Tasks;
using Kurisu.Framework.React;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Resource
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AssetReferenceConstraintAttribute : PropertyAttribute
    {
        /// <summary>
        /// Asset type to select
        /// </summary>
        public Type AssetType { get; private set; }
        /// <summary>
        /// Formatter method to get customized address
        /// </summary>
        public string Formatter { get; private set; }
        /// <summary>
        /// Group to register referenced asset, default use AddressableAssetSettingsDefaultObject.Settings.DefaultGroup
        /// </summary>
        public string Group { get; private set; }
        public AssetReferenceConstraintAttribute(Type AssetType = null, string Formatter = null, string Group = null)
        {
            this.AssetType = AssetType;
            this.Formatter = Formatter;
            this.Group = Group;
        }
    }
    /// <summary>
    /// A lightweight asset reference only use address as identifier
    /// </summary>
    [Serializable]
    public struct SoftAssetReference<T> where T : Object
    {
        public string Address;
#if UNITY_EDITOR
        [SerializeField]
        internal T Object;
        [SerializeField]
        internal bool Locked;
#endif
        /// <summary>
        /// Create asset reference from address
        /// </summary>
        /// <param name="address"></param>
        public SoftAssetReference(string address)
        {
            Address = address;
#if UNITY_EDITOR
            Object = null;
            Locked = false;
#endif
        }
        public readonly T Load<FUnregister>(ref FUnregister unregister) where FUnregister : struct, IUnRegister
        {
#if UNITY_EDITOR
            if (Object)
            {
                return Object;
            }
            ResourceSystem.SafeCheck<T>(Address);
#endif
            return ResourceSystem.AsyncLoadAsset<T>(Address).AddTo(ref unregister).WaitForCompletion();

        }
        public readonly async UniTask<T> LoadAsync<TUnregister>(IUnRegister unregister)
        {
#if UNITY_EDITOR
            if (Object)
            {
                return Object;
            }
            await ResourceSystem.SafeCheckAsync<T>(Address);
#endif
            return await ResourceSystem.AsyncLoadAsset<T>(Address).AddTo(unregister);

        }
        public static implicit operator SoftAssetReference<T>(string address)
        {
            return new SoftAssetReference<T>() { Address = address };
        }
        public override readonly string ToString()
        {
            return Address;
        }
    }
    /// <summary>
    /// A lightweight asset reference only use address as identifier
    /// </summary>
    [Serializable]
    public struct SoftAssetReference
    {
        public string Address;
#if UNITY_EDITOR
        [SerializeField]
        internal Object Object;
        [SerializeField]
        internal bool Locked;
#endif
        /// <summary>
        /// Create asset reference from address
        /// </summary>
        /// <param name="address"></param>
        public SoftAssetReference(string address)
        {
            Address = address;
#if UNITY_EDITOR
            Object = null;
            Locked = false;
#endif
        }
        public readonly Object Load<FUnregister>(ref FUnregister unregister) where FUnregister : struct, IUnRegister
        {
#if UNITY_EDITOR
            if (Object)
            {
                return Object;
            }
            ResourceSystem.SafeCheck<Object>(Address);
#endif
            return ResourceSystem.AsyncLoadAsset<Object>(Address).AddTo(ref unregister).WaitForCompletion();

        }
        public readonly async UniTask<Object> LoadAsync<TUnregister>(IUnRegister unregister)
        {
#if UNITY_EDITOR
            if (Object)
            {
                return Object;
            }
            await ResourceSystem.SafeCheckAsync<Object>(Address);
#endif
            return await ResourceSystem.AsyncLoadAsset<Object>(Address).AddTo(unregister);

        }
        public static implicit operator SoftAssetReference(string address)
        {
            return new SoftAssetReference() { Address = address };
        }
        public override readonly string ToString()
        {
            return Address;
        }
    }
}
