using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ToggleEvent = UnityEngine.UIElements.ChangeEvent<bool>;
namespace Chris.Events.Editor
{
    class EventTypeChoice : IComparable<EventTypeChoice>
    {
        public string Name;
        public string Group;
        public long TypeId;

        public int CompareTo(EventTypeChoice other)
        {
            if (Group == Name)
            {
                var comparison = Group.CompareTo(other.Group);
                return comparison == 0 ? -1 : comparison;
            }

            if (other.Group == other.Name)
            {
                var comparison = Group.CompareTo(other.Group);
                return comparison == 0 ? 1 : comparison;
            }

            return Group.CompareTo(other.Group) * 2 + Name.CompareTo(other.Name);
        }
    }

    internal class EventTypeSearchField : ToolbarSearchField
    {
        public new class UxmlFactory : UxmlFactory<EventTypeSearchField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private const int k_MaxTooltipLines = 40;
        private const
         string EllipsisText = "...";

        private readonly VisualElement m_MenuContainer;
        private readonly VisualElement m_OuterContainer;
        private readonly ListView m_ListView;

        private Dictionary<long, bool> m_State;
        private readonly Dictionary<string, List<long>> m_GroupedEvents;
        private readonly List<EventTypeChoice> m_Choices;
        private List<EventTypeChoice> m_FilteredChoices;
        private Dictionary<long, int> m_EventCountLog;
        private bool m_IsFocused;
        private readonly FieldInfo visualInputField = typeof(BaseField<bool>).GetField("m_VisualInput", BindingFlags.Instance | BindingFlags.NonPublic);
        public int GetSelectedCount() => m_Choices.Count(c => c.TypeId > 0 && m_State[c.TypeId]);

        public new static readonly string ussClassName = "event-debugger-filter";
        public static readonly string ussContainerClassName = ussClassName + "__container";
        public static readonly string ussListViewClassName = ussClassName + "__list-view";
        public static readonly string ussItemContainerClassName = ussClassName + "__item-container";
        public static readonly string ussItemLabelClassName = ussClassName + "__item-label";
        public static readonly string ussGroupLabelClassName = ussClassName + "__group-label";
        public static readonly string ussItemCountClassName = ussClassName + "__item-count";
        public static readonly string ussItemToggleClassName = ussClassName + "__item-toggle";

        public IReadOnlyDictionary<long, bool> State => m_State;

        public void SetState(Dictionary<long, bool> state)
        {
            m_State = state;
            UpdateTextHint();
        }

        private bool IsGenericTypeOf(Type t, Type genericDefinition)
        {
            return IsGenericTypeOf(t, genericDefinition, out Type[] _);
        }

        private bool IsGenericTypeOf(Type t, Type genericDefinition, out Type[] genericParameters)
        {
            genericParameters = new Type[] { };
            if (!genericDefinition.IsGenericType)
            {
                return false;
            }

            var isMatch = t.IsGenericType && t.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition();
            if (!isMatch && t.BaseType != null)
            {
                isMatch = IsGenericTypeOf(t.BaseType, genericDefinition, out genericParameters);
            }
            if (!isMatch && genericDefinition.IsInterface && t.GetInterfaces().Any())
            {
                foreach (var i in t.GetInterfaces())
                {
                    if (IsGenericTypeOf(i, genericDefinition, out genericParameters))
                    {
                        isMatch = true;
                        break;
                    }
                }
            }

            if (isMatch && !genericParameters.Any())
            {
                genericParameters = t.GetGenericArguments();
            }
            return isMatch;
        }

