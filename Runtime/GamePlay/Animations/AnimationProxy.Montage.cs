using System;
using System.Collections.Generic;
using Kurisu.Framework.Schedulers;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Animations
{
    public partial class AnimationProxy
    {
        public class AnimationPlayableNode : IDisposable
        {
            public PlayableGraph Graph { get; }
            public Playable Playable { get; protected set; }
            public AnimationPlayableNode(Playable playable)
            {
                Graph = playable.GetGraph();
                Playable = playable;
            }
            public static implicit operator AnimationPlayableNode(Playable playable)
            {
                return new AnimationPlayableNode(playable);
            }
            /// <summary>
            /// Destroy playable recursively
            /// </summary>
            public void Destroy()
            {
                Playable playable = Playable;
                while (playable.IsValid())
                {
                    var input = playable.GetInput(0);
                    playable.Destroy();
                    playable = input;
                }
            }
            /// <summary>
            /// Dispose playable resources
            /// </summary>
            public virtual void Dispose()
            {

            }
        }
        public class AnimationMontageNode : AnimationPlayableNode
        {
            public AnimationMixerPlayable Montage => (AnimationMixerPlayable)Playable;
            public AnimationMontageNode Parent;
            public AnimationPlayableNode Child; /* Can be montage or normal playable node */
            public SchedulerHandle BlendHandle;
            public float BlendWeight => Playable.GetInputWeight(1);
            public AnimationMontageNode(AnimationMixerPlayable playable) : base(playable)
            {
            }

            /// <summary>
            /// Whether node is composite root, which should have no left child
            /// </summary>
            /// <returns></returns>
            public bool IsRootMontage()
            {
                return Parent == null;
            }
            /// <summary>
            /// Dispose playable resources recursively
            /// </summary>
            public override void Dispose()
            {
                BlendHandle.Dispose();
                Child?.Dispose();
                Child = null;
                Parent = null;
            }
            /// <summary>
            /// Create root montage without parent
            /// </summary>
            /// <param name="sourcePlayable"></param>
            /// <returns></returns>
            public static AnimationMontageNode CreateRootMontage(Playable sourcePlayable)
            {
                var graph = sourcePlayable.GetGraph();
                var newMixer = AnimationMixerPlayable.Create(graph, 2);

                // Only has child
                graph.Connect(sourcePlayable, 0, newMixer, 1);

                // Set weight
                newMixer.SetInputWeight(0, 1);
                newMixer.SetInputWeight(1, 0);

                return new AnimationMontageNode(newMixer)
                {
                    Parent = null,
                    Child = new AnimationPlayableNode(sourcePlayable)
                };
            }
            /// <summary>
            /// Create montage from montage and new playable
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="source"></param>
            /// <returns></returns>
            public static AnimationMontageNode CreateMontage(AnimationMontageNode parent, AnimationPlayableNode source)
            {
                var playablePtr = source.Playable;
                var graph = parent.Graph;
                var leafMontage = parent.Montage;
                var leafNode = parent.Child;

                // Layout as a binary tree
                var newMontage = AnimationMixerPlayable.Create(graph, 2);

                // Disconnect right leaf from leaf montage
                graph.Disconnect(leafMontage, 1);
                // Current right leaf => New left leaf
                graph.Connect(leafNode.Playable, 0, newMontage, 0);
                // New right leaf
                graph.Connect(playablePtr, 0, newMontage, 1);
                // Connect to parent
                graph.Connect(newMontage, 0, leafMontage, 1);
                // Set weight
                newMontage.SetInputWeight(0, 1);
                newMontage.SetInputWeight(1, 0);

                var newMontageNode = new AnimationMontageNode(newMontage)
                {
                    Parent = parent,
                    Child = source
                };
                // Link child
                parent.Child = newMontageNode;

                return newMontageNode;
            }
            /// <summary>
            /// Crossfade parent to child by weight
            /// </summary>
            /// <param name="weight"></param>
            public void Blend(float weight)
            {
                Playable.SetInputWeight(0, 1 - weight);
                Playable.SetInputWeight(1, weight);
            }
            /// <summary>
            /// Shrink link list to release not used playables
            /// </summary>
            /// <returns>Current leaf node</returns>
            public AnimationMontageNode Shrink()
            {
                if (BlendWeight != 1 || Parent == null)
                {
                    return this;
                }
                // Disconnect child output first
                Graph.Disconnect(Montage, 1);
                Parent.SetChild(Child);
                var parent = Parent;
                // Release playable
                Child = null;
                Parent = null;
                Destroy();
                return parent.Shrink();
            }
            private void SetChild(AnimationPlayableNode newChild)
            {
                Child = newChild;
                Graph.Disconnect(Montage, 1);
                Graph.Connect(newChild.Playable, 0, Montage, 1);
            }
            public void ScheduleBlendIn(float duration, Action callBack = null)
            {
                Scheduler.Delay(ref BlendHandle, duration, () => { Blend(1); callBack?.Invoke(); }, x => Blend(x / duration));
            }
            public void ScheduleBlendOut(float duration, Action callBack = null)
            {
                Scheduler.Delay(ref BlendHandle, duration, () => { Blend(0); callBack?.Invoke(); }, x => Blend(1 - x / duration));
            }
            public static AnimationMontageNode operator |(AnimationMontageNode left, AnimationPlayableNode right)
            {
                return CreateMontage(left, right);
            }
        }
        #region Wrapper
        public virtual float GetFloat(string name)
        {
            return LeafAnimatorPlayable.GetFloat(name);
        }

        public virtual float GetFloat(int id)
        {
            return LeafAnimatorPlayable.GetFloat(id);
        }
        public virtual void SetFloat(string name, float value)
        {
            LeafAnimatorPlayable.SetFloat(name, value);
        }
        public virtual void SetFloat(int id, float value)
        {
            LeafAnimatorPlayable.SetFloat(id, value);
        }
        public virtual bool GetBool(string name)
        {
            return LeafAnimatorPlayable.GetBool(name);
        }
        public virtual bool GetBool(int id)
        {
            return LeafAnimatorPlayable.GetBool(id);
        }
        public virtual void SetBool(string name, bool value)
        {
            LeafAnimatorPlayable.SetBool(name, value);
        }

        public virtual void SetBool(int id, bool value)
        {
            LeafAnimatorPlayable.SetBool(id, value);
        }

        public virtual int GetInteger(string name)
        {
            return LeafAnimatorPlayable.GetInteger(name);
        }
        public virtual int GetInteger(int id)
        {
            return LeafAnimatorPlayable.GetInteger(id);
        }
        public virtual void SetInteger(string name, int value)
        {
            LeafAnimatorPlayable.SetInteger(name, value);
        }

        public virtual void SetInteger(int id, int value)
        {
            LeafAnimatorPlayable.SetInteger(id, value);
        }
        public virtual void SetTrigger(string name)
        {
            LeafAnimatorPlayable.SetTrigger(name);
        }

        public virtual void SetTrigger(int id)
        {
            LeafAnimatorPlayable.SetTrigger(id);
        }

        public virtual void ResetTrigger(string name)
        {
            LeafAnimatorPlayable.ResetTrigger(name);
        }
        public virtual void ResetTrigger(int id)
        {
            LeafAnimatorPlayable.ResetTrigger(id);
        }
        public virtual bool IsParameterControlledByCurve(string name)
        {
            return LeafAnimatorPlayable.IsParameterControlledByCurve(name);
        }
        public virtual bool IsParameterControlledByCurve(int id)
        {
            return LeafAnimatorPlayable.IsParameterControlledByCurve(id);
        }

        public virtual int GetLayerCount()
        {
            return LeafAnimatorPlayable.GetLayerCount();
        }

        public virtual string GetLayerName(int layerIndex)
        {
            return LeafAnimatorPlayable.GetLayerName(layerIndex);
        }

        public virtual int GetLayerIndex(string layerName)
        {
            return LeafAnimatorPlayable.GetLayerIndex(layerName);
        }
        public virtual float GetLayerWeight(int layerIndex)
        {
            return LeafAnimatorPlayable.GetLayerWeight(layerIndex);
        }
        public virtual void SetLayerWeight(int layerIndex, float weight)
        {
            LeafAnimatorPlayable.SetLayerWeight(layerIndex, weight);
        }

        public virtual AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
        {
            return LeafAnimatorPlayable.GetCurrentAnimatorStateInfo(layerIndex);
        }

        public virtual AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex)
        {
            return LeafAnimatorPlayable.GetNextAnimatorStateInfo(layerIndex);
        }

        public virtual AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex)
        {
            return LeafAnimatorPlayable.GetAnimatorTransitionInfo(layerIndex);
        }

        public virtual AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex)
        {
            return LeafAnimatorPlayable.GetCurrentAnimatorClipInfo(layerIndex);
        }

        public virtual void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
        {
            LeafAnimatorPlayable.GetCurrentAnimatorClipInfo(layerIndex, clips);
        }

        public virtual void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
        {
            LeafAnimatorPlayable.GetNextAnimatorClipInfo(layerIndex, clips);
        }
        public virtual int GetCurrentAnimatorClipInfoCount(int layerIndex)
        {
            return LeafAnimatorPlayable.GetCurrentAnimatorClipInfoCount(layerIndex);
        }
        public virtual int GetNextAnimatorClipInfoCount(int layerIndex)
        {
            return LeafAnimatorPlayable.GetNextAnimatorClipInfoCount(layerIndex);
        }

        public virtual AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex)
        {
            return LeafAnimatorPlayable.GetNextAnimatorClipInfo(layerIndex);
        }
        public virtual bool IsInTransition(int layerIndex)
        {
            return LeafAnimatorPlayable.IsInTransition(layerIndex);
        }

        public virtual int GetParameterCount()
        {
            return LeafAnimatorPlayable.GetParameterCount();
        }

        public virtual AnimatorControllerParameter GetParameter(int index)
        {
            return LeafAnimatorPlayable.GetParameter(index);
        }

        public virtual void CrossFadeInFixedTime(string stateName, float transitionDuration, int layer = -1, float fixedTime = 0f)
        {
            LeafAnimatorPlayable.CrossFadeInFixedTime(stateName, transitionDuration, layer, fixedTime);
        }
        public virtual void CrossFadeInFixedTime(int stateNameHash, float transitionDuration, int layer = -1, float fixedTime = 0.0f)
        {
            LeafAnimatorPlayable.CrossFadeInFixedTime(stateNameHash, transitionDuration, layer, fixedTime);
        }
        public virtual void CrossFade(string stateName, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.CrossFade(stateName, transitionDuration, layer, normalizedTime);
        }

        public virtual void CrossFade(int stateNameHash, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
        }
        public virtual void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.PlayInFixedTime(stateName, layer, fixedTime);
        }

        public virtual void PlayInFixedTime(int stateNameHash, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.PlayInFixedTime(stateNameHash, layer, fixedTime);
        }

        public virtual void Play(string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.Play(stateName, layer, normalizedTime);
        }


        public virtual void Play(int stateNameHash, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            LeafAnimatorPlayable.Play(stateNameHash, layer, normalizedTime);
        }

        public virtual bool HasState(int layerIndex, int stateID)
        {
            return LeafAnimatorPlayable.HasState(layerIndex, stateID);
        }
        #endregion Public API
    }
}
