using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Kurisu.Framework.Editor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using Kurisu.Framework.Serialization;
using UnityEngine.AddressableAssets;
namespace Kurisu.Framework.Resource.Editor
{
    public abstract class AssetReferenceDrawer : PropertyDrawer
    {
        private const string AddressPropertyName = "Address";
        private const string GuidPropertyName = "Guid";
        private const string LockPropertyName = "Locked";
        private static readonly GUIContent LockedContent = new("", "when address is in locked, validation will prefer to use Object value");
        private static readonly GUIStyle LockedStyle = new("IN LockButton");
        protected abstract Type GetAssetType();
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get generic constraint type
            AddressableAssetGroup assetGroup = AddressableAssetSettingsDefaultObject.Settings.DefaultGroup;
            Type assetType = GetAssetType();

            string processMethod = null;
            var constraint = fieldInfo.GetCustomAttribute<AssetReferenceConstraintAttribute>(false);
            if (constraint != null)
            {
                // Attribute constraint should match generic constraint
                if (constraint.AssetType != null && constraint.AssetType.IsSubclassOf(assetType))
                    assetType = constraint.AssetType;

                processMethod = constraint.Formatter;
                if (!string.IsNullOrEmpty(constraint.Group))
                {
                    var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(constraint.Group); ;
                    if (group) assetGroup = group;
                }
            }
            var addressProp = property.FindPropertyRelative(AddressPropertyName);
            var guidProp = property.FindPropertyRelative(GuidPropertyName);
            var lockProp = property.FindPropertyRelative(LockPropertyName);

            // Get cache asset from hard reference
            UObject Object = SoftAssetReferenceEditorUtils.GetAssetFromGUID(guidProp.stringValue);

            // TODO: Only need validate once
            ValidateAddress();

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            Object = EditorGUI.ObjectField(position, label, Object, assetType, false);
            if (EditorGUI.EndChangeCheck())
            {
                if (Object)
                {
                    GetOrCreateObjectAddress();
                }
                else
                {
                    addressProp.stringValue = string.Empty;
                    guidProp.stringValue = string.Empty;
                }
            }
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.width -= 30;
            GUI.enabled = !lockProp.boolValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, addressProp, GUIContent.none, true);
            GUI.enabled = true;
            position.x += position.width + 10;
            position.width = 20;
            lockProp.boolValue = GUI.Toggle(position, lockProp.boolValue, LockedContent, LockedStyle);
            if (EditorGUI.EndChangeCheck())
            {
                Object = ResourceEditorExtension.FindAssetEntry(addressProp.stringValue, assetType)?.MainAsset;
                guidProp.stringValue = Object.GetAssetGUID();
            }
            EditorGUI.EndProperty();

