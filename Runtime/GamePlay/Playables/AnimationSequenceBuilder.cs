using System;
using System.Collections.Generic;
using Kurisu.Framework.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Playables.Tasks
{
    /// <summary>
    /// Provide an animation sequence using UnityEngine.Playables API
    /// </summary>
    public class AnimationSequenceBuilder : IDisposable
    {
        private readonly PlayableGraph playableGraph;
        private AnimationPlayableOutput playableOutput;
        private readonly List<TaskBase> taskBuffer = new();
        private Playable rootMixer;
        private Playable mixerPointer;
        private SequenceTask sequence;
        private float fadeOutTime = 0f;
        public AnimationSequenceBuilder(Animator animator)
        {
            playableGraph = PlayableGraph.Create($"{animator.name}_AnimationSequence_{GetHashCode()}");
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            mixerPointer = rootMixer = AnimationMixerPlayable.Create(playableGraph, 2);
            playableOutput.SetSourcePlayable(mixerPointer);
        }
        /// <summary>
        /// Append an animation clip
        /// </summary>
        /// <param name="animationClip">Clip to play</param>
        /// <param name="fadeIn">FadeIn time</param>
        /// <returns></returns>
        public AnimationSequenceBuilder Append(AnimationClip animationClip, float fadeIn)
        {
            return Append(animationClip, animationClip.length, fadeIn);
        }
        /// <summary>
        /// Append an animation clip
        /// </summary>
        /// <param name="animationClip">Clip to play</param>
        /// <param name="duration">Duration can be infinity as loop</param>
        /// <param name="fadeIn">FadeIn time</param>
        /// <returns></returns>
        public AnimationSequenceBuilder Append(AnimationClip animationClip, float duration, float fadeIn)
        {
            if (IsBuilt()) return this;
            if (!IsValid()) return this;
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
            clipPlayable.SetDuration(duration);
            clipPlayable.SetSpeed(0d);
            return AppendInternal(clipPlayable, fadeIn);
        }
        private AnimationSequenceBuilder AppendInternal(Playable clipPlayable, float fadeIn)
        {
            if (mixerPointer.GetInput(1).IsNull())
            {
                playableGraph.Connect(clipPlayable, 0, mixerPointer, 1);
            }
            else
            {
                // Layout as a binary tree
                var newMixer = AnimationMixerPlayable.Create(playableGraph, 2);
                var right = mixerPointer.GetInput(1);
                taskBuffer.Add(WaitPlayableTask.GetPooled(right, right.GetDuration() - fadeIn));
                //Disconnect leaf
                playableGraph.Disconnect(mixerPointer, 1);
                //Right=>left
                playableGraph.Connect(right, 0, newMixer, 0);
                //New right leaf
                playableGraph.Connect(clipPlayable, 0, newMixer, 1);
                //Connect to parent
                playableGraph.Connect(newMixer, 0, mixerPointer, 1);
                //Update pointer
                mixerPointer = newMixer;
            }
            mixerPointer.SetInputWeight(0, 1);
            mixerPointer.SetInputWeight(1, 0);
            taskBuffer.Add(FadeInPlayableTask.GetPooled(mixerPointer, clipPlayable, fadeIn));
            return this;
        }
        /// <summary>
        /// Set last playable duration
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public AnimationSequenceBuilder SetDuration(double duration)
        {
            mixerPointer.GetInput(1).SetDuration(duration);
            return this;
        }
        /// <summary>
        /// Build an animation sequence
        /// </summary>
        public SequenceTask Build()
        {
            if (IsBuilt())
            {
                Debug.LogWarning("Graph is already built, rebuild is not allowed");
                return sequence;
            }
            if (!IsValid())
            {
                Debug.LogWarning("Graph is already destroyed before build");
                return sequence;
            }
            return BuildInternal(SequenceTask.GetPooled(Dispose));
        }
        /// <summary>
        /// Append animation sequence after an existed sequence
        /// </summary>
        /// <param name="sequenceTask"></param>
        public void Build(SequenceTask sequenceTask)
        {
            if (IsBuilt())
            {
                Debug.LogWarning("Graph is already built, rebuild is not allowed");
                return;
            }
            if (!IsValid())
            {
                Debug.LogWarning("Graph is already destroyed before build");
                return;
            }
            BuildInternal(sequenceTask);
            sequenceTask.AppendCallBack(Dispose);
        }
        private SequenceTask BuildInternal(SequenceTask sequenceTask)
        {
            if (!playableGraph.IsPlaying())
            {
                playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                playableGraph.Play();
            }
            foreach (var task in taskBuffer)
                sequenceTask.Append(task);
            var right = (AnimationClipPlayable)mixerPointer.GetInput(1);
            sequenceTask.Append(WaitPlayableTask.GetPooled(right, right.GetAnimationClip().length - fadeOutTime));
            if (fadeOutTime > 0)
            {
                sequenceTask.Append(FadeOutPlayableTask.GetPooled(rootMixer, right, fadeOutTime));
            }
            sequence = sequenceTask;
            taskBuffer.Clear();
            sequence.Acquire();
            return sequence;
        }
        /// <summary>
        /// Build an animation sequence to force fade out the playable, useful when your clip is loop and you want to crossfade it
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public SequenceTask BuildFadeOut(Action callBack)
        {
            return SequenceTask.GetPooled(AbsolutelyFadeOutPlayableTask.GetPooled(rootMixer, fadeOutTime), callBack);
        }
        /// <summary>
        /// Set animation sequence fadeOut time, default is 0
        /// </summary>
        /// <param name="fadeOut"></param>
        /// <returns></returns>
        public AnimationSequenceBuilder SetFadeOut(float fadeOut)
        {
            if (IsBuilt()) return this;
            fadeOutTime = fadeOut;
            return this;
        }
        /// <summary>
        /// Whether animation sequence is already built
        /// </summary>
        /// <returns></returns>
        public bool IsBuilt()
        {
            return sequence != null;
        }
        /// <summary>
        /// Whether animation sequence is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return playableGraph.IsValid();
        }
        /// <summary>
        /// Dispose internal playable graph
        /// </summary> <summary>
        public void Dispose()
        {
            sequence?.Dispose();
            sequence = null;
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }
    }
}
