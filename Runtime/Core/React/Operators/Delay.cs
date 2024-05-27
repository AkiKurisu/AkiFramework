using System;
using System.Collections.Generic;
using Kurisu.Framework.Events;
using Kurisu.Framework.Schedulers;
namespace Kurisu.Framework.React
{
    internal class DelayObservable<T> : IObservable<T>
    {
        private readonly IObservable<T> source;
        private readonly TimeSpan dueTime;
        private readonly bool ignoreTimeScale;
        public DelayObservable(IObservable<T> source, TimeSpan dueTime, bool ignoreTimeScale)
        {
            this.source = source;
            this.dueTime = dueTime;
            this.ignoreTimeScale = ignoreTimeScale;
        }
        public IDisposable Subscribe(Action<T> observer)
        {
            return new Delay(this, observer).Run();
        }
        private class Delay
        {
            private readonly DelayObservable<T> parent;
            private readonly Action<T> observer;
            private readonly Queue<Timestamped<T>> queue = new();
            private SerialDisposable serialDisposable;
            private bool running = false;
            private static readonly bool isEvent;
            static Delay()
            {
                isEvent = typeof(T).IsSubclassOf(typeof(EventBase));
            }
            public Delay(DelayObservable<T> parent, Action<T> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }

            public IDisposable Run()
            {
                serialDisposable = new();
                return StableCompositeDisposable.Create(parent.source.Subscribe(OnNext), serialDisposable);
            }

            private void OnNext(T value)
            {
                var dueTime = Scheduler.Now.Add(parent.dueTime);
                if (isEvent) (value as EventBase).Acquire();
                queue.Enqueue(new Timestamped<T>(value, dueTime));
                if (!running)
                {
                    serialDisposable.Disposable = Scheduler.Delay(parent.dueTime, DrainQueue, parent.ignoreTimeScale);
                    running = true;
                }
            }

            private void DrainQueue(Action<TimeSpan> recurse)
            {
                var shouldYield = false;
                while (true)
                {
                    var hasValue = false;
                    var value = default(T);
                    var shouldRecurse = false;
                    var recurseDueTime = default(TimeSpan);
                    if (queue.Count > 0)
                    {
                        var nextDue = queue.Peek().Timestamp;

                        if (nextDue.CompareTo(Scheduler.Now) <= 0 && !shouldYield)
                        {
                            value = queue.Dequeue().Value;
                            hasValue = true;
                        }
                        else
                        {
                            shouldRecurse = true;
                            recurseDueTime = Scheduler.Normalize(nextDue.Subtract(Scheduler.Now));
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