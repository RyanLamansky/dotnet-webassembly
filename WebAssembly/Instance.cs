using System;
using System.Runtime.InteropServices;

namespace WebAssembly
{
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
            this.Exports = exports ?? throw new ArgumentNullException(nameof(exports));
        }

        /// <summary>
        /// Exported features of the assembly.
        /// </summary>
        public TExports Exports { get; }

        /// <summary>
        /// Calls <see cref="Dispose"/> to release unmanaged resources associated with this instance.
        /// </summary>
        ~Instance() => this.Dispose();

        /// <summary>
        /// Releases unmanaged resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            if (this.Exports is IDisposable disposable)
                disposable.Dispose();
        }
    }
}