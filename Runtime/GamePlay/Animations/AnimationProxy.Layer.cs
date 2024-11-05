using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UAnimator = UnityEngine.Animator;
namespace Kurisu.Framework.Animations
{
    /// <summary>
    /// Proxy layer handle
    /// </summary>
    public struct LayerHandle : IEquatable<LayerHandle>
    {
        public int Id;

        public static bool operator ==(LayerHandle left, LayerHandle right)
        {
            return left.Id == right.Id;
        }
        public static bool operator !=(LayerHandle left, LayerHandle right)
        {
            return left.Id != right.Id;
        }
        public override readonly bool Equals(object obj)
        {
            if (obj is not LayerHandle handle) return false;
            return handle.Id == Id;
        }
        public readonly bool Equals(LayerHandle other)
        {
            return other.Id == Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public readonly bool IsValid()
        {
            return Id != 0;
        }
    }
    /// <summary>
    /// Proxy layer descriptor
    /// </summary>
    public struct LayerDescriptor : IEquatable<LayerDescriptor>
    {
        public string Name;
        public uint Index;
        public bool Additive;
        public AvatarMask AvatarMask;

        public readonly bool Equals(LayerDescriptor other)
        {
            return Index == other.Index && Additive == other.Additive
                    && AvatarMask == other.AvatarMask && Name == other.Name;
        }
    }
    public partial class AnimationProxy
    {
        /// <summary>
        /// Runtime proxy layer context
        /// </summary>
        public class LayerContext
        {
            public LayerHandle Handle;
            public LayerDescriptor Descriptor;
            public AnimationLayerMontageNode MontageNode;
            public static readonly LayerContext Empty = new();
        }
        public readonly Dictionary<LayerHandle, LayerContext> _layerContexts = new();
        /// <summary>
        /// Get or create a new montage layer
        /// </summary>
        /// <param name="layerHandle"></param>
        /// <param name="layerName"></param>
        /// <param name="layerIndex"></param>
        /// <param name="avatarMask"></param>
        public void CreateLayer(ref LayerHandle layerHandle, string layerName, uint layerIndex = 0, AvatarMask avatarMask = null)
        {
            var descriptor = new LayerDescriptor()
            {
                Name = layerName,
                Index = layerIndex,
                AvatarMask = avatarMask
            };
            CreateLayer(ref layerHandle, descriptor);
        }
        /// <summary>
        ///  Get or create a new montage layer
        /// </summary>
        /// <param name="layerHandle"></param>
        /// <param name="layerDescriptor"></param>
        public void CreateLayer(ref LayerHandle layerHandle, LayerDescriptor layerDescriptor)
        {
            layerHandle = new LayerHandle()
            {
                Id = UAnimator.StringToHash(layerDescriptor.Name)
            };
            if (_layerContexts.TryGetValue(layerHandle, out var context))
            {
                /* Skip if is matched with current layer */
                if (context.Descriptor.Equals(layerDescriptor))
                {
                    return;
                }
                /* We should not modify graph when executing montage with previews descriptor */
                if (context.MontageNode != null && context.MontageNode.IsValid())
                {
                    Debug.LogError($"[AnimationProxy] Can not create new layer when another layer named {layerDescriptor.Name} is executing");
                    return;
                }
                _layerContexts.Remove(layerHandle);
            }
            context = new LayerContext()
            {
                Handle = layerHandle,
                Descriptor = layerDescriptor,
                MontageNode = null,
            };
            _layerContexts.Add(layerHandle, context);
        }
        public LayerContext GetLayerContext(LayerHandle handle)
        {
            if (_layerContexts.TryGetValue(handle, out var layerContext))
            {
                return layerContext;
            }
            return LayerContext.Empty;
        }
        public LayerContext[] GetAllLayerContexts()
        {
            return _layerContexts.Values.OrderBy(x => x.Descriptor.Index).ToArray();
        }
        public int GetLayerIndex(LayerHandle layerHandle)
        {
            if (RootMontage is AnimationLayerMontageNode)
            {
                var context = GetLayerContext(layerHandle);
                if (context.Handle.IsValid())
                    return (int)context.Descriptor.Index;
            }
            return DefaultLayerIndex;
        }
        public readonly struct AnimationClipInstanceProxy
        {
            private readonly AnimationClipPlayable clipPlayable;
            internal AnimationClipInstanceProxy(AnimationClipPlayable playable)
            {
                clipPlayable = playable;
            }
            #region Wrapper
            /// <summary>
            /// Returns the AnimationClip stored in the AnimationClipPlayable.
            /// </summary>
            /// <returns></returns>
            public AnimationClip GetAnimationClip()
            {
                return clipPlayable.GetAnimationClip();
            }

            /// <summary>
            /// Returns the state of the ApplyFootIK flag.
            /// </summary>
            /// <returns></returns>
            public bool GetApplyFootIK()
            {
                return clipPlayable.GetApplyFootIK();
            }

            /// <summary>
            ///  Sets the value of the ApplyFootIK flag.
            /// </summary>
            /// <param name="value"></param>
            public void SetApplyFootIK(bool value)
            {
                clipPlayable.SetApplyFootIK(value);
            }

            /// <summary>
            /// Returns the state of the ApplyPlayableIK flag.
            /// </summary>
            /// <returns></returns>
            public bool GetApplyPlayableIK()
            {
                return clipPlayable.GetApplyPlayableIK();
            }

            /// <summary>
            /// Requests OnAnimatorIK to be called on the animated GameObject.
            /// </summary>
            /// <param name="value"></param>
            public void SetApplyPlayableIK(bool value)
            {
                clipPlayable.SetApplyPlayableIK(value);
            }
            #endregion Wrapper
        }
        public readonly struct AnimatorControllerInstanceProxy
        {
            private readonly AnimatorControllerPlayable animatorPlayable;
            private readonly RuntimeAnimatorController animatorController;
            internal AnimatorControllerInstanceProxy(AnimatorControllerPlayable playable, RuntimeAnimatorController sourceController)
            {
                animatorPlayable = playable;
                animatorController = sourceController;
            }
            public RuntimeAnimatorController GetAnimatorController()
            {
                return animatorController;
            }
            #region Wrapper
            public float GetFloat(string name)
            {
                return animatorPlayable.GetFloat(name);
            }

            public float GetFloat(int id)
            {
                return animatorPlayable.GetFloat(id);
            }
            public void SetFloat(string name, float value)
            {
                animatorPlayable.SetFloat(name, value);
            }
            public void SetFloat(int id, float value)
            {
                animatorPlayable.SetFloat(id, value);
            }
            public bool GetBool(string name)
            {
                return animatorPlayable.GetBool(name);
            }
            public bool GetBool(int id)
            {
                return animatorPlayable.GetBool(id);
            }
            public void SetBool(string name, bool value)
            {
                animatorPlayable.SetBool(name, value);
            }

            public void SetBool(int id, bool value)
            {
                animatorPlayable.SetBool(id, value);
            }

            public int GetInteger(string name)
            {
                return animatorPlayable.GetInteger(name);
            }
            public int GetInteger(int id)
            {
                return animatorPlayable.GetInteger(id);
            }
            public void SetInteger(string name, int value)
            {
                animatorPlayable.SetInteger(name, value);
            }

            public void SetInteger(int id, int value)
            {
                animatorPlayable.SetInteger(id, value);
            }
            public void SetTrigger(string name)
            {
                animatorPlayable.SetTrigger(name);
            }

            public void SetTrigger(int id)
            {
                animatorPlayable.SetTrigger(id);
            }

            public void ResetTrigger(string name)
            {
                animatorPlayable.ResetTrigger(name);
            }
            public void ResetTrigger(int id)
            {
                animatorPlayable.ResetTrigger(id);
            }
            public bool IsParameterControlledByCurve(string name)
            {
                return animatorPlayable.IsParameterControlledByCurve(name);
            }
            public bool IsParameterControlledByCurve(int id)
            {
                return animatorPlayable.IsParameterControlledByCurve(id);
            }

            public int GetLayerCount()
            {
                return animatorPlayable.GetLayerCount();
            }

            public string GetLayerName(int layerIndex)
            {
                return animatorPlayable.GetLayerName(layerIndex);
            }

            public int GetLayerIndex(string layerName)
            {
                return animatorPlayable.GetLayerIndex(layerName);
            }
            public float GetLayerWeight(int layerIndex)
            {
                return animatorPlayable.GetLayerWeight(layerIndex);
            }
            public void SetLayerWeight(int layerIndex, float weight)
            {
                animatorPlayable.SetLayerWeight(layerIndex, weight);
            }

            public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
            {
                return animatorPlayable.GetCurrentAnimatorStateInfo(layerIndex);
            }

            public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex)
            {
                return animatorPlayable.GetNextAnimatorStateInfo(layerIndex);
            }

