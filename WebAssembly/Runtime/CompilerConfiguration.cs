using System;
using System.Diagnostics;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Configures the WebAssembly compiler.
    /// </summary>
    public class CompilerConfiguration
    {
        /// <summary>
        /// Creates a new <see cref="CompilerConfiguration"/> instance with default properties.
        /// </summary>
        public CompilerConfiguration()
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] //Wrapped by a property
        private GetDelegateForTypeCallback getDelegateForType = GetStandardDelegateForType;

        /// <summary>
        /// A function that returns a generic delegate for the number of parameters and returns, used for imports.
        /// The default implementation is <see cref="GetStandardDelegateForType"/>.
        /// </summary>
        public GetDelegateForTypeCallback GetDelegateForType
        {
            get => getDelegateForType;
            set => getDelegateForType = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns the standard .NET delegate type, i.e. <see cref="Func{T, TResult}"/>/<see cref="Action"/> or their peers, for the provided parameter and return count.
        /// </summary>
        /// <param name="parameters">The number of parameters; if not 0 through 16 (inclusive), null is returned.</param>
        /// <param name="returns">The number of returns; if no 0 or 1, null is returned.</param>
        /// <returns>One of the <see cref="Func{T, TResult}"/>/<see cref="Action"/> variations,
        /// or null if no variation exists for the <paramref name="parameters"/>/<paramref name="returns"/> combination.</returns>
        /// <remarks>This can help build custom <see cref="GetDelegateForType"/> solutions by covering common cases.</remarks>
        public static System.Type GetStandardDelegateForType(int parameters, int returns)
        {
            switch (returns)
            {
                case 0:
                    switch (parameters)
                    {
                        case 00: return typeof(Action);
                        case 01: return typeof(Action<>);
                        case 02: return typeof(Action<,>);
                        case 03: return typeof(Action<,,>);
                        case 04: return typeof(Action<,,,>);
                        case 05: return typeof(Action<,,,,>);
                        case 06: return typeof(Action<,,,,,>);
                        case 07: return typeof(Action<,,,,,,>);
                        case 08: return typeof(Action<,,,,,,,>);
                        case 09: return typeof(Action<,,,,,,,,>);
                        case 10: return typeof(Action<,,,,,,,,,>);
                        case 11: return typeof(Action<,,,,,,,,,,>);
                        case 12: return typeof(Action<,,,,,,,,,,,>);
                        case 13: return typeof(Action<,,,,,,,,,,,,>);
                        case 14: return typeof(Action<,,,,,,,,,,,,,>);
                        case 15: return typeof(Action<,,,,,,,,,,,,,,>);
                        case 16: return typeof(Action<,,,,,,,,,,,,,,,>);
                    }
                    break;
                case 1:
                    switch (parameters)
                    {
                        case 00: return typeof(Func<>);
                        case 01: return typeof(Func<,>);
                        case 02: return typeof(Func<,,>);
                        case 03: return typeof(Func<,,,>);
                        case 04: return typeof(Func<,,,,>);
                        case 05: return typeof(Func<,,,,,>);
                        case 06: return typeof(Func<,,,,,,>);
                        case 07: return typeof(Func<,,,,,,,>);
                        case 08: return typeof(Func<,,,,,,,,>);
                        case 09: return typeof(Func<,,,,,,,,,>);
                        case 10: return typeof(Func<,,,,,,,,,,>);
                        case 11: return typeof(Func<,,,,,,,,,,,>);
                        case 12: return typeof(Func<,,,,,,,,,,,,>);
                        case 13: return typeof(Func<,,,,,,,,,,,,,>);
                        case 14: return typeof(Func<,,,,,,,,,,,,,,>);
                        case 15: return typeof(Func<,,,,,,,,,,,,,,,>);
                        case 16: return typeof(Func<,,,,,,,,,,,,,,,,>);
                    }
                    break;
            }

            return null;
        }
    }

    /// <summary>
    /// Provides a generic delegate type accepting the number of provided parameters and returns.
    /// </summary>
    /// <param name="parameters">The count of parameters.</param>
    /// <param name="returns">The count of returns.</param>
    /// <returns>
    /// A generic delegate or null if one is not available--this will lead to a <see cref="MissingDelegateTypesException"/> with the list of all misses.
    /// Typically, variants of <see cref="Func{T, TResult}"/>/<see cref="Action"/> are used, but these don't cover every possibility.
    /// If more than 16 parameters are needed, a custom delegate type must be created.</returns>
    /// <remarks><see cref="CompilerConfiguration.GetStandardDelegateForType(int, int)"/> can be combined with custom solutions to handle common cases.</remarks>
    public delegate System.Type GetDelegateForTypeCallback(int parameters, int returns);
}
