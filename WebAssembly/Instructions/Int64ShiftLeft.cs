using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Sign-agnostic shift left.
    /// </summary>
    public class Int64ShiftLeft : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ShiftLeft"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ShiftLeft;

        /// <summary>
        /// Creates a new  <see cref="Int64ShiftLeft"/> instance.
        /// </summary>
        public Int64ShiftLeft()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);

            //Unlike WASM, CIL OpCodes.Shl requires the shift amount to be int32 or native int
            //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shl?view=net-5.0
            context.Emit(OpCodes.Conv_I);  //Convert shift amount into native int
            context.Emit(OpCodes.Shl);

            context.Stack.Push(WebAssemblyValueType.Int64);
        }
    }
}