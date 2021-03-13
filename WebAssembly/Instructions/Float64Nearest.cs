using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Round to nearest integer, ties to even.
    /// </summary>
    public class Float64Nearest : ValueOneToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Nearest"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Nearest;

        /// <summary>
        /// Creates a new  <see cref="Float64Nearest"/> instance.
        /// </summary>
        public Float64Nearest()
        {
        }

        private protected sealed override MethodInfo MethodInfo => Method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

        internal static readonly RegeneratingWeakReference<MethodInfo> Method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Round))
                    return false;

                var parms = m.GetParameters();
                return parms.Length == 1 && parms[0].ParameterType == typeof(double);
            }));
    }
}