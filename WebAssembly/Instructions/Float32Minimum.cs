using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Minimum (binary operator); if either operand is NaN, returns NaN.
    /// </summary>
    public class Float32Minimum : ValueTwoToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32Minimum"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32Minimum;

        /// <summary>
        /// Creates a new  <see cref="Float32Minimum"/> instance.
        /// </summary>
        public Float32Minimum()
        {
        }

        private protected sealed override MethodInfo MethodInfo => method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;

        private static readonly RegeneratingWeakReference<MethodInfo> method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Min))
                    return false;

                var parms = m.GetParameters();
                return
                    parms.Length == 2 &&
                    parms[0].ParameterType == typeof(float) &&
                    parms[1].ParameterType == typeof(float)
                    ;
            }));
    }
}