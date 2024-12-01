using UnityEngine;
using UnityEditor;
using System.Linq;
using Chris.Editor;

namespace Chris.Serialization.Editor
{
    // EditorWindow is modified from R3.Unity
    public class SerializationDebuggerEditorWindow : EditorWindow
    {
        static SerializationDebuggerEditorWindow window;

        [MenuItem("Tools/Chris/Serialization Debugger")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }

            // will called OnEnable(singleton instance will be set).
            GetWindow<SerializationDebuggerEditorWindow>("Serialization Debugger").Show();
        }

        static readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];

        SerializationDebuggerTreeView treeView;
        object splitterState;

        private void OnEnable()
        {
            window = this; // set singleton.
            splitterState = SplitterGUILayout.CreateSplitterState(new float[] { 75f, 25f }, new int[] { 32, 32 }, null);
            treeView = new SerializationDebuggerTreeView();
        }
        private void Update()
        {
            if (GlobalObjectManager.CheckAndResetDirty())
            {
                treeView.ReloadAndSort();
                Repaint();
            }
        }
        private void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(splitterState, EmptyLayoutOption);
            {
                // Column Tabble
                RenderTable();

                // StackTrace details
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        #region HeadPanel

        private static readonly GUIContent CleanupHeadContent = EditorGUIUtility.TrTextContent("Cleanup", "Cleanup Global Objects", (Texture)null);

        private void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, EmptyLayoutOption);
            GUILayout.Label($"Global Objects Count");
            GUILayout.Label($"{GlobalObjectManager.GetObjectNum()}");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CleanupHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                GlobalObjectManager.Cleanup();
                treeView.ReloadAndSort();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region TableColumn

        private Vector2 tableScroll;
        private GUIStyle tableListStyle;

        private void RenderTable()
        {
            if (tableListStyle == null)
            {
                tableListStyle = new GUIStyle("CN Box");
                tableListStyle.margin.top = 0;
                tableListStyle.padding.left = 3;
            }

            EditorGUILayout.BeginVertical(tableListStyle, EmptyLayoutOption);

            tableScroll = EditorGUILayout.BeginScrollView(tableScroll, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)
            });
            var controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true)
            });


            treeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }



        #endregion

        #region Details

        private static GUIStyle detailsStyle;
        private Vector2 detailsScroll;

        private void RenderDetailsPanel()
        {
            if (detailsStyle == null)
            {
                detailsStyle = new GUIStyle("CN Message")
                {
                    wordWrap = false,
                    stretchHeight = true
                };
                detailsStyle.margin.right = 15;
            }

            string message = "";
            var selected = treeView.state.selectedIDs;
            if (selected.Count > 0)
            {
                var first = selected[0];
                if (treeView.CurrentBindingItems.FirstOrDefault(x => x.id == first) is SerializationDebuggerTreeView.ViewItem item)
                {
                    message = item.Object != null ? item.Object.name : string.Empty;
                }
            }

            detailsScroll = EditorGUILayout.BeginScrollView(this.detailsScroll, EmptyLayoutOption);
            var vector = detailsStyle.CalcSize(new GUIContent(message));
            EditorGUILayout.SelectableLabel(message, detailsStyle, new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(vector.x),
                GUILayout.MinHeight(vector.y)
            });
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}

