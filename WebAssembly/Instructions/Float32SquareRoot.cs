using System.Reflection;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Square root.
    /// </summary>
    public class Float32SquareRoot : Float64CallWrapperInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32SquareRoot"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32SquareRoot;

        /// <summary>
        /// Creates a new  <see cref="Float32SquareRoot"/> instance.
        /// </summary>
        public Float32SquareRoot()
        {
        }

        private protected sealed override MethodInfo MethodInfo => Float64SquareRoot.Method;
    }
}