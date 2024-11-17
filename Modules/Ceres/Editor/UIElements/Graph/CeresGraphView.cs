using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public abstract class CeresGraphView: GraphView, IVariableSource
    {
        public List<SharedVariable> SharedVariables { get; } = new();
        
        public EditorWindow EditorWindow { get; set; }
        
        public CeresBlackboard Blackboard { get; set; }
        
        public GroupBlockHandler GroupBlockHandler { get; protected set; }

        public ContextualMenuRegistry ContextualMenuRegistry { get; } = new();

        protected CeresGraphView(EditorWindow editorWindow)
        {
            EditorWindow = editorWindow;
            style.flexGrow = 1;
            style.flexShrink = 1;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Insert(0, new GridBackground());
            var contentDragger = new ContentDragger();
            contentDragger.activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.MiddleMouse,
            });
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(contentDragger);
            var dragDropManipulator = new DragDropManipulator(this);
            dragDropManipulator.OnDragOverEvent += CopyFromObject;
            this.AddManipulator(dragDropManipulator);
            canPasteSerializedData += (data) => true;
            serializeGraphElements += OnSerialize;
            unserializeAndPaste += OnPaste;
        }

        protected virtual void CopyFromObject(Object data, Vector3 mousePosition)
        {
        }
        
        protected virtual string OnSerialize(IEnumerable<GraphElement> elements)
        {
            return string.Empty;
        }

        protected virtual void OnPaste(string a, string b)
        {

        }
    }
}