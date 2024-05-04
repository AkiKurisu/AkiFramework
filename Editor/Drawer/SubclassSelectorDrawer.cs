using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace Kurisu.Framework.Editor
{

	[CustomPropertyDrawer(typeof(SubclassSelector))]
	public class SubclassSelectorDrawer : PropertyDrawer
	{

		private readonly struct TypePopupCache
		{
			public AdvancedTypePopup TypePopup { get; }
			public AdvancedDropdownState State { get; }
			public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state)
			{
				TypePopup = typePopup;
				State = state;
			}
		}
		private const int k_MaxTypePopupLineCount = 13;
		private static readonly Type k_UnityObjectType = typeof(UnityEngine.Object);
		private static readonly GUIContent k_NullDisplayName = new(TypeMenuUtility.k_NullDisplayName);
		private static readonly GUIContent k_IsNotManagedReferenceLabel = new("The property type is not manage reference.");

		private readonly Dictionary<string, TypePopupCache> m_TypePopups = new();
		private readonly Dictionary<string, GUIContent> m_TypeNameCaches = new();

		private SerializedProperty m_TargetProperty;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				// Draw the subclass selector popup.
				Rect popupPosition = new(position);
				popupPosition.width -= EditorGUIUtility.labelWidth;
				popupPosition.x += EditorGUIUtility.labelWidth;
				popupPosition.height = EditorGUIUtility.singleLineHeight;

				if (EditorGUI.DropdownButton(popupPosition, GetTypeName(property), FocusType.Keyboard))
				{
					TypePopupCache popup = GetTypePopup(property);
					m_TargetProperty = property;
					popup.TypePopup.Show(popupPosition);
				}
				// Draw the managed reference property.
				EditorGUI.PropertyField(position, property, label, true);
			}
			else
			{
				EditorGUI.LabelField(position, label, k_IsNotManagedReferenceLabel);
			}

			EditorGUI.EndProperty();
		}

		private TypePopupCache GetTypePopup(SerializedProperty property)
		{
			// Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
			string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

			if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename, out TypePopupCache result))
			{
				var state = new AdvancedDropdownState();

				Type baseType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);
				var popup = new AdvancedTypePopup(
					TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
						(p.IsPublic || p.IsNestedPublic) &&
						!p.IsAbstract &&
						!p.IsGenericType &&
						!k_UnityObjectType.IsAssignableFrom(p)
					),
					k_MaxTypePopupLineCount,
					state
				);
				popup.OnItemSelected += item =>
				{
					Type type = item.Type;
					object obj = m_TargetProperty.SetManagedReference(type);
					m_TargetProperty.isExpanded = obj != null;
					m_TargetProperty.serializedObject.ApplyModifiedProperties();
					m_TargetProperty.serializedObject.Update();
				};

				result = new TypePopupCache(popup, state);
				m_TypePopups.Add(managedReferenceFieldTypename, result);
			}
			return result;
		}

		private GUIContent GetTypeName(SerializedProperty property)
		{
			// Cache this string.
			string managedReferenceFullTypename = property.managedReferenceFullTypename;

			if (string.IsNullOrEmpty(managedReferenceFullTypename))
			{
				return k_NullDisplayName;
			}
			if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out GUIContent cachedTypeName))
			{
				return cachedTypeName;
			}

			Type type = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
			string typeName = null;

			AddTypeMenu typeMenu = TypeMenuUtility.GetAttribute(type);
			if (typeMenu != null)
			{
				typeName = typeMenu.GetTypeNameWithoutPath();
				if (!string.IsNullOrWhiteSpace(typeName))
				{
					typeName = ObjectNames.NicifyVariableName(typeName);
				}
			}

			if (string.IsNullOrWhiteSpace(typeName))
			{
				typeName = ObjectNames.NicifyVariableName(type.Name);
			}

			GUIContent result = new(typeName);
			m_TypeNameCaches.Add(managedReferenceFullTypename, result);
			return result;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, true);
		}

	}
}