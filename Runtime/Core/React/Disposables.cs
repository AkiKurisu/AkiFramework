using System.Collections.Generic;
using System;
namespace Kurisu.Framework.React
{
    public static class Disposable
    {
        public static readonly IDisposable Empty = EmptyDisposable.Singleton;
        public static IDisposable Create(Action disposeAction)
        {
            return new AnonymousDisposable(disposeAction);
        }
    }
    public sealed class SingleAssignmentDisposable : IDisposable, ICancelable
    {
        IDisposable current;
        bool disposed;

        public bool IsDisposed { get { return disposed; } }


        public IDisposable Disposable
        {
            get
            {
                return current;
            }
            set
            {
                bool alreadyDisposed;
                alreadyDisposed = disposed;
                IDisposable old = current;
                if (!alreadyDisposed)
                {
                    if (value == null) return;
                    current = value;
                }
                if (alreadyDisposed && value != null)
                {
                    value.Dispose();
                    return;
                }
                if (old != null) throw new InvalidOperationException("Disposable is already set");
            }
        }
        public void Dispose()
        {
            IDisposable old = null;
            if (!disposed)
            {
                disposed = true;
                old = current;
                current = null;
            }
            old?.Dispose();
        }
    }
    public sealed class BooleanDisposable : IDisposable, ICancelable
    {
        public bool IsDisposed { get; private set; }

        public BooleanDisposable()
        {

        }

        internal BooleanDisposable(bool isDisposed)
        {
            IsDisposed = isDisposed;
        }

        public void Dispose()
        {
            if (!IsDisposed) IsDisposed = true;
        }
    }
    /// <summary>
    /// Represents a group of disposable resources that are disposed together.
    /// </summary>
    public abstract class StableCompositeDisposable : ICancelable
    {
        /// <summary>
        /// Creates a new group containing two disposable resources that are disposed together.
        /// </summary>
        /// <param name="disposable1">The first disposable resoruce to add to the group.</param>
        /// <param name="disposable2">The second disposable resoruce to add to the group.</param>
        /// <returns>Group of disposable resources that are disposed together.</returns>
        public static ICancelable Create(IDisposable disposable1, IDisposable disposable2)
        {
            if (disposable1 == null) throw new ArgumentNullException("disposable1");
            if (disposable2 == null) throw new ArgumentNullException("disposable2");

            return new Binary(disposable1, disposable2);
        }

        /// <summary>
        /// Creates a new group containing three disposable resources that are disposed together.
        /// </summary>
        /// <param name="disposable1">The first disposable resoruce to add to the group.</param>
        /// <param name="disposable2">The second disposable resoruce to add to the group.</param>
        /// <param name="disposable3">The third disposable resoruce to add to the group.</param>
        /// <returns>Group of disposable resources that are disposed together.</returns>
        public static ICancelable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
        {
            if (disposable1 == null) throw new ArgumentNullException("disposable1");
            if (disposable2 == null) throw new ArgumentNullException("disposable2");
            if (disposable3 == null) throw new ArgumentNullException("disposable3");

            return new Trinary(disposable1, disposable2, disposable3);
        }

        /// <summary>
        /// Creates a new group containing four disposable resources that are disposed together.
        /// </summary>
        /// <param name="disposable1">The first disposable resoruce to add to the group.</param>
        /// <param name="disposable2">The second disposable resoruce to add to the group.</param>
        /// <param name="disposable3">The three disposable resoruce to add to the group.</param>
        /// <param name="disposable4">The four disposable resoruce to add to the group.</param>
        /// <returns>Group of disposable resources that are disposed together.</returns>
        public static ICancelable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
        {
            if (disposable1 == null) throw new ArgumentNullException("disposable1");
            if (disposable2 == null) throw new ArgumentNullException("disposable2");
            if (disposable3 == null) throw new ArgumentNullException("disposable3");
            if (disposable4 == null) throw new ArgumentNullException("disposable4");

            return new Quaternary(disposable1, disposable2, disposable3, disposable4);
        }

        /// <summary>
        /// Creates a new group of disposable resources that are disposed together.
        /// </summary>
        /// <param name="disposables">Disposable resources to add to the group.</param>
        /// <returns>Group of disposable resources that are disposed together.</returns>
        public static ICancelable Create(params IDisposable[] disposables)
        {
            if (disposables == null) throw new ArgumentNullException("disposables");

            return new CompositeDisposable(disposables);
        }

