using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Maximum (binary operator); if either operand is NaN, returns NaN.
    /// </summary>
    public class Float32Maximum : ValueTwoToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32Maximum"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32Maximum;

        /// <summary>
        /// Creates a new  <see cref="Float32Maximum"/> instance.
        /// </summary>
        public Float32Maximum()
        {
        }

        private protected sealed override MethodInfo MethodInfo => method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;

        private static readonly RegeneratingWeakReference<MethodInfo> method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Max))
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