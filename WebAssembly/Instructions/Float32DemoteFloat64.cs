using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Demote a 64-bit float to a 32-bit float.
    /// </summary>
    public class Float32DemoteFloat64 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float32DemoteFloat64"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float32DemoteFloat64;

        /// <summary>
        /// Creates a new  <see cref="Float32DemoteFloat64"/> instance.
        /// </summary>
        public Float32DemoteFloat64()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(OpCode.Float32DemoteFloat64, WebAssemblyValueType.Float64);
            context.Stack.Push(WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_R4);
        }
    }
}