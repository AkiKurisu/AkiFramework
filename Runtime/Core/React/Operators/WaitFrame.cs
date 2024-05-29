using System;
using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.Schedulers;
using UnityEngine;
namespace Kurisu.Framework.React
{
    internal class WaitFrameObservable<T> : IObservable<T>
    {
        private readonly IObservable<T> source;
        private readonly int frameCount;
        public WaitFrameObservable(IObservable<T> source, int frameCount)
        {
            this.source = source;
            this.frameCount = frameCount;
        }

        public IDisposable Subscribe(Action<T> observer)
        {
            return new WaitFrame(this, observer).Run();
        }

        private class WaitFrame
        {
            private readonly WaitFrameObservable<T> parent;
            private readonly Queue<FrameInterval<T>> queue = new();
            private SerialDisposable serialDisposable;
            private bool running = false;
            private static readonly bool isEvent;
            private readonly Action<T> observer;
            static WaitFrame()
            {
                isEvent = typeof(T).IsSubclassOf(typeof(EventBase));
            }
            public WaitFrame(WaitFrameObservable<T> parent, Action<T> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }

            public IDisposable Run()
            {
                serialDisposable = new();
                return StableCompositeDisposable.Create(parent.source.Subscribe(OnNext), serialDisposable);
            }

            public void OnNext(T value)
            {
                var dueTime = Time.frameCount + parent.frameCount;
                if (isEvent) (value as EventBase).Acquire();
                queue.Enqueue(new FrameInterval<T>(value, dueTime));
                if (!running)
                {
                    serialDisposable.Disposable = Scheduler.WaitFrame(parent.frameCount, DrainQueue);
                    running = true;
                }
            }

            private void DrainQueue(Action<int> recurse)
            {
                var shouldYield = false;
                while (true)
                {
                    var hasValue = false;
                    var value = default(T);
                    var shouldRecurse = false;
                    var recurseDueTime = default(int);
                    if (queue.Count > 0)
                    {
                        var nextDue = queue.Peek().Interval;

                        if (nextDue.CompareTo(Time.frameCount) <= 0 && !shouldYield)
                        {
                            value = queue.Dequeue().Value;
                            hasValue = true;
                        }
                        else
                        {
                            shouldRecurse = true;
                            recurseDueTime = nextDue - Time.frameCount;
                            running = false;
                        }
                    }
                    else
                    {
                        running = false;
                    }
                    if (hasValue)
                    {
                        observer(value);
                        if (isEvent) (value as EventBase).Dispose();
                        shouldYield = true;
                    }
                    else
                    {
                        if (shouldRecurse)
                        {
                            recurse(recurseDueTime);
                        }
                        return;
                    }
                }
            }
        }
    }
}