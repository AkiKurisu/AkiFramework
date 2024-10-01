using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.Framework.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(SerializedType<>))]
    public class SerializedTypeDrawer : PropertyDrawer
    {
        private const string NullType = "Null";
        private static readonly GUIStyle DropdownStyle = new("ExposablePopupMenu");
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var totalHeight = position.height;
            position.height = EditorGUIUtility.singleLineHeight;
            var reference = property.FindPropertyRelative("serializedTypeString");
            Type type;
            try
            {
                type = SerializedType.FromString(reference.stringValue);
            }
            catch
            {
                type = null;
            }
            string id = type != null ? type.Name : NullType;
            float width = position.width;
            float x = position.x;
            position.width = GUI.skin.label.CalcSize(label).x;
            GUI.Label(position, label);
            position.x += position.width + 10;
            position.width = width - position.width - 10;
            if (EditorGUI.DropdownButton(position, new GUIContent(id), FocusType.Keyboard, DropdownStyle))
            {
                var provider = ScriptableObject.CreateInstance<TypeSearchWindow>();
                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsArray)
                {
                    fieldType = fieldType.GetElementType();
                }
                provider.Initialize(fieldType.GetGenericArguments()[0], (selectType) =>
                {
                    reference.stringValue = selectType != null ? SerializedType.ToString(selectType) : NullType;
                    property.serializedObject.ApplyModifiedProperties();
                });
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }
            EditorGUI.EndProperty();
        }
    }
    public class TypeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private Texture2D _indentationIcon;
        private Action<Type> typeSelectCallBack;
        private Type searchType;
        public void Initialize(Type searchType, Action<Type> typeSelectCallBack)
        {
            this.searchType = searchType;
            this.typeSelectCallBack = typeSelectCallBack;
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }
        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Select Type"), 0),
                new(new GUIContent("<Null>", _indentationIcon)) { level = 1, userData = null }
            };
            List<Type> nodeTypes = FindSubClasses(searchType).ToList();
            var groups = nodeTypes.GroupBy(t => t.Assembly);
            foreach (var group in groups)
            {
                entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {group.Key.GetName().Name}"), 1));
                var subGroups = group.GroupBy(x => x.Namespace);
                foreach (var subGroup in subGroups)
                {
                    entries.Add(new SearchTreeGroupEntry(new GUIContent($"Select {subGroup.Key}"), 2));
                    foreach (var type in subGroup)
                    {
                        entries.Add(new SearchTreeEntry(new GUIContent(type.Name, _indentationIcon)) { level = 3, userData = type });
                    }
                }
            }
            return entries;
        }
        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as Type;
            typeSelectCallBack?.Invoke(type);
            return true;
        }
        private static IEnumerable<Type> FindSubClasses(Type father)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(t => father.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);
        }
    }
}