using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Maximum (binary operator); if either operand is NaN, returns NaN.
    /// </summary>
    public class Float64Maximum : ValueTwoToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64Maximum"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64Maximum;

        /// <summary>
        /// Creates a new  <see cref="Float64Maximum"/> instance.
        /// </summary>
        public Float64Maximum()
        {
        }

        private protected sealed override MethodInfo MethodInfo => method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float64;

        private static readonly RegeneratingWeakReference<MethodInfo> method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Max))
                    return false;

                var parms = m.GetParameters();
                return
                    parms.Length == 2 &&
                    parms[0].ParameterType == typeof(double) &&
                    parms[1].ParameterType == typeof(double)
                    ;
            }));
    }
}