        public EventTypeSearchField()
        {
            m_Choices = new List<EventTypeChoice>();
            m_State = new Dictionary<long, bool>();
            m_GroupedEvents = new Dictionary<string, List<long>>();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            foreach (Assembly assembly in currentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes().Where(t => typeof(EventBase).IsAssignableFrom(t) && !t.ContainsGenericParameters))
                    {
                        AddType(type, true);
                    }
                    // Special case for ChangeEvent<>.
                    var implementingTypes = GetAllTypesImplementingOpenGenericType(typeof(INotifyValueChanged<>), assembly).ToList();
                    foreach (var valueChangedType in implementingTypes)
                    {
                        var baseType = valueChangedType.BaseType;
                        if (baseType == null || baseType.GetGenericArguments().Length <= 0)
                            continue;

                        var argumentType = baseType.GetGenericArguments()[0];
                        if (!argumentType.IsGenericParameter)
                        {
                            AddType(typeof(ChangeEvent<>).MakeGenericType(argumentType), true);
                        }
                    }
                }
                catch (TypeLoadException e)
                {
                    Debug.LogWarningFormat("Error while loading types from assembly {0}: {1}", assembly.FullName, e);
                }
                catch (ReflectionTypeLoadException e)
                {
                    for (var i = 0; i < e.LoaderExceptions.Length; i++)
                    {
                        if (e.LoaderExceptions[i] != null)
                        {
                            Debug.LogError(e.Types[i] + ": " + e.LoaderExceptions[i].Message);
                        }
                    }
                }
            }

            m_State.Add(0, true);

            // Add groups, with negative ids.
            var keyIndex = -1;
            foreach (var key in m_GroupedEvents.Keys.OrderBy(k => k))
            {
                m_Choices.Add(new EventTypeChoice() { Name = key, Group = key, TypeId = keyIndex });
                m_State.Add(keyIndex--, true);
            }

            m_Choices.Sort();
            m_Choices.Insert(0, new EventTypeChoice() { Name = "IAll", Group = "IAll", TypeId = 0 });
            m_FilteredChoices = m_Choices.ToList();

            m_MenuContainer = new VisualElement();
            m_MenuContainer.AddToClassList(ussClassName);

            m_OuterContainer = new VisualElement();
            m_OuterContainer.AddToClassList(ussContainerClassName);
            m_MenuContainer.Add(m_OuterContainer);

            m_ListView = new ListView();
            m_ListView.AddToClassList(ussListViewClassName);
            m_ListView.pickingMode = PickingMode.Position;
            m_ListView.showBoundCollectionSize = false;
            m_ListView.fixedItemHeight = 20;
            m_ListView.selectionType = SelectionType.None;
            m_ListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            m_ListView.makeItem = () =>
            {
                var container = new VisualElement();
                container.AddToClassList(ussItemContainerClassName);

                var toggle = new Toggle();
                toggle.labelElement.AddToClassList(ussItemLabelClassName);
                (visualInputField.GetValue(toggle) as VisualElement).AddToClassList(ussItemToggleClassName);
                toggle.RegisterValueChangedCallback(OnToggleValueChanged);
                container.Add(toggle);

                var label = new Label();
                label.AddToClassList(ussItemCountClassName);
                label.pickingMode = PickingMode.Ignore;
                container.Add(label);

                return container;
            };

            m_ListView.bindItem = (element, i) =>
            {
                var toggle = element[0] as Toggle;
                var countLabel = element[1] as Label;
                var choice = m_FilteredChoices[i];
                toggle.SetValueWithoutNotify(m_State[choice.TypeId]);
                var isGroup = choice.Name == choice.Group;

                toggle.label = isGroup ? $"{choice.Group[1..].Replace("Event", "")} Events" : choice.Name;
                toggle.labelElement.RemoveFromClassList(isGroup ? ussItemLabelClassName : ussGroupLabelClassName);
                toggle.labelElement.AddToClassList(isGroup ? ussGroupLabelClassName : ussItemLabelClassName);
                toggle.userData = i;

                if (m_EventCountLog != null && m_EventCountLog.ContainsKey(choice.TypeId))
                {
                    countLabel.style.display = DisplayStyle.Flex;
                    countLabel.text = m_EventCountLog[choice.TypeId].ToString();
                }
                else
                {
                    countLabel.text = "";
                    countLabel.style.display = DisplayStyle.None;
                }
            };

            m_ListView.itemsSource = m_FilteredChoices;
            m_OuterContainer.Add(m_ListView);

            UpdateTextHint();

