using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Kurisu.Framework.Editor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UObject = UnityEngine.Object;
using Object = UnityEngine.Object;
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
            GetPropertyMetaData(out var assetGroup, out var assetType, out var processMethod, out var forceMoveToGroup);
            var addressProp = property.FindPropertyRelative(AddressPropertyName);
            var guidProp = property.FindPropertyRelative(GuidPropertyName);
            var lockProp = property.FindPropertyRelative(LockPropertyName);

            // Get cache asset from hard reference
            UObject Object = SoftAssetReferenceEditorUtils.GetAssetFromGUID(guidProp.stringValue);

            // TODO: Only need validate once
            ValidateAddress();

            ListenDragAndDrop(position, property);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            Object = EditorGUI.ObjectField(position, label, Object, assetType, false);
            if (EditorGUI.EndChangeCheck())
            {
                if (Object)
                {
                    AssignAddress(property, Object, processMethod, assetGroup, forceMoveToGroup);
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
                Object = ResourceEditorUtils.FindAssetEntry(addressProp.stringValue, assetType)?.MainAsset;
                guidProp.stringValue = Object.GetAssetGUID();
            }
            EditorGUI.EndProperty();

            void ValidateAddress()
            {
                if (!Object && !string.IsNullOrEmpty(addressProp.stringValue))
                {
                    Object = ResourceEditorUtils.FindAssetEntry(addressProp.stringValue, assetType)?.MainAsset;
                    guidProp.stringValue = Object.GetAssetGUID();
                }
                else if (Object && string.IsNullOrEmpty(addressProp.stringValue))
                {
                    // when reference is in locked mode, prefer to Object value
                    if (lockProp.boolValue)
                    {
                        AssignAddress(property, Object, processMethod, assetGroup, forceMoveToGroup);
                    }
                    else
                    {
                        Object = null;
                        guidProp.stringValue = string.Empty;
                    }
                }
            }
        }
        private static void AssignAddress(SerializedProperty property, Object Object, string processMethod, AddressableAssetGroup assetGroup, bool forceMoveToGroup)
        {
            var addressProp = property.FindPropertyRelative(AddressPropertyName);
            var guidProp = property.FindPropertyRelative(GuidPropertyName);
            // Alreay exists => use entry current address, ensure to not effect other references
            string path = AssetDatabase.GetAssetPath(Object);
            var existingEntry = Object.ToAddressableAssetEntry();
            if (existingEntry != null && !(forceMoveToGroup && existingEntry.parentGroup != assetGroup))
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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
        private void ListenDragAndDrop(Rect rect, SerializedProperty property)
        {
            if (!fieldInfo.FieldType.IsArray)
            {
                return;
            }
            int index = property.propertyPath.LastIndexOf(".Array.data[");
            string parentPath = property.propertyPath[..index];
            var parentProp = property.serializedObject.FindProperty(parentPath);
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition))
                    {
                        break;
                    }

                    if (evt.type == EventType.DragPerform)
                    {
                        GetPropertyMetaData(out var assetGroup, out var assetType, out var processMethod, out var forceMoveToGroup);
                        var array = DragAndDrop.objectReferences.Where(asset =>
                        {
                            return asset.GetType() == assetType || asset.GetType().IsSubclassOf(assetType);
                        }).ToArray();

                        if (array.Length <= 1) return;

                        DragAndDrop.AcceptDrag();
                        int startId = 0;
                        var lstProp = parentProp.GetArrayElementAtIndex(parentProp.arraySize - 1);
                        var lstGuid = lstProp.FindPropertyRelative(GuidPropertyName).stringValue;
                        // If last slot is empty, assign it
                        if (string.IsNullOrEmpty(lstGuid))
                        {
                            startId = 1;
                            AssignAddress(lstProp, array[0], processMethod, assetGroup, forceMoveToGroup);
                            lstProp.FindPropertyRelative(LockPropertyName).boolValue = true;
                        }
                        for (int i = startId; i < array.Length; ++i)
                        {
                            parentProp.InsertArrayElementAtIndex(parentProp.arraySize);
                            var childProp = parentProp.GetArrayElementAtIndex(parentProp.arraySize - 1);
                            AssignAddress(childProp, array[i], processMethod, assetGroup, forceMoveToGroup);
                            childProp.FindPropertyRelative(LockPropertyName).boolValue = true;
                        }
                        Event.current.Use();
                    }
                    break;
                default:
                    break;
            }
        }
        private void GetPropertyMetaData(out AddressableAssetGroup assetGroup, out Type assetType,
                                out string processMethod, out bool forceMoveToGroup)
        {
            assetGroup = AddressableAssetSettingsDefaultObject.Settings.DefaultGroup;
            assetType = GetAssetType();
            processMethod = null;
            forceMoveToGroup = false;
            var constraint = fieldInfo.GetCustomAttribute<AssetReferenceConstraintAttribute>(false);
            if (constraint != null)
            {
                // Attribute constraint should match generic constraint
                if (constraint.AssetType != null && constraint.AssetType.IsSubclassOf(assetType))
                    assetType = constraint.AssetType;

                processMethod = constraint.Formatter;
                forceMoveToGroup = constraint.ForceGroup;
                if (!string.IsNullOrEmpty(constraint.Group))
                {
                    assetGroup = ResourceEditorUtils.GetOrCreateAssetGroup(constraint.Group);
                }
            }
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
}
