using System;
namespace Kurisu.Framework.React
{
    public static partial class Observable
    {
        public static IObservable<T> Delay<T>(this IObservable<T> source, TimeSpan dueTime, bool ignoreTimeScale = false)
        {
            return new DelayObservable<T>(source, dueTime, ignoreTimeScale);
        }
        public static IObservable<T> Delay<T>(this IObservable<T> source, float seconds, bool ignoreTimeScale = false)
        {
            return Delay(source, TimeSpan.FromSeconds(seconds), ignoreTimeScale);
        }
        public static IObservable<T> WaitFrame<T>(this IObservable<T> source, int frameCount)
        {
            return new WaitFrameObservable<T>(source, frameCount);
        }
        public static IObservable<T> Take<T>(this IObservable<T> source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0) return Empty<T>();

            // optimize .Take(count).Take(count)
            if (source is TakeObservable<T> take)
            {
                return take.Combine(count);
            }

            return new TakeObservable<T>(source, count);
        }
    }
}