            public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex)
            {
                return animatorPlayable.GetAnimatorTransitionInfo(layerIndex);
            }

            public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex)
            {
                return animatorPlayable.GetCurrentAnimatorClipInfo(layerIndex);
            }

            public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
            {
                animatorPlayable.GetCurrentAnimatorClipInfo(layerIndex, clips);
            }

            public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
            {
                animatorPlayable.GetNextAnimatorClipInfo(layerIndex, clips);
            }
            public int GetCurrentAnimatorClipInfoCount(int layerIndex)
            {
                return animatorPlayable.GetCurrentAnimatorClipInfoCount(layerIndex);
            }
            public int GetNextAnimatorClipInfoCount(int layerIndex)
            {
                return animatorPlayable.GetNextAnimatorClipInfoCount(layerIndex);
            }

            public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex)
            {
                return animatorPlayable.GetNextAnimatorClipInfo(layerIndex);
            }
            public bool IsInTransition(int layerIndex)
            {
                return animatorPlayable.IsInTransition(layerIndex);
            }

            public int GetParameterCount()
            {
                return animatorPlayable.GetParameterCount();
            }

            public AnimatorControllerParameter GetParameter(int index)
            {
                return animatorPlayable.GetParameter(index);
            }

            public void CrossFadeInFixedTime(string stateName, float transitionDuration, int layer = -1, float fixedTime = 0f)
            {
                animatorPlayable.CrossFadeInFixedTime(stateName, transitionDuration, layer, fixedTime);
            }
            public void CrossFadeInFixedTime(int stateNameHash, float transitionDuration, int layer = -1, float fixedTime = 0.0f)
            {
                animatorPlayable.CrossFadeInFixedTime(stateNameHash, transitionDuration, layer, fixedTime);
            }
            public void CrossFade(string stateName, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
            {
                animatorPlayable.CrossFade(stateName, transitionDuration, layer, normalizedTime);
            }

            public void CrossFade(int stateNameHash, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
            {
                animatorPlayable.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
            }
            public void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = float.NegativeInfinity)
            {
                animatorPlayable.PlayInFixedTime(stateName, layer, fixedTime);
            }

            public void PlayInFixedTime(int stateNameHash, int layer = -1, float fixedTime = float.NegativeInfinity)
            {
                animatorPlayable.PlayInFixedTime(stateNameHash, layer, fixedTime);
            }

            public void Play(string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity)
            {
                animatorPlayable.Play(stateName, layer, normalizedTime);
            }

            public void Play(int stateNameHash, int layer = -1, float normalizedTime = float.NegativeInfinity)
            {
                animatorPlayable.Play(stateNameHash, layer, normalizedTime);
            }

            public bool HasState(int layerIndex, int stateID)
            {
                return animatorPlayable.HasState(layerIndex, stateID);
            }
            #endregion Public API
        }
    }
}