using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Return the current memory size in units of 65536-byte pages.
    /// </summary>
    public class MemorySize : Instruction
    {
        /// <summary>
        /// Always <see cref="OpCode.MemorySize"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.MemorySize;

        /// <summary>
        /// Not currently used.
        /// </summary>
        public byte Reserved { get; set; }

        /// <summary>
        /// Creates a new  <see cref="MemorySize"/> instance.
        /// </summary>
        public MemorySize()
        {
        }

        internal MemorySize(Reader reader)
        {
            Reserved = reader.ReadVarUInt1();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.MemorySize);
            writer.Write(this.Reserved);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction other) =>
            other is MemorySize instruction
            && instruction.Reserved == this.Reserved
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Reserved);

        internal sealed override void Compile(CompilationContext context)
        {
            if (context.Memory == null)
                throw new CompilerException("Cannot use instructions that depend on linear memory when linear memory is not defined.");

            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, context.Memory);
            context.Emit(OpCodes.Call, Runtime.UnmanagedMemory.SizeGetter);
            context.Emit(OpCodes.Ldc_I4, Memory.PageSize);
            context.Emit(OpCodes.Div_Un);

            context.Stack.Push(WebAssemblyValueType.Int32);
        }
    }
}