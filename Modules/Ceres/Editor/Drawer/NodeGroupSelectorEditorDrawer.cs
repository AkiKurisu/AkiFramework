using System;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEditor;
using UnityEngine;
namespace Ceres.Editor
{
    [CustomPropertyDrawer(typeof(NodeGroupSelectorAttribute))]
    public class NodeGroupSelectorEditorDrawer : PropertyDrawer
    {
        private static readonly Type[] DefaultTypes = { typeof(CeresNode) };
        
        private const string HiddenGroup = "Hidden";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (EditorGUI.DropdownButton(position, new GUIContent(property.stringValue, property.tooltip), FocusType.Passive))
            {
                var types = ((NodeGroupSelectorAttribute)attribute).Types ?? DefaultTypes; 
                var groups = SubclassSearchUtility.FindSubClassTypes(types)
                .Where(x => x.GetCustomAttribute<NodeGroupAttribute>() != null)
                .Select(x => SubclassSearchUtility.GetSplittedGroupName(x.GetCustomAttribute<NodeGroupAttribute>().Group)[0])
                .Distinct()
                .ToList();
                var menu = new GenericMenu();
                foreach (var group in groups)
                {
                    if(group == HiddenGroup) continue;
                    menu.AddItem(new GUIContent(group), false, () => property.stringValue = group);
                }
                menu.ShowAsContext();
            }
            EditorGUI.EndProperty();
        }
    }
}
