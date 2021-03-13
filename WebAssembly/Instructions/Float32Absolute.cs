using System;
using System.Linq;
using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Absolute value.
    /// </summary>
    public class Float32Absolute : ValueOneToOneCallInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32Absolute"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32Absolute;

        /// <summary>
        /// Creates a new  <see cref="Float32Absolute"/> instance.
        /// </summary>
        public Float32Absolute()
        {
        }

        private protected sealed override MethodInfo MethodInfo => method;

        private protected sealed override WebAssemblyValueType ValueType => WebAssemblyValueType.Float32;

        private static readonly RegeneratingWeakReference<MethodInfo> method = new(() =>
            typeof(Math).GetTypeInfo().DeclaredMethods.First(m =>
            {
                if (m.Name != nameof(Math.Abs))
                    return false;

                var parms = m.GetParameters();
                return parms.Length == 1 && parms[0].ParameterType == typeof(float);
            }));
    }
}