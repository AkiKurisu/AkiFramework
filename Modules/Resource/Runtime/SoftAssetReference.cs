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
        /// <summary>
        /// Enable to move asset entry to defined group if already in other asset group
        /// </summary>
        /// <value></value>
        public bool ForceGroup { get; private set; }
        public AssetReferenceConstraintAttribute(Type AssetType = null, string Formatter = null, string Group = null, bool ForceGroup = false)
        {
            this.AssetType = AssetType;
            this.Formatter = Formatter;
            this.Group = Group;
            this.ForceGroup = ForceGroup;
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
        internal string Guid;
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
            Guid = string.Empty;
            Locked = false;
#endif
        }

        public readonly T Load<FUnregister>(ref FUnregister unregister) where FUnregister : struct, IDisposableUnregister
        {
            return ResourceSystem.LoadAssetAsync<T>(Address).AddTo(ref unregister).WaitForCompletion();

        }

        public readonly T Load<FUnregister>(IDisposableUnregister unregister)
        {
            return ResourceSystem.LoadAssetAsync<T>(Address).AddTo(unregister).WaitForCompletion();
        }

        public readonly async UniTask<T> LoadAsync<TUnregister>(IDisposableUnregister unregister)
        {
            return await ResourceSystem.LoadAssetAsync<T>(Address).AddTo(unregister);
        }

        public readonly ResourceHandle<T> LoadAsync()
        {
            return ResourceSystem.LoadAssetAsync<T>(Address);
        }

        public static implicit operator SoftAssetReference<T>(string address)
        {
            return new SoftAssetReference<T>() { Address = address };
        }

        public static implicit operator SoftAssetReference<T>(SoftAssetReference assetReference)
        {
            return new SoftAssetReference<T>()
            {
                Address = assetReference.Address,
#if UNITY_EDITOR
                Guid = assetReference.Guid,
                Locked = assetReference.Locked
#endif
            };
        }

        public static implicit operator SoftAssetReference(SoftAssetReference<T> assetReference)
        {
            return new SoftAssetReference()
            {
                Address = assetReference.Address,
#if UNITY_EDITOR
                Guid = assetReference.Guid,
                Locked = assetReference.Locked
#endif
            };
        }
        public override readonly string ToString()
        {
            return Address;
        }
        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(Address);
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
        internal string Guid;
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
            Guid = string.Empty;
            Locked = false;
#endif
        }
        public readonly Object Load<FUnregister>(ref FUnregister unregister) where FUnregister : struct, IDisposableUnregister
        {
            return ResourceSystem.LoadAssetAsync<Object>(Address).AddTo(ref unregister).WaitForCompletion();
        }

        public readonly Object Load<FUnregister>(IDisposableUnregister unregister)
        {
            return ResourceSystem.LoadAssetAsync<Object>(Address).AddTo(unregister).WaitForCompletion();
        }

        public readonly async UniTask<Object> LoadAsync<TUnregister>(IDisposableUnregister unregister)
        {
            return await ResourceSystem.LoadAssetAsync<Object>(Address).AddTo(unregister);
        }

        public readonly ResourceHandle LoadAsync()
        {
            return ResourceSystem.LoadAssetAsync<Object>(Address);
        }

        public static implicit operator SoftAssetReference(string address)
        {
            return new SoftAssetReference() { Address = address };
        }

        public override readonly string ToString()
        {
            return Address;
        }

        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(Address);
        }
    }
}
