using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace Kurisu.Framework.Schedulers.Editor
{
    public class SchedulerDebuggerTreeView : TreeView
    {
        internal class ViewItem : TreeViewItem
        {
            public ulong Handle { get; set; }
            public string Name { get; set; }
            public TickFrame TickFrame { get; set; }
            public bool Running { get; set; }
            public double ElapsedTime { get; set; }
            public SchedulerRunner.ScheduledItem ScheduledItem { get; set; }
            public ViewItem(int id) : base(id)
            {

            }
        }
        const string sortedColumnIndexStateKey = "SchedulerDebuggerTreeView_sortedColumnIndex";

        public IReadOnlyList<TreeViewItem> CurrentBindingItems;

        public SchedulerDebuggerTreeView()
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Handle"), width = 10},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Name")},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("TickFrame"), width = 10},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Running"), width = 5},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("ElapsedTime"), width = 10}
            })))
        {
        }

        SchedulerDebuggerTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += Header_sortingChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(sortedColumnIndexStateKey, 1);
        }

        public void ReloadAndSort()
        {
            var currentSelected = state.selectedIDs;
            Reload();
            Header_sortingChanged(multiColumnHeader);
            state.selectedIDs = currentSelected;
        }

        private void Header_sortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SessionState.SetInt(sortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            var index = multiColumnHeader.sortedColumnIndex;
            var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

            var items = rootItem.children.Cast<ViewItem>();
            IOrderedEnumerable<ViewItem> orderedEnumerable = index switch
            {
                0 => ascending ? items.OrderBy(item => item.Handle) : items.OrderByDescending(item => item.Handle),
                1 => ascending ? items.OrderBy(item => item.Name) : items.OrderByDescending(item => item.Name),
                2 => ascending ? items.OrderBy(item => item.TickFrame) : items.OrderByDescending(item => item.TickFrame),
                3 => ascending ? items.OrderBy(item => item.Running) : items.OrderByDescending(item => item.Running),
                4 => ascending ? items.OrderBy(item => item.ElapsedTime) : items.OrderByDescending(item => item.ElapsedTime),
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };

            var children = new List<TreeViewItem>();

            if (Application.isPlaying)
            {
                foreach (var scheduled in SchedulerRunner.Instance.scheduledItems)
                {
                    string taskName = string.Empty;
                    if (SchedulerRegistry.TryGetListener(scheduled.Value, out var listener))
                        taskName = listener.name;
                    children.Add(new ViewItem(scheduled.Value.Handle.GetIndex())
                    {
                        Handle = scheduled.Value.Handle.Handle,
                        Name = taskName,
                        TickFrame = scheduled.TickFrame,
                        Running = !scheduled.Value.IsPaused,
                        ElapsedTime = Time.timeSinceLevelLoadAsDouble - scheduled.Timestamp,
                        ScheduledItem = scheduled
                    });
                }
            }

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as ViewItem;

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                switch (columnIndex)
                {
                    case 0:
                        EditorGUI.LabelField(rect, item.Handle.ToString(), labelStyle);
                        break;
                    case 1:
                        EditorGUI.LabelField(rect, item.Name.ToString(), labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(rect, item.TickFrame.ToString(), labelStyle);
                        break;
                    case 3:
                        EditorGUI.Toggle(rect, item.Running);
                        break;
                    case 4:
                        EditorGUI.LabelField(rect, item.ElapsedTime.ToString(), labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }
    }

}