        /// <summary>
        /// Creates a new group of disposable resources that are disposed together.
        /// </summary>
        /// <param name="disposables">Disposable resources to add to the group.</param>
        /// <returns>Group of disposable resources that are disposed together.</returns>
        public static ICancelable Create(IEnumerable<IDisposable> disposables)
        {
            if (disposables == null) throw new ArgumentNullException("disposables");

            return new CompositeDisposable(disposables);
        }

        /// <summary>
        /// Disposes all disposables in the group.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public abstract bool IsDisposed
        {
            get;
        }

        class Binary : StableCompositeDisposable
        {
            int disposedCallCount = -1;
            private readonly IDisposable _disposable1;
            private readonly IDisposable _disposable2;

            public Binary(IDisposable disposable1, IDisposable disposable2)
            {
                _disposable1 = disposable1;
                _disposable2 = disposable2;
            }

            public override bool IsDisposed
            {
                get
                {
                    return disposedCallCount != -1;
                }
            }

            public override void Dispose()
            {
                if (++disposedCallCount == 0)
                {
                    _disposable1.Dispose();
                    _disposable2.Dispose();
                }
            }
        }

        class Trinary : StableCompositeDisposable
        {
            int disposedCallCount = -1;
            private readonly IDisposable _disposable1;
            private readonly IDisposable _disposable2;
            private readonly IDisposable _disposable3;

            public Trinary(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
            {
                _disposable1 = disposable1;
                _disposable2 = disposable2;
                _disposable3 = disposable3;
            }

            public override bool IsDisposed
            {
                get
                {
                    return disposedCallCount != -1;
                }
            }

            public override void Dispose()
            {
                if (++disposedCallCount == 0)
                {
                    _disposable1.Dispose();
                    _disposable2.Dispose();
                    _disposable3.Dispose();
                }
            }
        }

        class Quaternary : StableCompositeDisposable
        {
            int disposedCallCount = -1;
            private readonly IDisposable _disposable1;
            private readonly IDisposable _disposable2;
            private readonly IDisposable _disposable3;
            private readonly IDisposable _disposable4;

            public Quaternary(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
            {
                _disposable1 = disposable1;
                _disposable2 = disposable2;
                _disposable3 = disposable3;
                _disposable4 = disposable4;
            }

            public override bool IsDisposed
            {
                get
                {
                    return disposedCallCount != -1;
                }
            }

            public override void Dispose()
            {
                if (++disposedCallCount == 0)
                {
                    _disposable1.Dispose();
                    _disposable2.Dispose();
                    _disposable3.Dispose();
                    _disposable4.Dispose();
                }
            }
        }
    }
    /// <summary>
    /// Anonymous callBack implement of <see cref="IDisposable"/>, will invoke callback on dispose
    /// </summary>
    internal class AnonymousDisposable : IDisposable
    {
        private bool isDisposed;
        private readonly Action onDispose;
        public AnonymousDisposable(Action onDispose)
        {
            this.onDispose = onDispose;
        }
        public void Dispose()
        {
            if (!isDisposed)
            {
                onDispose();
                isDisposed = true;
            }
        }
    }
    /// <summary>
    /// Composite implement of <see cref="IDisposable"/> and <see cref="IUnRegister"/>, will dispose inner disposable children on dispose
    /// </summary>
    public class CompositeDisposable : StableCompositeDisposable, IUnRegister
    {
        int disposedCallCount = -1;
        private readonly List<IDisposable> disposables = new();
        public override bool IsDisposed
        {
            get
            {
                return disposedCallCount != -1;
            }
        }
        public CompositeDisposable() { }
        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            this.disposables.AddRange(disposables);
        }
        public void Add(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void Remove(IDisposable disposable)
        {
            disposables.Remove(disposable);
        }

        public override void Dispose()
        {
            if (++disposedCallCount == 0)
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
                disposables.Clear();
            }
        }
    }
    /// <summary>
    /// Represents a disposable whose underlying disposable can be swapped for another disposable which causes the previous underlying disposable to be disposed.
    /// </summary>
    public sealed class SerialDisposable : IDisposable
    {
        private IDisposable current;
        private bool disposed;
        public IDisposable Disposable
        {
            get
            {
                return current;
            }
            set
            {
                var old = default(IDisposable);
                if (!disposed)
                {
                    old = current;
                    current = value;
                }
                old?.Dispose();
                if (disposed && value != null)
                {
                    value.Dispose();
                }
            }
        }

        public void Dispose()
        {
            var old = default(IDisposable);
            if (!disposed)
            {
                disposed = true;
                old = current;
                current = null;
            }
            old?.Dispose();
        }
    }
}