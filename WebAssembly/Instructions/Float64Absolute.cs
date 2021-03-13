using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Absolute value.
    /// </summary>
    public class Float64Absolute : ValueOneToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Absolute"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Absolute;

        /// <summary>
        /// Creates a new  <see cref="Float64Absolute"/> instance.
        /// </summary>
        public Float64Absolute()
        {
        }

        private protected sealed override MethodInfo MethodInfo => method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

        private static readonly RegeneratingWeakReference<MethodInfo> method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Abs))
                    return false;

                var parms = m.GetParameters();
                return parms.Length == 1 && parms[0].ParameterType == typeof(double);
            }));
    }
}