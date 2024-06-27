using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
namespace Kurisu.Framework.Editor
{
    [CustomPropertyDrawer(typeof(AssetReferenceSelector))]
    public class AssetReferenceSelectorDrawer : PropertyDrawer
    {
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selector = attribute as AssetReferenceSelector;
            EditorGUI.BeginProperty(position, label, property);
            position.width /= 2;
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
            int indent = EditorGUI.indentLevel;
            position.x += position.width;
            EditorGUI.BeginChangeCheck();
            var Object = EditorGUI.ObjectField(position, null, selector.SelectAssetType, selector.AllowSceneObjects);
            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(selector.ProcessMethod))
                {
                    property.stringValue = Object.name;
                }
                else
                {
                    object target = ReflectionUtility.GetTargetObjectWithProperty(property);
                    var method = target.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetParameters().Length == 1).Where(x => x.Name == selector.ProcessMethod).First();
                    property.stringValue = (string)method.Invoke(target, new object[1] { Object });
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
