using System;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Interface for AkiFramework's light-weight react solution.
    /// Remove observer concept and standardized interface (IObserver), use delegate instead.
    /// </summary>
    public interface IObservable<T> where T : Delegate
    {
        IDisposable Subscribe(T observer);
    }
}