using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Zero-replicating (logical) shift right.
    /// </summary>
    public class Int64ShiftRightSigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ShiftRightSigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ShiftRightSigned;

        /// <summary>
        /// Creates a new  <see cref="Int64ShiftRightSigned"/> instance.
        /// </summary>
        public Int64ShiftRightSigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);

            //Unlike WASM, CIL OpCodes.Shr requires the shift amount to be int32 or native int.
            //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr?view=net-5.0
            context.Emit(OpCodes.Conv_I);  //Convert shift amount into native int
            context.Emit(OpCodes.Shr);

            context.Stack.Push(WebAssemblyValueType.Int64);
        }
    }
}