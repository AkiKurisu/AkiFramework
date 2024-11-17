using System.Linq;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Ceres.Editor
{
    public class GroupBlockHandler
    {
        protected readonly GraphView _graphView;
        
        public GroupBlockHandler(GraphView graphView)
        {
            _graphView = graphView;
        }
        
        public virtual Group CreateGroup(Rect rect, NodeGroupBlock blockData = null)
        {
            blockData ??= new NodeGroupBlock();
            var group = new Group
            {
                autoUpdateGeometry = true,
                title = blockData.Title
            };
            _graphView.AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        
        public virtual void SelectGroup(Node node)
        {
            var block = CreateGroup(new Rect(node.transform.position, new Vector2(100, 100)));
            foreach (var select in _graphView.selection)
            {
                if (select is not Node selectNode) continue;
                block.AddElement(selectNode);
            }
        }
        
        public virtual void UnselectGroup()
        {
            foreach (var select in _graphView.selection)
            {
                if (select is not Node node) continue;
                var block = _graphView.graphElements.OfType<Group>().FirstOrDefault(x => x.ContainsElement(node));
                block?.RemoveElement(node);
            }
        }
    }
}
