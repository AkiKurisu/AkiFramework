using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Chris.DataDriven;
namespace Chris.Editor
{
    [CustomPropertyDrawer(typeof(PopupSelector), true)]
    public class PopupSelectorDrawer : PropertyDrawer
    {
        private static readonly GUIContent k_IsNotStringLabel = new("The property type is not string.");
        private static readonly GUIContent k_IsNotPopupSetLabel = new("The popup type is not implemented form PopupSet.");
        private static readonly Dictionary<Type, PopupSet> dataSetDict = new();

        public static PopupSet GetOrCreateSettings(Type type)
        {
            var guids = AssetDatabase.FindAssets($"t:{type.FullName}");
            PopupSet setting = null;
            if (guids.Length != 0)
            {
                setting = guids.Select(x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), type) as PopupSet)
                                .FirstOrDefault(x => x.GetType() == type);
            }
            if (!setting)
            {
                setting = ScriptableObject.CreateInstance(type) as PopupSet;
                string k_SettingsPath = $"Assets/{type.Name}.asset";
                Debug.Log($"New {type.Name} Set Created! Saving Path:{k_SettingsPath}");
                AssetDatabase.CreateAsset(setting, k_SettingsPath);
                AssetDatabase.SaveAssets();
            }
            return setting;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var popupType = ((PopupSelector)attribute).PopupType;
            var title = ((PopupSelector)attribute).PopupTitle;
            if (popupType.IsSubclassOf(typeof(PopupSet)) || popupType == typeof(PopupSet))
            {
                if (property.propertyType == SerializedPropertyType.String)
                {
                    Rect popupPosition = new(position)
                    {
                        height = EditorGUIUtility.singleLineHeight
                    };
                    if (!dataSetDict.ContainsKey(popupType)) dataSetDict[popupType] = GetOrCreateSettings(popupType);
                    PopupSet popupSet = dataSetDict[popupType];
                    int index = EditorGUI.Popup(position: popupPosition, title ?? label.text, selectedIndex: popupSet.GetStateID(property.stringValue), displayedOptions: popupSet.Values);
                    if (index >= 0)
                    {
                        property.stringValue = popupSet.Values[index];
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label, k_IsNotStringLabel);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label, k_IsNotPopupSetLabel);
            }

            EditorGUI.EndProperty();
        }
    }
}
