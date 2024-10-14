using System;
using System.Collections.Generic;
using Kurisu.Framework.Schedulers;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Animations
{
    // TODO: Refactor: Release animator already blend out

    /// <summary>
    /// Animation proxy that can cross fade multi <see cref="RuntimeAnimatorController"/>
    /// </summary>
    /// <remarks>
    /// Useful to override default animation in cutscene and dialogue.
    /// </remarks>
    public class AnimationProxy : IDisposable
    {
        /// <summary>
        /// Bind animator
        /// </summary>
        /// <value></value>
        public Animator Animator { get; }
        protected RuntimeAnimatorController sourceController;
        protected PlayableGraph playableGraph;
        protected AnimationPlayableOutput playableOutput;
        protected Playable mixerPointer;
        protected Playable rootMixer;
        protected AnimatorControllerPlayable playablePointer;
        protected RuntimeAnimatorController currentController;
        /// <summary>
        /// Handle for root blending task
        /// </summary>
        protected SchedulerHandle rootHandle;
        /// <summary>
        /// Handle for subTree blending task
        /// </summary>
        /// <returns></returns>
        protected readonly Dictionary<RuntimeAnimatorController, SchedulerHandle> subHandleMap = new();
        protected bool isFadeOut;
        public bool IsPlaying
        {
            get
            {
                return playableGraph.IsValid() && playableGraph.IsPlaying();
            }
        }
        public AnimationProxy(Animator animator)
        {
            Animator = animator;
        }
        public virtual void Dispose()
        {
            currentController = null;
            sourceController = null;
            if (playableGraph.IsValid())
                playableGraph.Destroy();
        }
        private void CreateNewGraph()
        {
            playableGraph = PlayableGraph.Create($"{Animator.gameObject.name}_Playable");
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", Animator);
            mixerPointer = rootMixer = AnimationMixerPlayable.Create(playableGraph, 2);
            playableOutput.SetSourcePlayable(rootMixer);
        }
        public void LoadAnimation(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            if (isFadeOut)
            {
                rootHandle.Cancel();
                SetOutGraph();
            }
            if (IsPlaying && currentController == animatorController) return;
            // If graph has root controller, destroy it
            if (playableGraph.IsValid())
            {
                if (mixerPointer.GetInput(1).IsNull())
                {
                    LoadAnimation_Imp(animatorController, fadeInTime);
                    return;
                }
                else
                {
                    playableGraph.Stop();
                    playableGraph.Destroy();
                }
            }
            CreateNewGraph();
            LoadAnimation_Imp(animatorController, fadeInTime);
        }
        protected void LoadAnimation_Imp(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            sourceController = Animator.runtimeAnimatorController;
            playablePointer = AnimatorControllerPlayable.Create(playableGraph, currentController = animatorController);
            //Connect to second input of mixer
            playableGraph.Connect(playablePointer, 0, rootMixer, 1);
            rootMixer.SetInputWeight(0, 1);
            rootMixer.SetInputWeight(1, 0);
            rootHandle.Cancel();
            if (fadeInTime > 0)
                rootHandle = Scheduler.Delay(fadeInTime, SetInGraph, x => Blend(rootMixer, x / fadeInTime));
            else
                SetInGraph();
            if (!IsPlaying) playableGraph.Play();
        }
        private void SetInGraph()
        {
            Blend(rootMixer, 1);
            Animator.runtimeAnimatorController = null;
        }
        /// <summary>
        /// Cross fade to a new animator controller
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="fadeInTime"></param>
        public void CrossFade(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            if (isFadeOut)
            {
                rootHandle.Cancel();
                SetOutGraph();
            }
            if (IsPlaying && currentController == animatorController) return;
            //Graph is destroyed, create new graph and play instead
            if (!playableGraph.IsValid())
            {
                CreateNewGraph();
                LoadAnimation_Imp(animatorController, fadeInTime);
                return;
            }
            //If has no animator controller, use play instead
            if (mixerPointer.GetInput(1).IsNull())
            {
                LoadAnimation_Imp(animatorController, fadeInTime);
            }
            else
            {
                CrossFadeInternal(animatorController, fadeInTime);
            }
        }
        private void CrossFadeInternal(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            playablePointer = AnimatorControllerPlayable.Create(playableGraph, currentController = animatorController);
            // Layout as a binary tree
            var newMixer = AnimationMixerPlayable.Create(playableGraph, 2);
            var right = mixerPointer.GetInput(1);
            // Disconnect leaf
            playableGraph.Disconnect(mixerPointer, 1);
            // Right=>left
            playableGraph.Connect(right, 0, newMixer, 0);
            // New right leaf
            playableGraph.Connect(playablePointer, 0, newMixer, 1);
            // Connect to parent
            playableGraph.Connect(newMixer, 0, mixerPointer, 1);
            // Update pointer
            mixerPointer = newMixer;
            mixerPointer.SetInputWeight(0, 1);
            mixerPointer.SetInputWeight(1, 0);
            if (subHandleMap.TryGetValue(animatorController, out var handle))
            {
                handle.Cancel();
            }
            if (fadeInTime > 0)
                subHandleMap[animatorController] = Scheduler.Delay(fadeInTime, () => Blend(mixerPointer, 1), x => Blend(mixerPointer, x / fadeInTime));
            else
                Blend(mixerPointer, 1);
        }
        /// <summary>
        /// Utils function for blend playable with tow childs 
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="weight"></param>
        public static void Blend(Playable playable, float weight)
        {
            playable.SetInputWeight(0, 1 - weight);
            playable.SetInputWeight(1, weight);
        }
        public void Stop(float fadeOutTime = 0.25f)
        {
            if (!IsPlaying) return;
            foreach (var handle in subHandleMap.Values)
            {
                handle.Cancel();
            }
            subHandleMap.Clear();
            rootHandle.Cancel();
            Animator.runtimeAnimatorController = sourceController;
            if (fadeOutTime < 0)
            {
                SetOutGraph();
                return;
            }
            isFadeOut = true;
            rootHandle = Scheduler.Delay(fadeOutTime, SetOutGraph, x => Blend(rootMixer, 1 - x / fadeOutTime));
        }
        protected virtual void SetOutGraph()
        {
            Blend(rootMixer, 0);
            isFadeOut = false;
            playableGraph.Stop();
            playableGraph.Destroy();
            currentController = null;
        }
        public bool IsCurrent(RuntimeAnimatorController runtimeAnimatorController)
        {
            return currentController == runtimeAnimatorController;
        }
        #region Wrap
        public float GetFloat(string name)
        {
            return playablePointer.GetFloat(name);
        }

        public float GetFloat(int id)
        {
            return playablePointer.GetFloat(id);
        }
        public void SetFloat(string name, float value)
        {
            playablePointer.SetFloat(name, value);
        }
        public void SetFloat(int id, float value)
        {
            playablePointer.SetFloat(id, value);
        }
        public bool GetBool(string name)
        {
            return playablePointer.GetBool(name);
        }
        public bool GetBool(int id)
        {
            return playablePointer.GetBool(id);
        }
        public void SetBool(string name, bool value)
        {
            playablePointer.SetBool(name, value);
        }

        public void SetBool(int id, bool value)
        {
            playablePointer.SetBool(id, value);
        }

        public int GetInteger(string name)
        {
            return playablePointer.GetInteger(name);
        }
        public int GetInteger(int id)
        {
            return playablePointer.GetInteger(id);
        }
        public void SetInteger(string name, int value)
        {
            playablePointer.SetInteger(name, value);
        }

        public void SetInteger(int id, int value)
        {
            playablePointer.SetInteger(id, value);
        }
        public void SetTrigger(string name)
        {
            playablePointer.SetTrigger(name);
        }

        public void SetTrigger(int id)
        {
            playablePointer.SetTrigger(id);
        }

        public void ResetTrigger(string name)
        {
            playablePointer.ResetTrigger(name);
        }
        public void ResetTrigger(int id)
        {
            playablePointer.ResetTrigger(id);
        }
        public bool IsParameterControlledByCurve(string name)
        {
            return playablePointer.IsParameterControlledByCurve(name);
        }
        public bool IsParameterControlledByCurve(int id)
        {
            return playablePointer.IsParameterControlledByCurve(id);
        }

        public int GetLayerCount()
        {
            return playablePointer.GetLayerCount();
        }

        public string GetLayerName(int layerIndex)
        {
            return playablePointer.GetLayerName(layerIndex);
        }

        public int GetLayerIndex(string layerName)
        {
            return playablePointer.GetLayerIndex(layerName);
        }
        public float GetLayerWeight(int layerIndex)
        {
            return playablePointer.GetLayerWeight(layerIndex);
        }
        public void SetLayerWeight(int layerIndex, float weight)
        {
            playablePointer.SetLayerWeight(layerIndex, weight);
        }

        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
        {
            return playablePointer.GetCurrentAnimatorStateInfo(layerIndex);
        }

        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex)
        {
            return playablePointer.GetNextAnimatorStateInfo(layerIndex);
        }

        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex)
        {
            return playablePointer.GetAnimatorTransitionInfo(layerIndex);
        }

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex)
        {
            return playablePointer.GetCurrentAnimatorClipInfo(layerIndex);
        }

        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
        {
            playablePointer.GetCurrentAnimatorClipInfo(layerIndex, clips);
        }

        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips)
        {
            playablePointer.GetNextAnimatorClipInfo(layerIndex, clips);
        }
        public int GetCurrentAnimatorClipInfoCount(int layerIndex)
        {
            return playablePointer.GetCurrentAnimatorClipInfoCount(layerIndex);
        }
        public int GetNextAnimatorClipInfoCount(int layerIndex)
        {
            return playablePointer.GetNextAnimatorClipInfoCount(layerIndex);
        }

        public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex)
        {
            return playablePointer.GetNextAnimatorClipInfo(layerIndex);
        }
        public bool IsInTransition(int layerIndex)
        {
            return playablePointer.IsInTransition(layerIndex);
        }

        public int GetParameterCount()
        {
            return playablePointer.GetParameterCount();
        }

        public AnimatorControllerParameter GetParameter(int index)
        {
            return playablePointer.GetParameter(index);
        }

        public void CrossFadeInFixedTime(string stateName, float transitionDuration, int layer = -1, float fixedTime = 0f)
        {
            playablePointer.CrossFadeInFixedTime(stateName, transitionDuration, layer, fixedTime);
        }
        public void CrossFadeInFixedTime(int stateNameHash, float transitionDuration, int layer = -1, float fixedTime = 0.0f)
        {
            playablePointer.CrossFadeInFixedTime(stateNameHash, transitionDuration, layer, fixedTime);
        }
        public void CrossFade(string stateName, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            playablePointer.CrossFade(stateName, transitionDuration, layer, normalizedTime);
        }

        public void CrossFade(int stateNameHash, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            playablePointer.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
        }
        public void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            playablePointer.PlayInFixedTime(stateName, layer, fixedTime);
        }

        public void PlayInFixedTime(int stateNameHash, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            playablePointer.PlayInFixedTime(stateNameHash, layer, fixedTime);
        }

        public void Play(string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            playablePointer.Play(stateName, layer, normalizedTime);
        }


        public void Play(int stateNameHash, int layer = -1, float normalizedTime = float.NegativeInfinity)
        {
            playablePointer.Play(stateNameHash, layer, normalizedTime);
        }

        public bool HasState(int layerIndex, int stateID)
        {
            return playablePointer.HasState(layerIndex, stateID);
        }
        #endregion
    }
}
