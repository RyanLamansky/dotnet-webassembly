using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Round to nearest integer towards zero.
    /// </summary>
    public class Float64Truncate : ValueOneToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Truncate"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Truncate;

        /// <summary>
        /// Creates a new  <see cref="Float64Truncate"/> instance.
        /// </summary>
        public Float64Truncate()
        {
        }

        private protected sealed override MethodInfo MethodInfo => Method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

        internal static readonly RegeneratingWeakReference<MethodInfo> Method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Truncate))
                    return false;

                var parms = m.GetParameters();
                return parms.Length == 1 && parms[0].ParameterType == typeof(double);
            }));
    }
}