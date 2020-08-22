using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WebAssembly.Runtime;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Common features of instructions that access linear memory.
    /// </summary>
    public abstract class MemoryImmediateInstruction : Instruction, IEquatable<MemoryImmediateInstruction>
    {
        /// <summary>
        /// Indicates options for the instruction.
        /// </summary>
        [Flags]
        public enum Options : uint
        {
            /// <summary>
            /// The access uses 8-bit alignment.
            /// </summary>
            Align1 = 0b00,

            /// <summary>
            /// The access uses 16-bit alignment.
            /// </summary>
            Align2 = 0b01,

            /// <summary>
            /// The access uses 32-bit alignment.
            /// </summary>
            Align4 = 0b10,

            /// <summary>
            /// The access uses 64-bit alignment.
            /// </summary>
            Align8 = 0b11,
        }

        /// <summary>
        /// A bitfield which currently contains the alignment in the least significant bits, encoded as log2(alignment).
        /// </summary>
        public Options Flags { get; set; }

        /// <summary>
        /// The index within linear memory for the access operation.
        /// </summary>
        public uint Offset { get; set; }

        private protected MemoryImmediateInstruction()
        {
        }

        private protected MemoryImmediateInstruction(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Flags = (Options)reader.ReadVarUInt32();
            Offset = reader.ReadVarUInt32();
        }

        internal sealed override void WriteTo(Writer writer)
        {
            writer.Write((byte)this.OpCode);
            writer.WriteVar((uint)this.Flags);
            writer.WriteVar(this.Offset);
        }

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(Instruction? other) => this.Equals(other as MemoryImmediateInstruction);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public bool Equals(MemoryImmediateInstruction? other) =>
            other != null
            && other.OpCode == this.OpCode
            && other.Flags == this.Flags
            && other.Offset == this.Offset
            ;

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Flags, (int)this.Offset);

        private protected abstract WebAssemblyValueType Type { get; }

        private protected abstract byte Size { get; }

        private protected abstract System.Reflection.Emit.OpCode EmittedOpCode { get; }

        private protected HelperMethod RangeCheckHelper
        {
            get
            {
                switch (this.Size)
                {
                    default: throw new InvalidOperationException(); // Shouldn't be possible.
                    case 1: return HelperMethod.RangeCheck8;
                    case 2: return HelperMethod.RangeCheck16;
                    case 4: return HelperMethod.RangeCheck32;
                    case 8: return HelperMethod.RangeCheck64;
                }
            }
        }

        private protected void EmitRangeCheck(CompilationContext context)
        {
            context.EmitLoadThis();
            context.Emit(OpCodes.Call, context[this.RangeCheckHelper, CreateRangeCheck]);
        }

        internal static MethodBuilder CreateRangeCheck(HelperMethod helper, CompilationContext context)
        {
            if (context.Memory == null)
                throw new CompilerException("Cannot use instructions that depend on linear memory when linear memory is not defined.");

            byte size;
            System.Reflection.Emit.OpCode opCode;
            switch (helper)
            {
                default: throw new InvalidOperationException(); // Shouldn't be possible.
                case HelperMethod.RangeCheck8:
                    size = 1;
                    opCode = OpCodes.Ldc_I4_1;
                    break;
                case HelperMethod.RangeCheck16:
                    size = 2;
                    opCode = OpCodes.Ldc_I4_2;
                    break;
                case HelperMethod.RangeCheck32:
                    size = 4;
                    opCode = OpCodes.Ldc_I4_4;
                    break;
                case HelperMethod.RangeCheck64:
                    size = 8;
                    opCode = OpCodes.Ldc_I4_8;
                    break;
            }

            var builder = context.CheckedExportsBuilder.DefineMethod(
                $"☣ Range Check {size}",
                CompilationContext.HelperMethodAttributes,
                typeof(uint),
                new[] { typeof(uint), context.CheckedExportsBuilder }
                );
            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldfld, context.Memory);
            il.Emit(OpCodes.Call, UnmanagedMemory.SizeGetter);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(opCode);
            il.Emit(OpCodes.Add_Ovf_Un);
            var outOfRange = il.DefineLabel();
            il.Emit(OpCodes.Blt_Un_S, outOfRange);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(outOfRange);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(opCode);
            il.Emit(OpCodes.Newobj, typeof(MemoryAccessOutOfRangeException)
                .GetTypeInfo()
                .DeclaredConstructors
                .First(c =>
                {
                    var parms = c.GetParameters();
                    return parms.Length == 2
                    && parms[0].ParameterType == typeof(uint)
                    && parms[1].ParameterType == typeof(uint)
                    ;
                }));
            il.Emit(OpCodes.Throw);
            return builder;
        }
    }
}