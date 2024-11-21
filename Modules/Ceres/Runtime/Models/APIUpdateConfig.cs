using System;
using System.Linq;
using Ceres.Graph;
using UnityEngine;
using UnityEngine.Serialization;
namespace Ceres
{
    [CreateAssetMenu(fileName = "APIUpdateConfig", menuName = "Ceres/APIUpdateConfig")]
    public class APIUpdateConfig : ScriptableObject
    {
        [Serializable]
        public class SerializeNodeType
        {
            private Type _type;
            
            public Type Type => _type ??= ToType();
            
            public string nodeType;
            
            private Type ToType()
            {
                var tokens = nodeType.Split(' ');
                return new CeresNodeData.NodeType(tokens[0], tokens[1], tokens[2]).ToType();
            }
            
            public string GetFullTypeName()
            {
                return $"{Type.Assembly.GetName().Name} {Type.FullName}";
            }
            
            public SerializeNodeType() { }
            
            public SerializeNodeType(Type type)
            {
                CeresNodeData.NodeType node = new(type);
                nodeType = $"{node._class} {node._ns} {node._asm}";
            }
            
            public SerializeNodeType(CeresNodeData.NodeType nodeType)
            {
                this.nodeType = $"{nodeType._class} {nodeType._ns} {nodeType._asm}";
            }
        }
        [Serializable]
        public class SerializeNodeTypeRedirector
        {
            public SerializeNodeType sourceNodeType;
            
            public SerializeNodeType targetNodeType;
            public SerializeNodeTypeRedirector() { }
            public SerializeNodeTypeRedirector(Type sourceType, Type targetType)
            {
                sourceNodeType = new SerializeNodeType(sourceType);
                targetNodeType = new SerializeNodeType(targetType);
            }
        }
        
        public SerializeNodeTypeRedirector[] m_Redirectors;
        public Type Redirect(CeresNodeData.NodeType nodeType)
        {
            var serializeType = new SerializeNodeType(nodeType);
            var redirector = m_Redirectors.FirstOrDefault(x => x.sourceNodeType.nodeType == serializeType.nodeType);
            return redirector?.targetNodeType.Type;
        }
#if UNITY_EDITOR
        public static APIUpdateConfig GetConfig()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(APIUpdateConfig)}");
            if (guids.Length == 0)
            {
                return null;
            }
            return UnityEditor.AssetDatabase.LoadAssetAtPath<APIUpdateConfig>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
        }
#endif
    }
}
