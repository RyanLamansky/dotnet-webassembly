using System.Reflection.Emit;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    ///Sign-replicating (arithmetic) shift right.
    /// </summary>
    public class Int64ShiftRightUnsigned : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Int64ShiftRightUnsigned"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Int64ShiftRightUnsigned;

        /// <summary>
        /// Creates a new  <see cref="Int64ShiftRightUnsigned"/> instance.
        /// </summary>
        public Int64ShiftRightUnsigned()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int64, WebAssemblyValueType.Int64);

            //Unlike WASM, CIL OpCodes.Shr_Un requires the shift amount to be int32 or native int.
            //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr_un?view=net-5.0
            context.Emit(OpCodes.Conv_I);  //Convert shift amount into native int
            context.Emit(OpCodes.Shr_Un);

            context.Stack.Push(WebAssemblyValueType.Int64);
        }
    }
}