using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kurisu.Framework.Animations
{
    /// <summary>
    /// Animation proxy that can cross fade multi <see cref="RuntimeAnimatorController"/> 
    /// and <see cref="AnimationClip"/> in multi layer. 
    /// </summary>
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
        /// Proxy default animation layer index
        /// </summary>
        public const int DefaultLayerIndex = 0;
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
        private AnimationMontageNode[] _leafMontages;
        private Playable[] _leafPlayables;
        public AnimationProxy(Animator animator)
        {
            Animator = animator;
        }
        private static string GetPlayableName(Animator animator)
        {
            return $"{animator.name}_AnimationProxyPlayable";
        }
        /// <summary>
        /// Get ref leaf montage node
        /// </summary>
        /// <param name="layerHandle"></param> 
        /// <value></value>
        protected ref AnimationMontageNode GetLeafMontageRef(LayerHandle layerHandle = default)
        {
            return ref _leafMontages[GetLayerIndex(layerHandle)];
        }
        /// <summary>
        /// Get ref leaf <see cref="Playable"/>
        /// </summary>
        /// <param name="layerHandle"></param> 
        /// <value></value>
        protected ref Playable GetLeafPlayableRef(LayerHandle layerHandle = default)
        {
            return ref _leafPlayables[GetLayerIndex(layerHandle)];
        }
        /// <summary>
        /// Create proxy root montage
        /// </summary>
        /// <param name="sourcePlayableNode"></param>
        /// <param name="context"></param>
        /// <param name="contexts"></param>
        private void CreateRootMontage(AnimationPlayableNode sourcePlayableNode, LayerContext context, LayerContext[] contexts)
        {
            int leafCount = Math.Max(1, contexts.Length);
            _leafMontages = new AnimationMontageNode[leafCount];
            _leafPlayables = new Playable[leafCount];
            RootMontage = AnimationMontageNode.CreateChildOnlyMontage(sourcePlayableNode);
            if (context.Handle.IsValid() && contexts.Length > 0)
            {
                var index = context.Descriptor.Index;
                var layerMontage = AnimationMontageNode.CreateLayerMontage(RootMontage, context, contexts);
                GetLeafPlayableRef(context.Handle) = sourcePlayableNode.Playable;
                GetLeafMontageRef(context.Handle) = layerMontage.Children[index];
                RootMontage = layerMontage;
            }
            else
            {
                GetLeafPlayableRef(default) = sourcePlayableNode.Playable;
                GetLeafMontageRef(default) = RootMontage;
            }
        }
        /// <summary>
        /// Load animator to the graph
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected virtual void LoadAnimator_Implementation(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // If Graph is not created or already destroyed, create a new one and use play api
            if (!Graph.IsValid())
            {
                PlayAnimatorInternal(animatorController, blendInDuration, layerHandle);
                return;
            }
            BlendAnimatorInternal(animatorController, blendInDuration);
        }
        /// <summary>
        /// Load animator to the graph in play mode
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected void PlayAnimatorInternal(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // Create new graph
            SourceController = Animator.runtimeAnimatorController;
            Graph = PlayableGraph.Create(GetPlayableName(Animator));
            var playableOutput = AnimationPlayableOutput.Create(Graph, nameof(RuntimeAnimatorController), Animator);
            var node = new AnimationPlayableNode(AnimatorControllerPlayable.Create(Graph, animatorController), animatorController);
            CreateRootMontage(node, GetLayerContext(layerHandle), GetAllLayerContexts());
            playableOutput.SetSourcePlayable(RootMontage.Playable);

            // Start play graph
            PlayInternal(blendInDuration);
        }
        /// <summary>
        /// Load animator to the graph in blend mode
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected void BlendAnimatorInternal(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            GetLeafPlayableRef(layerHandle) = AnimatorControllerPlayable.Create(Graph, animatorController);
            var node = new AnimationPlayableNode(GetLeafPlayableRef(layerHandle), animatorController);
            GetLeafMontageRef(layerHandle) |= node;
            var leafMontage = GetLeafMontage(layerHandle);
            if (blendInDuration > 0)
            {
                leafMontage.ScheduleBlendIn(blendInDuration, () => Shrink(leafMontage, layerHandle));
            }
            else
            {
                leafMontage.Blend(1);
                Shrink(leafMontage, layerHandle);
            }
        }
        /// <summary>
        /// Load animation clip to the graph
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected virtual void LoadAnimationClip_Implementation(AnimationClip animationClip, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // If Graph is not created or already destroyed, create a new one and use play api
            if (!Graph.IsValid())
            {
                PlayAnimationClipInternal(animationClip, blendInDuration, layerHandle);
                return;
            }
            BlendAnimationClipInternal(animationClip, blendInDuration);
        }
        /// <summary>
        /// Load animation clip to the graph in play mode
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected void PlayAnimationClipInternal(AnimationClip animationClip, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // Create new graph
            SourceController = Animator.runtimeAnimatorController;
            Graph = PlayableGraph.Create(GetPlayableName(Animator));
            var playableOutput = AnimationPlayableOutput.Create(Graph, nameof(AnimationClip), Animator);
            var node = new AnimationPlayableNode(AnimationClipPlayable.Create(Graph, animationClip));
            CreateRootMontage(node, GetLayerContext(layerHandle), GetAllLayerContexts());
            playableOutput.SetSourcePlayable(RootMontage.Playable);

            // Start play graph
            PlayInternal(blendInDuration);
        }
        /// <summary>
        /// Load animation clip to the graph in blend mode
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param> 
        protected void BlendAnimationClipInternal(AnimationClip animationClip, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            GetLeafPlayableRef(layerHandle) = AnimationClipPlayable.Create(Graph, animationClip);
            GetLeafMontageRef(layerHandle) |= new AnimationPlayableNode(GetLeafPlayableRef(layerHandle));
            var leafMontage = GetLeafMontage(layerHandle);
            if (blendInDuration > 0)
            {
                leafMontage.ScheduleBlendIn(blendInDuration, () => Shrink(leafMontage, layerHandle));
            }
            else
            {
                leafMontage.Blend(1);
                Shrink(leafMontage, layerHandle);
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
        protected virtual void Shrink(AnimationMontageNode node, LayerHandle layerHandle)
        {
            if (GetLeafMontageRef(layerHandle) != node) return; /* Has new montage in blend */
            if (!node.CanShrink())
            {
                Debug.LogWarning("[AnimationProxy] Montage is in use but try to release it.");
                return;
            }
            GetLeafMontageRef(layerHandle) = node.Shrink();
        }
        /// <summary>
        /// Call this function after animation proxy completly blend in
        /// </summary>
        protected virtual void SetInGraph()
        {
            IsBlendIn = false;
            // Can not clear source animator controller when any layer montage use it
            if (ClearAnimatorControllerOnStart && IsFullBodyOverride())
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
            eventTrackerTickHandle.Cancel();
            Graph.Stop();
            Graph.Destroy();
        }
        #region Public API
        /// <summary>
        /// Whether proxy override full body animation
        /// </summary>
        /// <returns></returns>
        public bool IsFullBodyOverride()
        {
            /* Currently only root can be layer montage */
            return RootMontage is not AnimationLayerMontageNode;
        }
        /// <summary>
        /// Get leaf montage node
        /// </summary>
        /// <param name="layerHandle"></param> 
        /// <value></value>
        public AnimationMontageNode GetLeafMontage(LayerHandle layerHandle = default)
        {
            return _leafMontages[GetLayerIndex(layerHandle)];
        }
        /// <summary>
        /// Get leaf <see cref="Playable"/>
        /// </summary>
        /// <param name="layerHandle"></param> 
        /// <value></value>
        public Playable GetLeafPlayable(LayerHandle layerHandle = default)
        {
            return _leafPlayables[GetLayerIndex(layerHandle)];
        }
        /// <summary>
        /// Start playing animation from new <see cref="RuntimeAnimatorController"/> 
        /// and blend in if <see cref="blendInDuration"/> greater than 0
        /// </summary>
        /// <param name="animatorController"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param>
        public void LoadAnimator(RuntimeAnimatorController animatorController, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // Ensure old graph is destroyed
            if (IsBlendOut)
            {
                RootMontage.BlendHandle.Cancel();
                SetOutGraph();
            }
            LoadAnimator_Implementation(animatorController, blendInDuration, layerHandle);
        }
        /// <summary>
        /// Start playing animation from new <see cref="AnimationClip"/> 
        /// and blend in if <see cref="blendInDuration"/> greater than 0
        /// </summary>
        /// <param name="animationClip"></param>
        /// <param name="blendInDuration"></param>
        /// <param name="layerHandle"></param>
        public void LoadAnimationClip(AnimationClip animationClip, float blendInDuration = 0.25f, LayerHandle layerHandle = default)
        {
            // Ensure old graph is destroyed
            if (IsBlendOut)
            {
                RootMontage.BlendHandle.Cancel();
                SetOutGraph();
            }
            LoadAnimationClip_Implementation(animationClip, blendInDuration, layerHandle);
        }
        /// <summary>
        /// Stop animation proxy montage and blend out if <see cref="blendOutDuration"/> greater than 0
        /// </summary>
        /// <param name="blendOutDuration"></param>
        public void Stop(float blendOutDuration = 0.25f)
        {
            if (!IsPlaying) return;
            RootMontage.Dispose();
            if (RestoreAnimatorControllerOnStop && Animator.runtimeAnimatorController != SourceController)
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
            notifierContexts.Clear();
            eventTrackerTickHandle.Dispose();
            SourceController = null;
            if (Graph.IsValid())
                Graph.Destroy();
        }
        /// <summary>
        /// Get proxy leaf animator controller name or animation clip name
        /// </summary>
        /// <param name="layerHandle"></param>
        /// <returns></returns>
        public string GetLeafAnimationName(LayerHandle layerHandle = default)
        {
            var playable = GetLeafPlayable(layerHandle);
            if (!playable.IsValid()) return string.Empty;

            if (playable.IsPlayableOfType<AnimatorControllerPlayable>())
            {
                return GetAnimatorControllerInstanceProxy(layerHandle).GetAnimatorController().name;
            }
            else
            {
                var proxy = GetAnimationClipInstanceProxy(layerHandle);
                return proxy.GetAnimationClip().name;
            }
        }
        /// <summary>
        /// Get proxy leaf animator controller current state or animation clip normalized time
        /// </summary>
        /// <param name="layerHandle">Proxy layer</param>
        /// <param name="innerLayerIndex">Animator layer if leaf montage use animator controller</param>
        /// <returns></returns>
        public float GetLeafAnimationNormalizedTime(LayerHandle layerHandle = default, int innerLayerIndex = DefaultLayerIndex)
        {
            var playable = GetLeafPlayable(layerHandle);
            if (innerLayerIndex < DefaultLayerIndex) innerLayerIndex = DefaultLayerIndex;
            float normalizedTime;
            if (playable.IsPlayableOfType<AnimatorControllerPlayable>())
            {
                /* AnimatorControllerPlayable can get normalized time directly */
                var proxy = GetAnimatorControllerInstanceProxy(layerHandle);
                normalizedTime = proxy.GetCurrentAnimatorStateInfo(innerLayerIndex).normalizedTime;
            }
            else
            {
                var proxy = GetAnimationClipInstanceProxy(layerHandle);
                var length = proxy.GetAnimationClip().length;
                normalizedTime = (float)(playable.GetTime() % length);
            }
            return normalizedTime;
        }
        /// <summary>
        /// Get proxy leaf animator controller current state or animation clip duration
        /// </summary>
        /// <param name="layerHandle">Proxy layer</param>
        /// <param name="innerLayerIndex">Animator layer if leaf montage use animator controller</param>
        /// <returns></returns>
        public float GetLeafAnimationDuration(LayerHandle layerHandle = default, int innerLayerIndex = DefaultLayerIndex)
        {
            var playable = GetLeafPlayable(layerHandle);
            if (innerLayerIndex < DefaultLayerIndex) innerLayerIndex = DefaultLayerIndex;
            float duration;
            if (playable.IsPlayableOfType<AnimatorControllerPlayable>())
            {
                /* AnimatorControllerPlayable can get normalized time directly */
                var proxy = GetAnimatorControllerInstanceProxy(layerHandle);
                duration = proxy.GetCurrentAnimatorStateInfo(innerLayerIndex).length;
            }
            else
            {
                var proxy = GetAnimationClipInstanceProxy(layerHandle);
                duration = proxy.GetAnimationClip().length;
            }
            return duration;
        }
        /// <summary>
        /// Get animator controller instance proxy if leaf montage use <see cref="RuntimeAnimatorController"/> 
        /// </summary>
        /// <param name="layerHandle"></param>
        /// <returns></returns>
        public AnimatorControllerInstanceProxy GetAnimatorControllerInstanceProxy(LayerHandle layerHandle = default)
        {
            AnimatorControllerPlayable playable = AnimatorControllerPlayable.Null;
            RuntimeAnimatorController runtimeAnimatorController = null;
            if (GetLeafPlayable(layerHandle).IsPlayableOfType<AnimatorControllerPlayable>())
            {
                playable = (AnimatorControllerPlayable)GetLeafPlayable(layerHandle);
                runtimeAnimatorController = GetLeafMontage(layerHandle).AnimatorController;
            }
            return new AnimatorControllerInstanceProxy(playable, runtimeAnimatorController);
        }
        /// <summary>
        /// Get animation clip instance proxy if leaf montage use <see cref="AnimationClip"/> 
        /// </summary>
        /// <param name="layerHandle"></param>
        /// <returns></returns>
        public AnimationClipInstanceProxy GetAnimationClipInstanceProxy(LayerHandle layerHandle = default)
        {
            AnimationClipPlayable playable = default;
            if (GetLeafPlayable(layerHandle).IsPlayableOfType<AnimationClipPlayable>())
            {
                playable = (AnimationClipPlayable)GetLeafPlayable(layerHandle);
            }
            return new AnimationClipInstanceProxy(playable);
        }
        #endregion Public API
    }
}
