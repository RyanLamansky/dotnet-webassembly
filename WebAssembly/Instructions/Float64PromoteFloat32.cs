using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Promote a 32-bit float to a 64-bit float.
    /// </summary>
    public class Float64PromoteFloat32 : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Float64PromoteFloat32"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Float64PromoteFloat32;

        /// <summary>
        /// Creates a new  <see cref="Float64PromoteFloat32"/> instance.
        /// </summary>
        public Float64PromoteFloat32()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(OpCode.Float64PromoteFloat32, WebAssemblyValueType.Float32);

            context.Emit(OpCodes.Conv_R8);

            stack.Push(WebAssemblyValueType.Float64);
        }
    }
}