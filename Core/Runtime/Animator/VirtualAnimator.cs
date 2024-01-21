using System;
using System.Collections.Generic;
using Kurisu.Framework.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Animation
{
    /// <summary>
    /// Virtual animator can cross fade multi RuntimeAnimatorController
    /// </summary>
    public class VirtualAnimator : IDisposable
    {
        /// <summary>
        /// Bind animator
        /// </summary>
        /// <value></value>
        public Animator Animator { get; }
        private PlayableGraph playableGraph;
        private AnimationPlayableOutput playableOutput;
        private Playable mixerPointer;
        private Playable rootMixer;
        private AnimatorControllerPlayable playablePointer;
        private RuntimeAnimatorController currentController;
        public bool IsPlaying
        {
            get
            {
                return playableGraph.IsValid() && playableGraph.IsPlaying();
            }
        }
        /// <summary>
        /// Handle for root blending task
        /// </summary>
        private JobHandle rootHandle;
        /// <summary>
        /// Handle for subTree blending task
        /// </summary>
        /// <returns></returns>
        private readonly Dictionary<RuntimeAnimatorController, JobHandle> subHandleMap = new();
        public VirtualAnimator(Animator animator)
        {
            Animator = animator;
            CreateNewGraph();
        }
        private void CreateNewGraph()
        {
            playableGraph = PlayableGraph.Create($"{Animator.gameObject.name}_VirtualAnimator");
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", Animator);
            mixerPointer = rootMixer = AnimationMixerPlayable.Create(playableGraph, 2);
            playableOutput.SetSourcePlayable(rootMixer);
        }
        public void Play(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            if (IsPlaying && currentController == animatorController) return;
            //If graph has root controller, destroy it
            if (playableGraph.IsValid())
            {
                if (mixerPointer.GetInput(1).IsNull())
                {
                    PlayInternal(animatorController, fadeInTime);
                    return;
                }
                else
                {
                    playableGraph.Stop();
                    playableGraph.Destroy();
                }
            }
            CreateNewGraph();
            PlayInternal(animatorController, fadeInTime);
        }
        private void PlayInternal(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            playablePointer = AnimatorControllerPlayable.Create(playableGraph, currentController = animatorController);
            //Connect to second input of mixer
            playableGraph.Connect(playablePointer, 0, rootMixer, 1);
            rootMixer.SetInputWeight(0, 1);
            rootMixer.SetInputWeight(1, 0);
            rootHandle.Cancel();
            if (fadeInTime > 0)
                rootHandle = Task.Schedule(() => FadeIn(rootMixer, 1), x => FadeIn(rootMixer, x / fadeInTime), fadeInTime);
            else
                FadeIn(rootMixer, 1);
            if (!IsPlaying) playableGraph.Play();
        }
        public void CrossFade(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f)
        {
            if (IsPlaying && currentController == animatorController) return;
            //Graph is destroyed, create new graph and play instead
            if (!playableGraph.IsValid())
            {
                CreateNewGraph();
                PlayInternal(animatorController, fadeInTime);
                return;
            }
            //If has no animator controller, use play instead
            if (mixerPointer.GetInput(1).IsNull())
            {
                PlayInternal(animatorController, fadeInTime);
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
            //Disconnect leaf
            playableGraph.Disconnect(mixerPointer, 1);
            //Right=>left
            playableGraph.Connect(right, 0, newMixer, 0);
            //New right leaf
            playableGraph.Connect(playablePointer, 0, newMixer, 1);
            //Connect to parent
            playableGraph.Connect(newMixer, 0, mixerPointer, 1);
            //Update pointer
            mixerPointer = newMixer;
            mixerPointer.SetInputWeight(0, 1);
            mixerPointer.SetInputWeight(1, 0);
            if (subHandleMap.TryGetValue(animatorController, out var handle))
            {
                handle.Cancel();
            }
            if (fadeInTime > 0)
                subHandleMap[animatorController] = Task.Schedule(() => FadeIn(mixerPointer, 1), x => FadeIn(mixerPointer, x / fadeInTime), fadeInTime);
            else
                FadeIn(mixerPointer, 1);
        }
        private void FadeIn(Playable playable, float weight)
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
            if (fadeOutTime < 0)
            {
                SetStop();
                return;
            }
            rootHandle = Task.Schedule(SetStop, x => FadeIn(rootMixer, 1 - x / 0.25f), 0.25f);
        }
        private void SetStop()
        {
            FadeIn(rootMixer, 0);
            playableGraph.Stop();
            playableGraph.Destroy();
            currentController = null;
        }
        public void CrossFade(string stateName, float transitionTime)
        {
            playablePointer.CrossFade(stateName, transitionTime);
        }
        public void CrossFade(int stateNameHash, float transitionTime)
        {
            playablePointer.CrossFade(stateNameHash, transitionTime);
        }
        public void Play(string stateName)
        {
            playablePointer.Play(stateName);
        }
        public void Play(int stateNameHash)
        {
            playablePointer.Play(stateNameHash);
        }
        public void SetFloat(string name, float value)
        {
            playablePointer.SetFloat(name, value);
        }
        public void SetFloat(int id, float value)
        {
            playablePointer.SetFloat(id, value);
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
        public void Dispose()
        {
            if (playableGraph.IsValid())
                playableGraph.Destroy();
        }
    }
}
