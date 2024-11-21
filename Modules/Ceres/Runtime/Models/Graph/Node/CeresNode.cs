using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;
namespace Ceres.Graph
{
    /// <summary>
    /// Base class for ceres graph node
    /// </summary>
    [Serializable]
    public class CeresNode: IEnumerable<CeresNode>, IDisposable
    {
        public CeresNodeData NodeData { get; internal set; }= new();

        public string Guid 
        { 
            get => NodeData.guid; 
            set => NodeData.guid = value; 
        }
        
        /// <summary>
        /// Release on node destroy
        /// </summary>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Get child not at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual CeresNode GetChildAt(int index)
        {
            return null;
        }
        
        /// <summary>
        /// Add new child node
        /// </summary>
        /// <param name="node"></param>
        public virtual void AddChild(CeresNode node)
        {

        }
        
        /// <summary>
        /// Get child node count
        /// </summary>
        /// <returns></returns>
        public virtual int GetChildrenCount() => 0;
        
        /// <summary>
        /// Clear all child nodes
        /// </summary>
        public virtual void ClearChildren() { }
        
        /// <summary>
        ///  Get all child nodes
        /// </summary>
        /// <returns></returns>
        public virtual CeresNode[] GetChildren()
        {
            return Array.Empty<CeresNode>();
        }
        
        /// <summary>
        /// Set child nodes
        /// </summary>
        /// <param name="children"></param>
        public virtual void SetChildren(CeresNode[] children) { }
        
        /// <summary>
        /// Get serialized data of this node
        /// </summary>
        /// <returns></returns>
        public virtual CeresNodeData GetSerializedData()
        {
            /* Allows polymorphic serialization */
            var data = NodeData.Clone();
            data.Serialize(this);
            return data;
        }
        
        protected struct Enumerator : IEnumerator<CeresNode>
        {
            private readonly Stack<CeresNode> _stack;
            
            private static readonly ObjectPool<Stack<CeresNode>> Pool = new(() => new(), null, s => s.Clear());
            
            private CeresNode _currentNode;
            
            public Enumerator(CeresNode root)
            {
                _stack = Pool.Get();
                _currentNode = null;
                if (root != null)
                {
                    _stack.Push(root);
                }
            }

            public readonly CeresNode Current
            {
                get
                {
                    if (_currentNode == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return _currentNode;
                }
            }

            readonly object IEnumerator.Current => Current;

            public void Dispose()
            {
                Pool.Release(_stack);
                _currentNode = null;
            }
            public bool MoveNext()
            {
                if (_stack.Count == 0)
                {
                    return false;
                }

                _currentNode = _stack.Pop();
                int childrenCount = _currentNode.GetChildrenCount();
                for (int i = childrenCount - 1; i >= 0; i--)
                {
                    _stack.Push(_currentNode.GetChildAt(i));
                }
                return true;
            }
            public void Reset()
            {
                _stack.Clear();
                if (_currentNode != null)
                {
                    _stack.Push(_currentNode);
                }
                _currentNode = null;
            }
        }

        public IEnumerator<CeresNode> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
    /// <summary>
    /// Class store the node editor data
    /// </summary>
    [Serializable]
    public class CeresNodeData
    {
        /// <summary>
        /// Serialized node type in ManagedReference format
        /// </summary>
        [Serializable]
        public struct NodeType
        {
            // ReSharper disable once InconsistentNaming
            public string _class;

            // ReSharper disable once InconsistentNaming
            public string _ns;
            
            // ReSharper disable once InconsistentNaming
            public string _asm;
            
            public NodeType(string inClass, string inNamespace, string inAssembly)
            {
                _class = inClass;
                _ns = inNamespace;
                _asm = inAssembly;
            }
            
            public NodeType(Type type)
            {
                _class = type.Name;
                _ns = type.Namespace;
                _asm = type.Assembly.GetName().Name;
            }
            
            public readonly Type ToType()
            {
                return Type.GetType(Assembly.CreateQualifiedName(_asm, $"{_ns}.{_class}"));
            }
            
            public readonly override string ToString()
            {
                return $"class:{_class} ns: {_ns} asm:{_asm}";
            }
        }
        public Rect graphPosition = new(400, 300, 100, 100);
        
        public string description;
        
        public string guid;
        
        /// <summary>
        /// Node type that helps to locate and recover node when missing class
        /// </summary>
        public NodeType nodeType;
        
        public string serializedData;
        
        /// <summary>
        /// Serialize node data
        /// </summary>
        /// <param name="node"></param>
        public virtual void Serialize(CeresNode node)
        {
            nodeType = new NodeType(node.GetType());
            serializedData = CeresGraphData.Serialize(node);
            /* Override to customize serialization like ISerializationCallbackReceiver */
        }
        
        public virtual CeresNodeData Clone()
        {
            return new CeresNodeData
            {
                graphPosition = graphPosition,
                description = description,
                guid = guid,
                nodeType = nodeType,
                serializedData = serializedData
            };
        }
    }
}