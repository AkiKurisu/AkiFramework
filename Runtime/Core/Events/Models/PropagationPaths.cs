using System;
using System.Collections.Generic;
using Kurisu.Framework.Pool;
namespace Kurisu.Framework.Events
{
    internal class PropagationPaths
    {
        static readonly ObjectPool<PropagationPaths> s_Pool = new(() => new PropagationPaths());

        [Flags]
        public enum Type
        {
            None = 0,
            TrickleDown = 1,
            BubbleUp = 2
        }

        public readonly List<CallbackEventHandler> trickleDownPath;
        public readonly List<CallbackEventHandler> targetElements;
        public readonly List<CallbackEventHandler> bubbleUpPath;

        private const int k_DefaultPropagationDepth = 16;
        private const int k_DefaultTargetCount = 4;

        public PropagationPaths()
        {
            trickleDownPath = new List<CallbackEventHandler>(k_DefaultPropagationDepth);
            targetElements = new List<CallbackEventHandler>(k_DefaultTargetCount);
            bubbleUpPath = new List<CallbackEventHandler>(k_DefaultPropagationDepth);
        }

        public PropagationPaths(PropagationPaths paths)
        {
            trickleDownPath = new List<CallbackEventHandler>(paths.trickleDownPath);
            targetElements = new List<CallbackEventHandler>(paths.targetElements);
            bubbleUpPath = new List<CallbackEventHandler>(paths.bubbleUpPath);
        }

        internal static PropagationPaths Copy(PropagationPaths paths)
        {
            PropagationPaths copyPaths = s_Pool.Get();
            copyPaths.trickleDownPath.AddRange(paths.trickleDownPath);
            copyPaths.targetElements.AddRange(paths.targetElements);
            copyPaths.bubbleUpPath.AddRange(paths.bubbleUpPath);

            return copyPaths;
        }

        public static PropagationPaths Build(CallbackEventHandler elem, EventBase evt)
        {
            PropagationPaths paths = s_Pool.Get();
            // Go through the entire hierarchy.
            for (var ve = elem.Parent; ve != null; ve = ve.Parent)
            {
                //Reach root
                if (ve.IsCompositeRoot && !evt.SkipDisabledElements)
                {
                    paths.targetElements.Add(ve);
                }
                else
                {
                    if (evt.TricklesDown && ve.HasTrickleDownHandlers())
                    {
                        paths.trickleDownPath.Add(ve);
                    }
                    if (evt.Bubbles && ve.HasBubbleUpHandlers())
                    {
                        paths.bubbleUpPath.Add(ve);
                    }
                }
            }
            return paths;
        }

        public void Release()
        {
            // Empty paths to avoid leaking CallbackEventHandler.
            bubbleUpPath.Clear();
            targetElements.Clear();
            trickleDownPath.Clear();

            s_Pool.Release(this);
        }
    }
}
