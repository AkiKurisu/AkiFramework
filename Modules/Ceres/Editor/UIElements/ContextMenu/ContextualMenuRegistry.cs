using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public enum ContextualMenuType
    {
        Graph, Node
    }
    public interface IContextualMenuBuilder
    {
        bool CanBuild(Type constraintBehaviorType);
        ContextualMenuType MenuType { get; }
        void BuildContextualMenu(ContextualMenuPopulateEvent evt);
    }
    public class ContextualMenuRegistry
    {
        private readonly Dictionary<Type, IContextualMenuBuilder> _builderMap = new();
        public void Register(Type type, IContextualMenuBuilder builder)
        {
            _builderMap[type] = builder;
        }
        public void Register<T>(IContextualMenuBuilder builder)
        {
            Register(typeof(T), builder);
        }
        public void UnRegister<T>()
        {
            UnRegister(typeof(T));
        }
        public void UnRegister(Type nodeType)
        {
            if (_builderMap.ContainsKey(nodeType))
                _builderMap.Remove(nodeType);
        }
        public void BuildContextualMenu(ContextualMenuType menuType, ContextualMenuPopulateEvent evt, Type constraintType)
        {
            foreach (var builder in _builderMap.Values)
            {
                if (!builder.CanBuild(constraintType))
                    continue;
                if (builder.MenuType == menuType)
                    builder.BuildContextualMenu(evt);
            }
        }
    }
    public class ContextualMenuBuilder : IContextualMenuBuilder
    {
        public ContextualMenuType MenuType { get; }
        
        private readonly Func<Type, bool> _canBuildFunc;
        
        private readonly Action<ContextualMenuPopulateEvent> _onBuildContextualMenu;
        public ContextualMenuBuilder(ContextualMenuType contextualMenuType, Func<Type, bool> canBuildFunc, Action<ContextualMenuPopulateEvent> onBuildContextualMenu)
        {
            MenuType = contextualMenuType;
            _canBuildFunc = canBuildFunc;
            _onBuildContextualMenu = onBuildContextualMenu;
        }
        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            _onBuildContextualMenu(evt);
        }
        public bool CanBuild(Type constraintBehaviorType)
        {
            return _canBuildFunc(constraintBehaviorType);
        }
    }
}