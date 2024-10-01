using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Kurisu.Framework.Editor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using Kurisu.Framework.Serialization.Editor;
namespace Kurisu.Framework.Resource.Editor
{
    public abstract class AssetReferenceDrawer : PropertyDrawer
    {
        private const string AddressPropertyName = "Address";
        private const string ObjectPropertyName = "Object";
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
            var objectProp = property.FindPropertyRelative(ObjectPropertyName);
            var lockProp = property.FindPropertyRelative(LockPropertyName);
            // Get cache asset or find real asset
            UnityEngine.Object Object = objectProp.objectReferenceValue;

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
                    objectProp.objectReferenceValue = null;
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
                objectProp.objectReferenceValue = Object;
            }
            EditorGUI.EndProperty();

            void ValidateAddress()
            {
                if (!Object && !string.IsNullOrEmpty(addressProp.stringValue))
                {
                    Object = ResourceEditorExtension.FindAssetEntry(addressProp.stringValue, assetType)?.MainAsset;
                    objectProp.objectReferenceValue = Object;
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
                        objectProp.objectReferenceValue = null;
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
                    objectProp.objectReferenceValue = Object;
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
                                            .First();
                        addressProp.stringValue = (string)method.Invoke(target, new object[1] { Object });
                    }
                    objectProp.objectReferenceValue = Object;
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
            return typeof(UnityEngine.Object);
        }
    }
    public static class ResourceEditorExtension
    {
        public static AddressableAssetEntry AddAsset(this AddressableAssetGroup group, UnityEngine.Object asset, params string[] labels)
        {
            if (asset == null) return null;
            var guid = asset.GetAssetGUID();
            if (guid == null)
            {
                Debug.Log($"Can't find {asset} !");
                return null;
            }
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, false);
            if (labels != null)
            {
                for (int i = 0; i < labels.Length; i++) entry.SetLabel(labels[i], true, true, false);
            }
            return entry;
        }
        public static AddressableAssetEntry ToAddressableAssetEntry(this UnityEngine.Object asset)
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

        public static string GetAssetGUID(this UnityEngine.Object asset)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }
    }
}