            void ValidateAddress()
            {
                if (!Object && !string.IsNullOrEmpty(addressProp.stringValue))
                {
                    Object = ResourceEditorExtension.FindAssetEntry(addressProp.stringValue, assetType)?.MainAsset;
                    guidProp.stringValue = Object.GetAssetGUID();
                }
                else if (Object && string.IsNullOrEmpty(addressProp.stringValue))
                {
                    // when reference is in locked mode, prefer to Object value
                    if (lockProp.boolValue)
                    {
                        GetOrCreateObjectAddress();
                    }
                    else
                    {
                        Object = null;
                        guidProp.stringValue = string.Empty;
                    }
                }
            }
            void GetOrCreateObjectAddress()
            {
                // Alreay exists => use entry current address, ensure to not effect other references
                string path = AssetDatabase.GetAssetPath(Object);
                var existingEntry = Object.ToAddressableAssetEntry();
                if (existingEntry != null)
                {
                    addressProp.stringValue = existingEntry.address;
                    guidProp.stringValue = Object.GetAssetGUID();
                }
                else
                {
                    // Is new => format address and register it
                    if (string.IsNullOrEmpty(processMethod))
                    {
                        addressProp.stringValue = path;
                    }
                    else
                    {
                        object target = ReflectionUtility.GetTargetObjectWithProperty(property);
                        var method = target.GetType()
                                            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                            .Where(m => m.GetParameters().Length == 1)
                                            .Where(x => x.Name == processMethod)
                                            .FirstOrDefault();
                        if (method != null)
                            addressProp.stringValue = (string)method.Invoke(target, new object[1] { Object });
                        else
                            addressProp.stringValue = path;
                    }
                    guidProp.stringValue = Object.GetAssetGUID();
                    var entry = assetGroup.AddAsset(Object);
                    entry.address = addressProp.stringValue;
                    assetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, false, true);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
    [CustomPropertyDrawer(typeof(SoftAssetReference<>))]
    public class SoftAssetReferenceTDrawer : AssetReferenceDrawer
    {
        protected override Type GetAssetType()
        {
            Type assetType = fieldInfo.FieldType;
            if (assetType.IsArray)
            {
                assetType = assetType.GetElementType();
            }
            return assetType.GetGenericArguments()[0];
        }
    }
    [CustomPropertyDrawer(typeof(SoftAssetReference))]
    public class SoftAssetReferenceDrawer : AssetReferenceDrawer
    {
        protected override Type GetAssetType()
        {
            return typeof(UObject);
        }
    }
    public static class SoftAssetReferenceEditorUtils
    {
        private static readonly Dictionary<string, SoftObjectHandle> refDic = new();

        /// <summary>
        /// Optimized fast api for load asset from guid in editor
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static UObject GetAssetFromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            if (!refDic.TryGetValue(guid, out var handle))
            {
                var uObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(UObject));
                if (uObject)
                {
                    GlobalObjectManager.RegisterObject(uObject, ref handle);
                    refDic[guid] = handle;
                    return uObject;
                }
                return null;
            }
            var cacheObject = handle.GetObject();
            if (cacheObject) return cacheObject;

            GlobalObjectManager.UnregisterObject(handle);
            var newObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(UObject));
            if (newObject)
            {
                GlobalObjectManager.RegisterObject(newObject, ref handle);
                refDic[guid] = handle;
            }
            return newObject;
        }
        public static UObject GetAsset(SoftAssetReference softAssetReference)
        {
            if (string.IsNullOrEmpty(softAssetReference.Address)) return null;
            var asset = GetAssetFromGUID(softAssetReference.Guid);
            if (!asset)
            {
                asset = ResourceSystem.AsyncLoadAsset<UObject>(softAssetReference.Address).WaitForCompletion();
            }
            return asset;
        }
        /// <summary>
        /// Create a soft asset reference from object
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static SoftAssetReference FromObject(UObject asset)
        {
            if (!asset)
            {
                return new SoftAssetReference();
            }
            var reference = new SoftAssetReference() { Guid = asset.GetAssetGUID(), Locked = true };
            var existingEntry = asset.ToAddressableAssetEntry();
            if (existingEntry != null)
            {
                reference.Address = existingEntry.address;
            }
            else
            {
                AddressableAssetGroup assetGroup = AddressableAssetSettingsDefaultObject.Settings.DefaultGroup;
                var entry = assetGroup.AddAsset(asset);
                assetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, false, true);
                reference.Address = entry.address;
            }
            return reference;
        }
        /// <summary>
        /// Create a generic soft asset reference from object
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static SoftAssetReference<T> FromTObject<T>(T asset) where T : UObject
        {
            if (!asset)
            {
                return new SoftAssetReference<T>();
            }
            var reference = new SoftAssetReference<T>() { Guid = asset.GetAssetGUID(), Locked = true };
            var existingEntry = asset.ToAddressableAssetEntry();
            if (existingEntry != null)
            {
                reference.Address = existingEntry.address;
            }
            else
            {
                AddressableAssetGroup assetGroup = AddressableAssetSettingsDefaultObject.Settings.DefaultGroup;
                var entry = assetGroup.AddAsset(asset);
                assetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, false, true);
                reference.Address = entry.address;
            }
            return reference;
        }
        /// <summary>
        /// Move reference object safe in editor
        /// </summary>
        /// <param name="reference"></param>
        public static void MoveSoftReferenceObject(ref SoftAssetReference reference, AddressableAssetGroup group, params string[] labels)
        {
            var uObject = GetAssetFromGUID(reference.Guid);
            if (!uObject) return;
            var newEntry = group.AddAsset(uObject, labels);
            reference.Address = newEntry.address;
        }
    }
    public static class ResourceEditorExtension
    {
        public static AddressableAssetEntry AddAsset(this AddressableAssetGroup group, UObject asset, params string[] labels)
        {
            if (asset == null) return null;
            var guid = asset.GetAssetGUID();
            if (guid == null)
            {
                Debug.LogError($"[Resource Editor] Can't find {asset} !");
                return null;
            }
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, false);
            if (labels != null)
            {
                for (int i = 0; i < labels.Length; i++) entry.SetLabel(labels[i], true, true, false);
            }
            return entry;
        }
        public static AddressableAssetEntry ToAddressableAssetEntry(this UObject asset)
        {
            var entries = new List<AddressableAssetEntry>();
            var assetType = asset.GetType();
            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(entries, false, null,
                                    e =>
                                    {
                                        if (e == null) return false;
                                        var type = AssetDatabase.GetMainAssetTypeAtPath(e.AssetPath);
                                        if (type == null) return false;
                                        return type == assetType || type.IsSubclassOf(assetType);
                                    });
            string path = AssetDatabase.GetAssetPath(asset);
            return entries.FirstOrDefault(x => x.AssetPath == path);
        }
        public static AddressableAssetEntry FindAssetEntry(string address, Type assetType)
        {
            var entries = new List<AddressableAssetEntry>();
            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(entries, false, null,
                                    e =>
                                    {
                                        if (e == null) return false;
                                        var type = AssetDatabase.GetMainAssetTypeAtPath(e.AssetPath);
                                        if (type == null) return false;
                                        return (type == assetType || type.IsSubclassOf(assetType)) && e.address == address;
                                    });
            return entries.FirstOrDefault();
        }

        public static string GetAssetGUID(this UObject asset)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }
    }
}
