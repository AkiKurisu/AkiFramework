using System;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Class to manage <see cref="IDisposable"/> unregister
    /// </summary>
    public interface IUnRegister
    {
        void Add(IDisposable disposable);
        void Remove(IDisposable disposable);
    }
    public interface ICancelable : IDisposable
    {
        bool IsDisposed { get; }
    }
}