using System;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Interface for AkiFramework's light-weight and simplified react solution.
    /// Remove observer concept and standardized interface (IObserver), use Action instead.
    /// Should works on unity's main thread only (remove lock).
    /// Thus error handle, completion, a lot of operators are not supported. 
    /// </summary>
    public interface IObservable<T>
    {
        IDisposable Subscribe(Action<T> observer);
    }
}