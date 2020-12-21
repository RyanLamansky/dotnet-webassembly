using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Provides shared functionality for instructions that read from linear memory.
    /// </summary>
    public abstract class MemoryReadInstruction : MemoryImmediateInstruction
    {
        private protected MemoryReadInstruction()
            : base()
        {
        }

        private protected MemoryReadInstruction(Reader reader)
            : base(reader)
        {
        }

        private protected virtual System.Reflection.Emit.OpCode ConversionOpCode => OpCodes.Nop;

        internal sealed override void Compile(CompilationContext context)
        {
            var stack = context.Stack;

            context.PopStackNoReturn(this.OpCode, WebAssemblyValueType.Int32);

            if (this.Offset != 0)
            {
                Int32Constant.Emit(context, (int)this.Offset);
                context.Emit(OpCodes.Add_Ovf_Un);
            }

            this.EmitRangeCheck(context);

            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, context.CheckedMemory);
            context.Emit(OpCodes.Call, UnmanagedMemory.StartGetter);
            context.Emit(OpCodes.Add);

            byte alignment;
            switch (this.Flags & Options.Align8)
            {
                default: //Impossible to hit, but needed to avoid compiler error the about alignment variable.
                case Options.Align1: alignment = 1; break;
                case Options.Align2: alignment = 2; break;
                case Options.Align4: alignment = 4; break;
                case Options.Align8: alignment = 8; break;
            }

            //8-byte alignment is not available in IL.
            //See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unaligned?view=net-5.0
            //However, because 8-byte alignment is subset of 4-byte alignment,
            //We don't have to consider it.
            if (alignment != 4 && alignment != 8)
                context.Emit(OpCodes.Unaligned, alignment);

            context.Emit(this.EmittedOpCode);
            var conversion = this.ConversionOpCode;
            if (conversion != OpCodes.Nop)
                context.Emit(conversion);

            stack.Push(this.Type);
        }
    }
}