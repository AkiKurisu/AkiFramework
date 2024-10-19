using System;
using System.Collections.Generic;
using Kurisu.Framework.Tasks;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.Framework.Animations
{
    public partial class AnimationProxy
    {
        public delegate void AnimationProxyDelegate(AnimationProxy animationProxy);
        /// <summary>
        /// Builder for creating dynamic animation sequence.
        /// </summary>
        public struct AnimationSequenceBuilder : IDisposable
        {
            private List<TaskBase> taskBuffer;
            private SequenceTask sequence;
            private float blendOutTime;
            private bool isDisposed;
            private AnimationProxy proxy;
            internal AnimationSequenceBuilder(AnimationProxy proxy)
            {
                this.proxy = proxy;
                blendOutTime = 0f;
                sequence = null;
                isDisposed = false;
                taskBuffer = ListPool<TaskBase>.Get();
            }
            /// <summary>
            /// Append an animation clip
            /// </summary>
            /// <param name="animationClip">Clip to play</param>
            /// <param name="blendInDuration">FadeIn time</param>
            /// <returns></returns>
            public readonly AnimationSequenceBuilder Append(AnimationClip animationClip, float blendInDuration)
            {
                return Append(animationClip, animationClip.length, blendInDuration);
            }
            /// <summary>
            /// Append an animation clip
            /// </summary>
            /// <param name="animationClip">Clip to play</param>
            /// <param name="duration">Duration can be infinity as loop</param>
            /// <param name="blendInDuration">FadeIn time</param>
            /// <returns></returns>
            public readonly AnimationSequenceBuilder Append(AnimationClip animationClip, float duration, float blendInDuration)
            {
                if (!IsValid())
                {
                    Debug.LogWarning("Builder is invalid but try to access it");
                    return this;
                }
                taskBuffer.Add(LoadAnimationClipTask.GetPooled(proxy, animationClip, duration, blendInDuration));
                return this;
            }
            /// <summary>
            /// Append an animatior controller
            /// </summary>
            /// <param name="animationClip">Clip to play</param>
            /// <param name="duration">Duration can be infinity as loop</param>
            /// <param name="blendInDuration">FadeIn time</param>
            /// <returns></returns>
            public readonly AnimationSequenceBuilder Append(RuntimeAnimatorController animatorController, float duration, float blendInDuration)
            {
                if (!IsValid())
                {
                    Debug.LogWarning("Builder is invalid but try to access it");
                    return this;
                }
                taskBuffer.Add(LoadAnimatorTask.GetPooled(proxy, animatorController, duration, blendInDuration));
                return this;
            }
            /// <summary>
            /// Append a proxy call back after current last action in the sequence
            /// </summary>
            /// <param name="callBack"></param>
            /// <returns></returns>
            public readonly AnimationSequenceBuilder AppendCallBack(AnimationProxyDelegate callBack)
            {
                taskBuffer.Add(AnimationProxyCallBackTask.GetPooled(proxy, callBack));
                return this;
            }
            /// <summary>
            /// Set animation sequence blend out time, default is 0
            /// </summary>
            /// <param name="blendOutTime"></param>
            /// <returns></returns>
            public AnimationSequenceBuilder SetBlendOut(float blendOutTime)
            {
                if (!IsValid())
                {
                    Debug.LogWarning("Builder is invalid but try to access it");
                    return this;
                }
                this.blendOutTime = blendOutTime;
                return this;
            }
            /// <summary>
            /// Build an animation sequence
            /// </summary>
            public SequenceTask Build()
            {
                if (!IsValid())
                {
                    Debug.LogWarning("Builder is invalid, rebuild is not allowed");
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
                if (!IsValid())
                {
                    Debug.LogWarning("Builder is invalid, rebuild is not allowed");
                    return;
                }
                BuildInternal(sequenceTask);
                sequenceTask.AppendCallBack(Dispose);
            }
            private SequenceTask BuildInternal(SequenceTask sequenceTask)
            {
                foreach (var task in taskBuffer)
                {
                    sequenceTask.Append(task);
                }
                float time = blendOutTime;
                AnimationProxy animProxy = proxy;
                sequenceTask.AppendCallBack(() => animProxy.Stop(time));
                sequence = sequenceTask;
                taskBuffer.Clear();
                sequence.Acquire();
                return sequence;
            }
            /// <summary>
            /// Whether builder is valid
            /// </summary>
            /// <returns></returns>
            public readonly bool IsValid()
            {
                return sequence == null && !isDisposed;
            }
            /// <summary>
            /// Dispose internal playable graph
            /// </summary> <summary>
            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }
                isDisposed = true;
                proxy = null;
                sequence = null;
                ListPool<TaskBase>.Release(taskBuffer);
                taskBuffer = null;
            }
        }
        private class LoadAnimationClipTask : PooledTaskBase<LoadAnimationClipTask>
        {
            private AnimationProxy proxy;
            private AnimationClip animationClip;
            private float blendInTime;
            private float duration;
            private double startTimestamp;
            public static LoadAnimationClipTask GetPooled(AnimationProxy proxy, AnimationClip animationClip, float duration, float blendInTime)
            {
                var task = GetPooled();
                task.proxy = proxy;
                task.animationClip = animationClip;
                task.duration = duration;
                task.blendInTime = blendInTime;
                return task;
            }
            public override void Tick()
            {
                if (Time.timeSinceLevelLoadAsDouble - startTimestamp >= duration)
                {
                    CompleteTask();
                }
            }
            public override void Start()
            {
                base.Start();
                startTimestamp = Time.timeSinceLevelLoadAsDouble;
                proxy.LoadAnimationClip(animationClip, blendInTime);
            }
        }
        private class LoadAnimatorTask : PooledTaskBase<LoadAnimatorTask>
        {
            private AnimationProxy proxy;
            private RuntimeAnimatorController animatorController;
            private float blendInTime;
            private float duration;
            private double startTimestamp;
            public static LoadAnimatorTask GetPooled(AnimationProxy proxy, RuntimeAnimatorController animatorController, float duration, float blendInTime)
            {
                var task = GetPooled();
                task.proxy = proxy;
                task.animatorController = animatorController;
                task.duration = duration;
                task.blendInTime = blendInTime;
                return task;
            }
            public override void Tick()
            {
                if (Time.timeSinceLevelLoadAsDouble - startTimestamp >= duration)
                {
                    CompleteTask();
                }
            }
            public override void Start()
            {
                base.Start();
                startTimestamp = Time.timeSinceLevelLoadAsDouble;
                proxy.LoadAnimator(animatorController, blendInTime);
            }
            protected override void Reset()
            {
                base.Reset();
                proxy = null;
                animatorController = null;
            }
        }
        private class AnimationProxyCallBackTask : PooledTaskBase<AnimationProxyCallBackTask>
        {
            private AnimationProxy proxy;
            private AnimationProxyDelegate callBack;
            public static AnimationProxyCallBackTask GetPooled(AnimationProxy proxy, AnimationProxyDelegate callBack)
            {
                var task = GetPooled();
                task.callBack = callBack;
                task.proxy = proxy;
                return task;
            }
            public override void Tick()
            {
                callBack?.Invoke(proxy);
                CompleteTask();
            }
            protected override void Reset()
            {
                base.Reset();
                proxy = null;
                callBack = null;
            }
        }
        /// <summary>
        /// Create an <see cref="AnimationSequenceBuilder"/> from this proxy
        /// </summary>
        /// <returns></returns>
        public AnimationSequenceBuilder CreateSequenceBuilder()
        {
            return new AnimationSequenceBuilder(this);
        }
    }
}
