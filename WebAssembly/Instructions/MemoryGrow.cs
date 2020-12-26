using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Grow linear memory by a given unsigned delta of 65536-byte pages. Return the previous memory size in units of pages or -1 on failure.
    /// </summary>
    public class MemoryGrow : Instruction
    {
        /// <summary>
        /// Always <see cref="OpCode.MemoryGrow"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.MemoryGrow;

        /// <summary>
        /// Not currently used.
        /// </summary>
        public byte Reserved { get; set; }

        /// <summary>
        /// Creates a new  <see cref="MemoryGrow"/> instance.
        /// </summary>
        public MemoryGrow()
        {
        }

        internal MemoryGrow(Reader reader)
        {
            Reserved = reader.ReadVarUInt1();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)OpCode.MemoryGrow);
            writer.Write(this.Reserved);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) =>
            other is MemoryGrow instruction
            && instruction.Reserved == this.Reserved
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, this.Reserved);

        internal sealed override void Compile(CompilationContext context)
        {
            //Assuming validation passes, the remaining type will be Int32.
            context.ValidateStack(OpCode.MemoryGrow, WebAssemblyValueType.Int32);

            context.EmitLoadThis();
            context.Emit(OpCodes.Ldfld, context.CheckedMemory);
            context.Emit(OpCodes.Call, context[HelperMethod.GrowMemory, (helper, c) =>
            {
                var builder = c.CheckedExportsBuilder.DefineMethod(
                    "☣ GrowMemory",
                    CompilationContext.HelperMethodAttributes,
                    typeof(uint),
                    new[]
                    {
                        typeof(uint), //Delta
						typeof(UnmanagedMemory),
                    }
                    );

                var il = builder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, UnmanagedMemory.GrowMethod);
                il.Emit(OpCodes.Ret);

                return builder;
            }
            ]);
        }
    }
}