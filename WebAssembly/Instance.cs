using System;
using System.Diagnostics;

namespace WebAssembly;

/// <summary>
/// A compiled WebAssembly instance.
/// </summary>
public abstract class Instance<TExports> : IDisposable
    where TExports : class
{
    /// <summary>
    /// This is used by compiled WebAssembly files and should not be used by any other code.
    /// </summary>
    protected Instance(TExports exports)
    {
        this.exports = exports ?? throw new ArgumentNullException(nameof(exports));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
    private TExports? exports;

    /// <summary>
    /// Exported features of the assembly.
    /// </summary>
    public TExports Exports
    {
        get => this.exports ?? throw new ObjectDisposedException(nameof(Instance<TExports>));
    }

    /// <summary>
    /// Frees managed and unmanaged resources associated with this instance.
    /// </summary>
    /// <param name="disposing">When true, the caller is a <see cref="IDisposable.Dispose"/> method, if false, a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.exports == null)
            return;

        if (disposing)
        {
            if (this.Exports is IDisposable disposable)
                disposable.Dispose();
        }

        this.exports = null;
    }

    /// <summary>
    /// Releases unmanaged resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}