            m_MenuContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_MenuContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            textInputField.RegisterValueChangedCallback(OnValueChanged);

            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusEvent>(OnFocus);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        public void SetEventLog(Dictionary<long, int> log)
        {
            m_EventCountLog = log;
        }

        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly)
        {
            return from x in assembly.GetTypes()
                   from z in x.GetInterfaces()
                   let y = x.BaseType
                   where (y != null && y.IsGenericType && openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                   (z.IsGenericType && openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                   select x;
        }

        private void AddType(Type type, bool value)
        {
            var methodInfo = type.GetMethod("TypeId", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (methodInfo == null)
                return;

            var typeId = (long)methodInfo.Invoke(null, null);

            if (m_State.ContainsKey(typeId))
                return;

            m_State.Add(typeId, value);
            var nextType = type;

            static bool InterfacePredicate(Type t) => t.IsPublic && t.Namespace != nameof(System);

            Type interfaceType;
            do
            {
                var previousType = nextType;
                nextType = previousType.BaseType;
                interfaceType = previousType.GetInterfaces().Where(InterfacePredicate).Except(nextType.GetInterfaces().Where(InterfacePredicate)).FirstOrDefault();
            }
            while (interfaceType == null && nextType != typeof(EventBase));

            var readableTypeName = EventDebugger.GetTypeDisplayName(type);
            if (interfaceType != null)
            {
                if (!m_GroupedEvents.ContainsKey(interfaceType.Name))
                {
                    m_GroupedEvents.Add(interfaceType.Name, new List<long>());
                }

                m_GroupedEvents[interfaceType.Name].Add(typeId);
                m_Choices.Add(new EventTypeChoice { Name = readableTypeName, TypeId = typeId, Group = interfaceType.Name });
            }
            else
            {
                if (!m_GroupedEvents.ContainsKey("IUncategorized"))
                    m_GroupedEvents.Add("IUncategorized", new List<long>());

                m_GroupedEvents["IUncategorized"].Add(typeId);
                m_Choices.Add(new EventTypeChoice { Name = readableTypeName, TypeId = typeId, Group = "IUncategorized" });
            }
        }

        private void OnToggleValueChanged(ToggleEvent e)
        {
            var element = e.target as VisualElement;
            var index = (int)element.userData;
            var choice = m_FilteredChoices[index];
            m_State[choice.TypeId] = e.newValue;

            if (choice.TypeId < 0)
            {
                foreach (var eventTypeId in m_GroupedEvents[choice.Group])
                {
                    m_State[eventTypeId] = e.newValue;
                }
            }
            else if (choice.TypeId == 0)
            {
                foreach (var c in m_Choices)
                {
                    m_State[c.TypeId] = e.newValue;
                }
            }

            // All toggling
            if (m_State.Where(s => s.Key > 0).All(s => s.Value))
            {
                m_State[0] = true;
            }
            else if (m_State.Where(s => s.Key > 0).Any(s => !s.Value))
            {
                m_State[0] = false;
            }

            // Group toggling
            if (choice.TypeId != 0)
            {
                if (m_GroupedEvents[choice.Group].All(id => m_State[id]))
                {
                    var group = m_Choices.First(c => c.TypeId < 0 && c.Group == choice.Group);
                    m_State[group.TypeId] = true;
                }
                else if (m_GroupedEvents[choice.Group].Any(id => !m_State[id]))
                {
                    var group = m_Choices.First(c => c.TypeId < 0 && c.Group == choice.Group);
                    m_State[group.TypeId] = false;
                }
            }

            FilterEvents(value);
            using var evt = UnityEngine.UIElements.ChangeEvent<string>.GetPooled(null, null);
            evt.target = this;
            SendEvent(evt);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null)
                return;

            m_ListView.RegisterCallback<GeometryChangedEvent>(EnsureVisibilityInParent);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel == null)
                return;

            m_ListView.UnregisterCallback<GeometryChangedEvent>(EnsureVisibilityInParent);
            m_MenuContainer.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_MenuContainer.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            DropDown();
        }

        private void OnFocus(FocusEvent evt)
        {
            if (!m_IsFocused)
            {
                m_FilteredChoices = m_Choices.ToList();
                m_ListView.itemsSource = m_FilteredChoices;
                RefreshLayout();
                SetValueWithoutNotify("");
            }

            m_IsFocused = true;
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            var focusedElement = evt.relatedTarget as VisualElement;
            if (focusedElement?.FindCommonAncestor(m_ListView) != m_ListView && focusedElement?.FindCommonAncestor(this) != this)
            {
                Hide();
                UpdateTextHint();
                m_IsFocused = false;
            }
            else
            {
                m_MenuContainer.schedule.Execute(Focus);
            }
        }

        private void UpdateTextHint()
        {
            UpdateTooltip();
            var choiceCount = GetSelectedCount();
            base.SetValueWithoutNotify($"{choiceCount} selected event type{(choiceCount > 1 ? "s" : "")}");
        }

        private void OnValueChanged(UnityEngine.UIElements.ChangeEvent<string> changeEvent)
        {
            FilterEvents(changeEvent.newValue.Trim());
        }

        // Use quicksearch instead?
        private const string k_IsKeyword = "is:";
        private static readonly string[] k_OnKeywords = { "on", "enabled", "true" };
        private static readonly string[] k_OffKeywords = { "off", "disabled", "false" };

        private void FilterEvents(string filter)
        {
            m_FilteredChoices.Clear();
            var filterLower = filter.ToLower();

            bool? isOn = null;
            var checkIsParameter = filter.StartsWith(k_IsKeyword);
            if (checkIsParameter)
            {
                var parameter = filter[k_IsKeyword.Length..];
                if (k_OnKeywords.Contains(parameter))
                    isOn = true;
                else if (k_OffKeywords.Contains(parameter))
                    isOn = false;
            }

            foreach (var choice in m_Choices)
            {
                if (isOn != null && m_State[choice.TypeId] == isOn.Value)
                {
                    m_FilteredChoices.Add(choice);
                }
                else if (string.IsNullOrEmpty(filter) || choice.Name.ToLower().Contains(filterLower) || choice.Group.ToLower().Contains(filterLower))
                {
                    m_FilteredChoices.Add(choice);
                }
            }

            m_ListView.itemsSource = m_FilteredChoices;
            RefreshLayout();
        }

        private void DropDown()
        {
            var root = panel.GetRootVisualElement();
            root.Add(m_MenuContainer);

            m_MenuContainer.style.left = root.layout.x;
            m_MenuContainer.style.top = root.layout.y;
            m_MenuContainer.style.width = root.layout.width;
            m_MenuContainer.style.height = root.layout.height;

            m_OuterContainer.style.left = worldBound.x - root.layout.x;
            m_OuterContainer.style.top = worldBound.y - root.layout.y;

            ClearTooltip();
        }

        private void Hide()
        {
            m_MenuContainer.RemoveFromHierarchy();
        }

        private void EnsureVisibilityInParent(GeometryChangedEvent evt)
        {
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            var root = panel.GetRootVisualElement();
            if (root != null && !float.IsNaN(m_OuterContainer.layout.width) && !float.IsNaN(m_OuterContainer.layout.height))
            {
                m_OuterContainer.style.height = Mathf.Min(
                    m_MenuContainer.layout.height - m_MenuContainer.layout.y - m_OuterContainer.layout.y,
                    m_ListView.fixedItemHeight * m_ListView.itemsSource.Count +
                    m_ListView.resolvedStyle.borderTopWidth + m_ListView.resolvedStyle.borderBottomWidth +
                    m_OuterContainer.resolvedStyle.borderBottomWidth + m_OuterContainer.resolvedStyle.borderTopWidth);

                if (resolvedStyle.width > m_OuterContainer.resolvedStyle.width)
                {
                    m_OuterContainer.style.width = resolvedStyle.width;
                }
            }
        }

        private void UpdateTooltip()
        {
            var tooltipStr = new StringBuilder();
            var lineCount = 0;
            foreach (var selectedChoice in m_Choices.Where(c => c.TypeId > 0 && m_State[c.TypeId]))
            {
                if (lineCount++ >= k_MaxTooltipLines)
                {
                    tooltipStr.AppendLine(EllipsisText);
                    break;
                }

                tooltipStr.AppendLine(selectedChoice.Name);
            }

            textInputField.tooltip = tooltipStr.ToString();
        }

        private void ClearTooltip()
        {
            textInputField.tooltip = "Type in event name to filter the list. You can also use the keyword is:{on/off}.";
        }
    }
}
