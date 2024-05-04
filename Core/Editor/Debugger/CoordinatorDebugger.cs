using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Toolbar = UnityEditor.UIElements.Toolbar;
using Object = UnityEngine.Object;
namespace Kurisu.Framework.Events.Editor
{
    internal interface ICoordinatorChoice
    {
        MonoEventCoordinator Coordinator { get; }
    }

    internal class CoordinatorChoice : ICoordinatorChoice
    {
        public MonoEventCoordinator Coordinator { get; }

        public CoordinatorChoice(MonoEventCoordinator p)
        {
            Coordinator = p;
        }

        public override string ToString()
        {
            return Coordinator.gameObject.name;
        }
    }

    [Serializable]
    internal class CoordinatorDebugger : ICoordinatorDebugger
    {
        [SerializeField]
        private string m_LastVisualTreeName;

        protected EditorWindow m_DebuggerWindow;
        private ICoordinatorChoice m_SelectedCoordinator;
        protected VisualElement m_Toolbar;
        protected ToolbarMenu m_CoordinatorSelect;
        private List<ICoordinatorChoice> m_CoordinatorChoices;
        private IVisualElementScheduledItem m_ConnectWindowScheduledItem;
        private IVisualElementScheduledItem m_RestoreSelectionScheduledItem;
        public MonoEventCoordinator CoordinatorDebug { get; set; }
        public void Initialize(EditorWindow debuggerWindow)
        {
            m_DebuggerWindow = debuggerWindow;

            m_Toolbar ??= new Toolbar();

            // Register panel choice refresh on the toolbar so the event
            // is received before the ToolbarPopup clickable handle it.
            m_Toolbar.RegisterCallback<MouseDownEvent>((e) =>
            {
                if (e.target == m_CoordinatorSelect)
                    RefreshPanelChoices();
            }, TrickleDown.TrickleDown);

            m_CoordinatorChoices = new List<ICoordinatorChoice>();
            m_CoordinatorSelect = new ToolbarMenu
            {
                name = "panelSelectPopup",
                variant = ToolbarMenu.Variant.Popup,
                text = "Select a coordinator"
            };

            m_Toolbar.Insert(0, m_CoordinatorSelect);

            if (!string.IsNullOrEmpty(m_LastVisualTreeName))
                m_RestoreSelectionScheduledItem = m_Toolbar.schedule.Execute(RestorePanelSelection).Every(500);
        }

        public void OnDisable()
        {
            var lastTreeName = m_LastVisualTreeName;
            SelectCoordinatorToDebug(null);
            if (CoordinatorDebug) CoordinatorDebug.DetachDebugger(this);
            m_LastVisualTreeName = lastTreeName;
        }

        public void Disconnect()
        {
            var lastTreeName = m_LastVisualTreeName;
            m_SelectedCoordinator = null;
            SelectCoordinatorToDebug(null);

            m_LastVisualTreeName = lastTreeName;
        }

        public void ScheduleWindowToDebug(EditorWindow window)
        {
            if (window != null)
            {
                Disconnect();
                m_ConnectWindowScheduledItem = m_Toolbar.schedule.Execute(TrySelectWindow).Every(500);
            }
        }

        private void TrySelectWindow()
        {
            MonoEventCoordinator monoEventCoordinator = Object.FindAnyObjectByType<MonoEventCoordinator>();
            SelectPanelToDebug(monoEventCoordinator);

            if (m_SelectedCoordinator != null)
            {
                m_ConnectWindowScheduledItem.Pause();
            }
        }

        public virtual void Refresh()
        { }

        protected virtual bool ValidateDebuggerConnection(IEventCoordinator connection)
        {
            return true;
        }

        protected virtual void OnSelectCoordinateDebug(IEventCoordinator pdbg) { }
        protected virtual void OnRestorePanelSelection() { }

        protected virtual void PopulatePanelChoices(List<ICoordinatorChoice> panelChoices)
        {
            MonoEventCoordinator[] monoEventCoordinators = Object.FindObjectsByType<MonoEventCoordinator>(FindObjectsSortMode.InstanceID);
            panelChoices.AddRange(monoEventCoordinators.Select(x => new CoordinatorChoice(x)));
        }

        private void RefreshPanelChoices()
        {
            m_CoordinatorChoices.Clear();
            PopulatePanelChoices(m_CoordinatorChoices);

            var menu = m_CoordinatorSelect.menu;
            menu.ClearItems();

            foreach (var panelChoice in m_CoordinatorChoices)
            {
                menu.AppendAction(panelChoice.ToString(), OnSelectPanel, DropdownMenuAction.AlwaysEnabled, panelChoice);
            }
        }

        private void OnSelectPanel(DropdownMenuAction action)
        {
            if (m_RestoreSelectionScheduledItem != null && m_RestoreSelectionScheduledItem.isActive)
                m_RestoreSelectionScheduledItem.Pause();

            SelectCoordinatorToDebug(action.userData as ICoordinatorChoice);
        }

        private void RestorePanelSelection()
        {
            RefreshPanelChoices();
            if (m_CoordinatorChoices.Count > 0)
            {
                if (!string.IsNullOrEmpty(m_LastVisualTreeName))
                {
                    // Try to retrieve last selected VisualTree
                    for (int i = 0; i < m_CoordinatorChoices.Count; i++)
                    {
                        var vt = m_CoordinatorChoices[i];
                        if (vt.ToString() == m_LastVisualTreeName)
                        {
                            SelectCoordinatorToDebug(vt);
                            break;
                        }
                    }
                }

                if (m_SelectedCoordinator != null)
                    OnRestorePanelSelection();
                else
                    SelectCoordinatorToDebug(null);

                m_RestoreSelectionScheduledItem.Pause();
            }
        }

        protected virtual void SelectCoordinatorToDebug(ICoordinatorChoice pc)
        {
            // Detach debugger from current panel
            if (CoordinatorDebug != null)
                CoordinatorDebug.DetachDebugger(this);
            string menuText;

            if (pc != null && ValidateDebuggerConnection(pc.Coordinator))
            {
                pc.Coordinator.AttachDebugger(this);
                m_SelectedCoordinator = pc;
                m_LastVisualTreeName = pc.ToString();

                OnSelectCoordinateDebug(CoordinatorDebug);
                menuText = pc.ToString();
            }
            else
            {
                // No tree selected
                m_SelectedCoordinator = null;
                m_LastVisualTreeName = null;

                OnSelectCoordinateDebug(null);
                menuText = "Select a coordinator";
            }

            m_CoordinatorSelect.text = menuText;
        }

        protected void SelectPanelToDebug(MonoEventCoordinator panel)
        {
            // Select new tree
            if (m_SelectedCoordinator?.Coordinator != panel)
            {
                SelectCoordinatorToDebug(null);
                RefreshPanelChoices();
                for (int i = 0; i < m_CoordinatorChoices.Count; i++)
                {
                    var pc = m_CoordinatorChoices[i];
                    if (pc.Coordinator == panel)
                    {
                        SelectCoordinatorToDebug(pc);
                        break;
                    }
                }
            }
        }

        public virtual bool InterceptEvent(EventBase ev)
        {
            return false;
        }

        public virtual void PostProcessEvent(EventBase ev)
        { }
    }
}
