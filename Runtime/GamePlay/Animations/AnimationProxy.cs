using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Animations
{
    /// <summary>
    /// Animation proxy that can cross fade multi <see cref="RuntimeAnimatorController"/>
    /// </summary>
    /// <remarks>
    /// Useful to override default animation in cutscene and dialogue.
    /// </remarks>
    public partial class AnimationProxy : IDisposable
    {
        /// <summary>
        /// Get bound <see cref="Animator"/>
        /// </summary>
        /// <value></value>
        public Animator Animator { get; }
        /// <summary>
        /// Cached <see cref="RuntimeAnimatorController"/> of <see cref="Animator"/>
        /// </summary>
        /// <value></value>
        public RuntimeAnimatorController SourceController { get; private set; }
        /// <summary>
        /// Cached input <see cref="RuntimeAnimatorController"/> of <see cref="LeafAnimatorPlayable"/>
        /// </summary>
        /// <value></value>
        public RuntimeAnimatorController CurrentAnimatorController { get; private set; }
        /// <summary>
        /// Cached input <see cref="AnimationClip"/> of <see cref="LeafAnimationClipPlayable"/>
        /// </summary>
        /// <value></value>
        public AnimationClip CurrentAnimationClip { get; private set; }
        /// <summary>
        /// Get playing <see cref="PlayableGraph"/>
        /// </summary>
        /// <value></value>
        protected PlayableGraph Graph { get; private set; }
        /// <summary>
        /// Get root montage node
        /// </summary>
        /// <value></value>
        protected AnimationMontageNode RootMontage { get; private set; }
        /// <summary>
        /// Get leaf montage node
        /// </summary>
        /// <value></value>
        protected AnimationMontageNode LeafMontage { get; private set; }
        /// <summary>
        /// Get leaf <see cref="Playable"/>
        /// </summary>
        /// <value></value>
        protected Playable LeafPlayable { get; private set; }
        /// <summary>
        /// Get leaf <see cref="AnimatorControllerPlayable"/> if type matched
        /// </summary>
        /// <value></value>
        protected AnimatorControllerPlayable LeafAnimatorPlayable
        {
            get
            {
                if (LeafPlayable.IsPlayableOfType<AnimatorControllerPlayable>())
                {
                    return (AnimatorControllerPlayable)LeafPlayable;
                }
                return AnimatorControllerPlayable.Null;
            }
        }
        /// <summary>
        /// Get leaf <see cref="AnimationClipPlayable"/> if type matched
        /// </summary>
        /// <value></value>
        protected AnimationClipPlayable LeafAnimationClipPlayable
        {
            get
            {
                if (LeafPlayable.IsPlayableOfType<AnimationClipPlayable>())
                {
                    return (AnimationClipPlayable)LeafPlayable;
                }
                return default;
            }
        }
        /// <summary>
        /// Is proxy blendout
        /// </summary>
        /// <value></value>
        protected bool IsBlendIn { get; private set; }
        /// <summary>
        /// Is proxy blendout
        /// </summary>
        /// <value></value>
        protected bool IsBlendOut { get; private set; }
        /// <summary>
        /// Is proxy playing
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get
            {
                return Graph.IsValid() && Graph.IsPlaying();
            }
        }
        /// <summary>
        /// Should proxy clear <see cref="RuntimeAnimatorController"/> of <see cref="Animator"/> when completely blend in 
        /// which can prevent animation artifacts. Set <see cref="RestoreAnimatorControllerOnStop"/> to true to automatically 
        /// restore it after stopping
        /// </summary>
        /// <value></value>
        public bool ClearAnimatorControllerOnStart { get; set; } = true;
        /// <summary>
        /// Should proxy restore <see cref="RuntimeAnimatorController"/> after stopping
        /// </summary>
        /// <value></value>
        public bool RestoreAnimatorControllerOnStop { get; set; } = true;
        public AnimationProxy(Animator animator)
        {
            Animator = animator;
        }
        /// <summary>
        /// Load animator to the graph
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        protected virtual void LoadAnimator_Implementation(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f)
        {
            if (IsPlaying && CurrentAnimatorController == animatorController) return;
            // If Graph is not created or already destroyed, create a new one and use play api
            if (!Graph.IsValid())
            {
                PlayAnimatorInternal(animatorController, blendInDuration);
                return;
            }
            BlendAnimatorInternal(animatorController, blendInDuration);
        }
        /// <summary>
        /// Load animator to the graph in play mode
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        protected void PlayAnimatorInternal(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f)
        {
            // Create new graph
            SourceController = Animator.runtimeAnimatorController;
            Graph = PlayableGraph.Create($"{Animator.gameObject.name}_Playable");
            var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", Animator);
            LeafPlayable = AnimatorControllerPlayable.Create(Graph, CurrentAnimatorController = animatorController);
            LeafMontage = RootMontage = AnimationMontageNode.CreateRootMontage(LeafPlayable);
            playableOutput.SetSourcePlayable(RootMontage.Montage);

            // Start play graph
            PlayInternal(blendInDuration);
        }
        /// <summary>
        /// Load animator to the graph in blend mode
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        protected void BlendAnimatorInternal(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f)
        {
            LeafPlayable = AnimatorControllerPlayable.Create(Graph, CurrentAnimatorController = animatorController);
            LeafMontage |= new AnimationPlayableNode(LeafPlayable);
            if (blendInDuration > 0)
            {
                LeafMontage.ScheduleBlendIn(blendInDuration, () => Shrink(LeafMontage));
            }
            else
            {
                LeafMontage.Blend(1);
                Shrink(LeafMontage);
            }
        }
        /// <summary>
        /// Load animation clip to the graph
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        protected virtual void LoadAnimationClip_Implementation(AnimationClip animationClip, float blendInDuration = 0.25f)
        {
            if (IsPlaying && CurrentAnimationClip == animationClip) return;
            // If Graph is not created or already destroyed, create a new one and use play api
            if (!Graph.IsValid())
            {
                PlayAnimationClipInternal(animationClip, blendInDuration);
                return;
            }
            BlendAnimationClipInternal(animationClip, blendInDuration);
        }
        /// <summary>
        /// Load animation clip to the graph in play mode
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        protected void PlayAnimationClipInternal(AnimationClip animationClip, float blendInDuration = 0.25f)
        {
            // Create new graph
            SourceController = Animator.runtimeAnimatorController;
            Graph = PlayableGraph.Create($"{Animator.gameObject.name}_Playable");
            var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", Animator);
            LeafPlayable = AnimationClipPlayable.Create(Graph, CurrentAnimationClip = animationClip);
            LeafMontage = RootMontage = AnimationMontageNode.CreateRootMontage(LeafPlayable);
            playableOutput.SetSourcePlayable(RootMontage.Montage);

            // Start play graph
            PlayInternal(blendInDuration);
        }
        /// <summary>
        /// Load animation clip to the graph in blend mode
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        protected void BlendAnimationClipInternal(AnimationClip animationClip, float blendInDuration = 0.25f)
        {
            LeafPlayable = AnimationClipPlayable.Create(Graph, CurrentAnimationClip = animationClip);
            LeafMontage |= new AnimationPlayableNode(LeafPlayable);
            if (blendInDuration > 0)
            {
                LeafMontage.ScheduleBlendIn(blendInDuration, () => Shrink(LeafMontage));
            }
            else
            {
                LeafMontage.Blend(1);
                Shrink(LeafMontage);
            }
        }
        /// <summary>
        /// Start play graph and montage
        /// </summary>
        /// <param name="blendInDuration"></param>
        protected void PlayInternal(float blendInDuration)
        {
            IsBlendIn = true;
            if (blendInDuration > 0)
            {
                RootMontage.ScheduleBlendIn(blendInDuration, SetInGraph);
            }
            else
            {
                RootMontage.Blend(1);
                SetInGraph();
            }
            if (!IsPlaying) Graph.Play();
        }
        /// <summary>
        /// Call this function to release not used playables after montage completely blend in
        /// </summary>
        protected virtual void Shrink(AnimationMontageNode node)
        {
            if (LeafMontage != node) return; /* Has new montage in blend */
            if (node.BlendWeight != 1)
            {
                Debug.LogWarning("[AnimationProxy] Montage is in use but try to release it.");
                return;
            }
            LeafMontage = node.Shrink();
        }
        /// <summary>
        /// Call this function after animation proxy completly blend in
        /// </summary>
        protected virtual void SetInGraph()
        {
            IsBlendIn = false;
            if (ClearAnimatorControllerOnStart)
            {
                Animator.runtimeAnimatorController = null;
            }
        }
        /// <summary>
        /// Call this function after animation proxy completly blend out
        /// </summary>
        protected virtual void SetOutGraph()
        {
            IsBlendOut = false;
            Graph.Stop();
            Graph.Destroy();
            CurrentAnimatorController = null;
        }
        #region Public API
        /// <summary>
        /// Start playing animation from new <see cref="RuntimeAnimatorController"/> 
        /// and blend in if <see cref="blendInDuration"/> greater than 0
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        public void LoadAnimator(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f)
        {
            // Ensure old graph is destroyed
            if (IsBlendOut)
            {
                RootMontage.BlendHandle.Cancel();
                SetOutGraph();
            }
            LoadAnimator_Implementation(animatorController, blendInDuration);
        }
        /// <summary>
        /// Start playing animation from new <see cref="AnimationClip"/> 
        /// and blend in if <see cref="blendInDuration"/> greater than 0
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        public void LoadAnimationClip(AnimationClip animationClip, float blendInDuration = 0.25f)
        {
            // Ensure old graph is destroyed
            if (IsBlendOut)
            {
                RootMontage.BlendHandle.Cancel();
                SetOutGraph();
            }
            LoadAnimationClip_Implementation(animationClip, blendInDuration);
        }
        /// <summary>
        /// Stop animation proxy montage and blend out if <see cref="blendOutDuration"/> greater than 0
        /// </summary>
        /// <param name="blendOutDuration"></param>
        public void Stop(float blendOutDuration = 0.25f)
        {
            if (!IsPlaying) return;
            RootMontage.Dispose();
            if (RestoreAnimatorControllerOnStop)
            {
                Animator.runtimeAnimatorController = SourceController;
            }
            IsBlendOut = true;
            if (blendOutDuration <= 0)
            {
                RootMontage.Blend(0);
                SetOutGraph();
                return;
            }
            RootMontage.ScheduleBlendOut(blendOutDuration, SetOutGraph);
        }
        /// <summary>
        /// Release animation proxy
        /// </summary>
        public virtual void Dispose()
        {
            CurrentAnimatorController = null;
            SourceController = null;
            if (Graph.IsValid())
                Graph.Destroy();
        }
        /// <summary>
        /// Check if <see cref="LeafAnimatorPlayable"/> use this <see cref="RuntimeAnimatorController"/> 
        /// </summary>
        /// <param name="runtimeAnimatorController"></param>
        /// <returns></returns>
        public bool IsCurrent(RuntimeAnimatorController runtimeAnimatorController)
        {
            return CurrentAnimatorController == runtimeAnimatorController;
        }
        #endregion Public API
    }
}
