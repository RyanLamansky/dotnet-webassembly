using System;
using System.Collections.Generic;
using WebAssembly.Instructions;
using WebAssembly.Runtime.Compilation;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
    /// <summary>
    /// A combination of <see cref="OpCode"/> and its associated parameters.
    /// </summary>
    public abstract class Instruction : IEquatable<Instruction>
    {
        /// <summary>
        /// Creates a new <see cref="Instruction"/> instance.
        /// </summary>
        private protected Instruction()
        {
        }

        /// <summary>
        /// Gets the <see cref="OpCode"/> associated with this instruction.
        /// </summary>
        public abstract OpCode OpCode { get; }

        internal abstract void WriteTo(Writer writer);

        internal abstract void Compile(CompilationContext context);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="other">The instruction to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public abstract bool Equals(Instruction? other);

        /// <summary>
        /// Determines whether this instruction is identical to another.
        /// </summary>
        /// <param name="obj">The object instance to compare against.</param>
        /// <returns>True if they have the same type and value, otherwise false.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as Instruction);

        /// <summary>
        /// Returns a simple hash code based on the value of the instruction.
        /// </summary>
        /// <returns>The hash code.</returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// Provides a native representation of the instruction; the base implementation returns the opcode in WASM spec format.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString() => this.OpCode.ToNativeName();

        /// <summary>
        /// Parses an instruction stream restricted to the opcodes available for an initializer expression.
        /// </summary>
        /// <param name="reader">The source of binary data.</param>
        /// <returns>Parsed instructions.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
        internal static IEnumerable<Instruction> ParseInitializerExpression(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            //As of the initial version, the set of operations valid for initializer experssions is extremely limited.
            while (true)
            {
                var initialOffset = reader.Offset;
                var opCode = (OpCode)reader.ReadByte();
                switch (opCode)
                {
                    default: throw new ModuleLoadException($"Opcode \"{opCode}\" is not permitted in intializer expressions.", initialOffset);
                    case OpCode.GlobalGet: yield return new GlobalGet(reader); break;
                    case OpCode.Int32Constant: yield return new Int32Constant(reader); break;
                    case OpCode.Int64Constant: yield return new Int64Constant(reader); break;
                    case OpCode.Float32Constant: yield return new Float32Constant(reader); break;
                    case OpCode.Float64Constant: yield return new Float64Constant(reader); break;
                    case OpCode.End: yield return new End(); yield break;
                }
            }
        }

        /// <summary>
        /// Parses an instruction stream.
        /// </summary>
        /// <param name="reader">The source of binary data.</param>
        /// <returns>Parsed instructions.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> cannot be null.</exception>
        internal static IEnumerable<Instruction> Parse(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var depth = 1;
            while (true)
            {
                var initialOffset = reader.Offset;
                var opCode = (OpCode)reader.ReadByte();
                switch (opCode)
                {
                    default: throw new ModuleLoadException($"Don't know how to parse opcode \"{opCode}\".", initialOffset);
                    case OpCode.Unreachable: yield return new Unreachable(); break;
                    case OpCode.NoOperation: yield return new NoOperation(); break;

                    case OpCode.Block:
                        yield return new Block(reader);
                        depth++;
                        break;

                    case OpCode.Loop:
                        yield return new Loop(reader);
                        depth++;
                        break;

                    case OpCode.If:
                        yield return new If(reader);
                        depth++;
                        break;

                    case OpCode.Else: yield return new Else(); break;

                    case OpCode.End:
                        yield return new End();
                        Assert(depth > 0);
                        if (--depth == 0)
                            yield break;
                        break;

                    case OpCode.Branch: yield return new Branch(reader); break;
                    case OpCode.BranchIf: yield return new BranchIf(reader); break;
                    case OpCode.BranchTable: yield return new BranchTable(reader); break;
                    case OpCode.Return: yield return new Return(); break;
                    case OpCode.Call: yield return new Call(reader); break;
                    case OpCode.CallIndirect: yield return new CallIndirect(reader); break;
                    case OpCode.Drop: yield return new Drop(); break;
                    case OpCode.Select: yield return new Select(); break;
                    case OpCode.LocalGet: yield return new LocalGet(reader); break;
                    case OpCode.LocalSet: yield return new LocalSet(reader); break;
                    case OpCode.LocalTee: yield return new LocalTee(reader); break;
                    case OpCode.GlobalGet: yield return new GlobalGet(reader); break;
                    case OpCode.GlobalSet: yield return new GlobalSet(reader); break;
                    case OpCode.Int32Load: yield return new Int32Load(reader); break;
                    case OpCode.Int64Load: yield return new Int64Load(reader); break;
                    case OpCode.Float32Load: yield return new Float32Load(reader); break;
                    case OpCode.Float64Load: yield return new Float64Load(reader); break;
                    case OpCode.Int32Load8Signed: yield return new Int32Load8Signed(reader); break;
                    case OpCode.Int32Load8Unsigned: yield return new Int32Load8Unsigned(reader); break;
                    case OpCode.Int32Load16Signed: yield return new Int32Load16Signed(reader); break;
                    case OpCode.Int32Load16Unsigned: yield return new Int32Load16Unsigned(reader); break;
                    case OpCode.Int64Load8Signed: yield return new Int64Load8Signed(reader); break;
                    case OpCode.Int64Load8Unsigned: yield return new Int64Load8Unsigned(reader); break;
                    case OpCode.Int64Load16Signed: yield return new Int64Load16Signed(reader); break;
                    case OpCode.Int64Load16Unsigned: yield return new Int64Load16Unsigned(reader); break;
                    case OpCode.Int64Load32Signed: yield return new Int64Load32Signed(reader); break;
                    case OpCode.Int64Load32Unsigned: yield return new Int64Load32Unsigned(reader); break;
                    case OpCode.Int32Store: yield return new Int32Store(reader); break;
                    case OpCode.Int64Store: yield return new Int64Store(reader); break;
                    case OpCode.Float32Store: yield return new Float32Store(reader); break;
                    case OpCode.Float64Store: yield return new Float64Store(reader); break;
                    case OpCode.Int32Store8: yield return new Int32Store8(reader); break;
                    case OpCode.Int32Store16: yield return new Int32Store16(reader); break;
                    case OpCode.Int64Store8: yield return new Int64Store8(reader); break;
                    case OpCode.Int64Store16: yield return new Int64Store16(reader); break;
                    case OpCode.Int64Store32: yield return new Int64Store32(reader); break;
                    case OpCode.MemorySize: yield return new MemorySize(reader); break;
                    case OpCode.MemoryGrow: yield return new MemoryGrow(reader); break;
                    case OpCode.Int32Constant: yield return new Int32Constant(reader); break;
                    case OpCode.Int64Constant: yield return new Int64Constant(reader); break;
                    case OpCode.Float32Constant: yield return new Float32Constant(reader); break;
                    case OpCode.Float64Constant: yield return new Float64Constant(reader); break;
                    case OpCode.Int32EqualZero: yield return new Int32EqualZero(); break;
                    case OpCode.Int32Equal: yield return new Int32Equal(); break;
                    case OpCode.Int32NotEqual: yield return new Int32NotEqual(); break;
                    case OpCode.Int32LessThanSigned: yield return new Int32LessThanSigned(); break;
                    case OpCode.Int32LessThanUnsigned: yield return new Int32LessThanUnsigned(); break;
                    case OpCode.Int32GreaterThanSigned: yield return new Int32GreaterThanSigned(); break;
                    case OpCode.Int32GreaterThanUnsigned: yield return new Int32GreaterThanUnsigned(); break;
                    case OpCode.Int32LessThanOrEqualSigned: yield return new Int32LessThanOrEqualSigned(); break;
                    case OpCode.Int32LessThanOrEqualUnsigned: yield return new Int32LessThanOrEqualUnsigned(); break;
                    case OpCode.Int32GreaterThanOrEqualSigned: yield return new Int32GreaterThanOrEqualSigned(); break;
                    case OpCode.Int32GreaterThanOrEqualUnsigned: yield return new Int32GreaterThanOrEqualUnsigned(); break;
                    case OpCode.Int64EqualZero: yield return new Int64EqualZero(); break;
                    case OpCode.Int64Equal: yield return new Int64Equal(); break;
                    case OpCode.Int64NotEqual: yield return new Int64NotEqual(); break;
                    case OpCode.Int64LessThanSigned: yield return new Int64LessThanSigned(); break;
                    case OpCode.Int64LessThanUnsigned: yield return new Int64LessThanUnsigned(); break;
                    case OpCode.Int64GreaterThanSigned: yield return new Int64GreaterThanSigned(); break;
                    case OpCode.Int64GreaterThanUnsigned: yield return new Int64GreaterThanUnsigned(); break;
                    case OpCode.Int64LessThanOrEqualSigned: yield return new Int64LessThanOrEqualSigned(); break;
                    case OpCode.Int64LessThanOrEqualUnsigned: yield return new Int64LessThanOrEqualUnsigned(); break;
                    case OpCode.Int64GreaterThanOrEqualSigned: yield return new Int64GreaterThanOrEqualSigned(); break;
                    case OpCode.Int64GreaterThanOrEqualUnsigned: yield return new Int64GreaterThanOrEqualUnsigned(); break;
                    case OpCode.Float32Equal: yield return new Float32Equal(); break;
                    case OpCode.Float32NotEqual: yield return new Float32NotEqual(); break;
                    case OpCode.Float32LessThan: yield return new Float32LessThan(); break;
                    case OpCode.Float32GreaterThan: yield return new Float32GreaterThan(); break;
                    case OpCode.Float32LessThanOrEqual: yield return new Float32LessThanOrEqual(); break;
                    case OpCode.Float32GreaterThanOrEqual: yield return new Float32GreaterThanOrEqual(); break;
                    case OpCode.Float64Equal: yield return new Float64Equal(); break;
                    case OpCode.Float64NotEqual: yield return new Float64NotEqual(); break;
                    case OpCode.Float64LessThan: yield return new Float64LessThan(); break;
                    case OpCode.Float64GreaterThan: yield return new Float64GreaterThan(); break;
                    case OpCode.Float64LessThanOrEqual: yield return new Float64LessThanOrEqual(); break;
                    case OpCode.Float64GreaterThanOrEqual: yield return new Float64GreaterThanOrEqual(); break;
                    case OpCode.Int32CountLeadingZeroes: yield return new Int32CountLeadingZeroes(); break;
                    case OpCode.Int32CountTrailingZeroes: yield return new Int32CountTrailingZeroes(); break;
                    case OpCode.Int32CountOneBits: yield return new Int32CountOneBits(); break;
                    case OpCode.Int32Add: yield return new Int32Add(); break;
                    case OpCode.Int32Subtract: yield return new Int32Subtract(); break;
                    case OpCode.Int32Multiply: yield return new Int32Multiply(); break;
                    case OpCode.Int32DivideSigned: yield return new Int32DivideSigned(); break;
                    case OpCode.Int32DivideUnsigned: yield return new Int32DivideUnsigned(); break;
                    case OpCode.Int32RemainderSigned: yield return new Int32RemainderSigned(); break;
                    case OpCode.Int32RemainderUnsigned: yield return new Int32RemainderUnsigned(); break;
                    case OpCode.Int32And: yield return new Int32And(); break;
                    case OpCode.Int32Or: yield return new Int32Or(); break;
                    case OpCode.Int32ExclusiveOr: yield return new Int32ExclusiveOr(); break;
                    case OpCode.Int32ShiftLeft: yield return new Int32ShiftLeft(); break;
                    case OpCode.Int32ShiftRightSigned: yield return new Int32ShiftRightSigned(); break;
                    case OpCode.Int32ShiftRightUnsigned: yield return new Int32ShiftRightUnsigned(); break;
                    case OpCode.Int32RotateLeft: yield return new Int32RotateLeft(); break;
                    case OpCode.Int32RotateRight: yield return new Int32RotateRight(); break;
                    case OpCode.Int64CountLeadingZeroes: yield return new Int64CountLeadingZeroes(); break;
                    case OpCode.Int64CountTrailingZeroes: yield return new Int64CountTrailingZeroes(); break;
                    case OpCode.Int64CountOneBits: yield return new Int64CountOneBits(); break;
                    case OpCode.Int64Add: yield return new Int64Add(); break;
                    case OpCode.Int64Subtract: yield return new Int64Subtract(); break;
                    case OpCode.Int64Multiply: yield return new Int64Multiply(); break;
                    case OpCode.Int64DivideSigned: yield return new Int64DivideSigned(); break;
                    case OpCode.Int64DivideUnsigned: yield return new Int64DivideUnsigned(); break;
                    case OpCode.Int64RemainderSigned: yield return new Int64RemainderSigned(); break;
                    case OpCode.Int64RemainderUnsigned: yield return new Int64RemainderUnsigned(); break;
                    case OpCode.Int64And: yield return new Int64And(); break;
                    case OpCode.Int64Or: yield return new Int64Or(); break;
                    case OpCode.Int64ExclusiveOr: yield return new Int64ExclusiveOr(); break;
                    case OpCode.Int64ShiftLeft: yield return new Int64ShiftLeft(); break;
                    case OpCode.Int64ShiftRightSigned: yield return new Int64ShiftRightSigned(); break;
                    case OpCode.Int64ShiftRightUnsigned: yield return new Int64ShiftRightUnsigned(); break;
                    case OpCode.Int64RotateLeft: yield return new Int64RotateLeft(); break;
                    case OpCode.Int64RotateRight: yield return new Int64RotateRight(); break;
                    case OpCode.Float32Absolute: yield return new Float32Absolute(); break;
                    case OpCode.Float32Negate: yield return new Float32Negate(); break;
                    case OpCode.Float32Ceiling: yield return new Float32Ceiling(); break;
                    case OpCode.Float32Floor: yield return new Float32Floor(); break;
                    case OpCode.Float32Truncate: yield return new Float32Truncate(); break;
                    case OpCode.Float32Nearest: yield return new Float32Nearest(); break;
                    case OpCode.Float32SquareRoot: yield return new Float32SquareRoot(); break;
                    case OpCode.Float32Add: yield return new Float32Add(); break;
                    case OpCode.Float32Subtract: yield return new Float32Subtract(); break;
                    case OpCode.Float32Multiply: yield return new Float32Multiply(); break;
                    case OpCode.Float32Divide: yield return new Float32Divide(); break;
                    case OpCode.Float32Minimum: yield return new Float32Minimum(); break;
                    case OpCode.Float32Maximum: yield return new Float32Maximum(); break;
                    case OpCode.Float32CopySign: yield return new Float32CopySign(); break;
                    case OpCode.Float64Absolute: yield return new Float64Absolute(); break;
                    case OpCode.Float64Negate: yield return new Float64Negate(); break;
                    case OpCode.Float64Ceiling: yield return new Float64Ceiling(); break;
                    case OpCode.Float64Floor: yield return new Float64Floor(); break;
                    case OpCode.Float64Truncate: yield return new Float64Truncate(); break;
                    case OpCode.Float64Nearest: yield return new Float64Nearest(); break;
                    case OpCode.Float64SquareRoot: yield return new Float64SquareRoot(); break;
                    case OpCode.Float64Add: yield return new Float64Add(); break;
                    case OpCode.Float64Subtract: yield return new Float64Subtract(); break;
                    case OpCode.Float64Multiply: yield return new Float64Multiply(); break;
                    case OpCode.Float64Divide: yield return new Float64Divide(); break;
                    case OpCode.Float64Minimum: yield return new Float64Minimum(); break;
                    case OpCode.Float64Maximum: yield return new Float64Maximum(); break;
                    case OpCode.Float64CopySign: yield return new Float64CopySign(); break;
                    case OpCode.Int32WrapInt64: yield return new Int32WrapInt64(); break;
                    case OpCode.Int32TruncateFloat32Signed: yield return new Int32TruncateFloat32Signed(); break;
                    case OpCode.Int32TruncateFloat32Unsigned: yield return new Int32TruncateFloat32Unsigned(); break;
                    case OpCode.Int32TruncateFloat64Signed: yield return new Int32TruncateFloat64Signed(); break;
                    case OpCode.Int32TruncateFloat64Unsigned: yield return new Int32TruncateFloat64Unsigned(); break;
                    case OpCode.Int64ExtendInt32Signed: yield return new Int64ExtendInt32Signed(); break;
                    case OpCode.Int64ExtendInt32Unsigned: yield return new Int64ExtendInt32Unsigned(); break;
                    case OpCode.Int64TruncateFloat32Signed: yield return new Int64TruncateFloat32Signed(); break;
                    case OpCode.Int64TruncateFloat32Unsigned: yield return new Int64TruncateFloat32Unsigned(); break;
                    case OpCode.Int64TruncateFloat64Signed: yield return new Int64TruncateFloat64Signed(); break;
                    case OpCode.Int64TruncateFloat64Unsigned: yield return new Int64TruncateFloat64Unsigned(); break;
                    case OpCode.Float32ConvertInt32Signed: yield return new Float32ConvertInt32Signed(); break;
                    case OpCode.Float32ConvertInt32Unsigned: yield return new Float32ConvertInt32Unsigned(); break;
                    case OpCode.Float32ConvertInt64Signed: yield return new Float32ConvertInt64Signed(); break;
                    case OpCode.Float32ConvertInt64Unsigned: yield return new Float32ConvertInt64Unsigned(); break;
                    case OpCode.Float32DemoteFloat64: yield return new Float32DemoteFloat64(); break;
                    case OpCode.Float64ConvertInt32Signed: yield return new Float64ConvertInt32Signed(); break;
                    case OpCode.Float64ConvertInt32Unsigned: yield return new Float64ConvertInt32Unsigned(); break;
                    case OpCode.Float64ConvertInt64Signed: yield return new Float64ConvertInt64Signed(); break;
                    case OpCode.Float64ConvertInt64Unsigned: yield return new Float64ConvertInt64Unsigned(); break;
                    case OpCode.Float64PromoteFloat32: yield return new Float64PromoteFloat32(); break;
                    case OpCode.Int32ReinterpretFloat32: yield return new Int32ReinterpretFloat32(); break;
                    case OpCode.Int64ReinterpretFloat64: yield return new Int64ReinterpretFloat64(); break;
                    case OpCode.Float32ReinterpretInt32: yield return new Float32ReinterpretInt32(); break;
                    case OpCode.Float64ReinterpretInt64: yield return new Float64ReinterpretInt64(); break;
                    case OpCode.Int32Extend8Signed: yield return new Int32Extend8Signed(); break;
                    case OpCode.Int32Extend16Signed: yield return new Int32Extend16Signed(); break;
                    case OpCode.Int64Extend8Signed: yield return new Int64Extend8Signed(); break;
                    case OpCode.Int64Extend16Signed: yield return new Int64Extend16Signed(); break;
                    case OpCode.Int64Extend32Signed: yield return new Int64Extend32Signed(); break;

                    case OpCode.MiscellaneousOperationPrefix:
                        var miscellaneousOpCodeOffset = reader.Offset;
                        var miscellaneousOpCode = (MiscellaneousOpCode)reader.ReadByte();
                        switch (miscellaneousOpCode)
                        {
                            default: throw new ModuleLoadException($"Don't know how to parse miscellaneous opcode \"{miscellaneousOpCode}\".", miscellaneousOpCodeOffset);
                            case MiscellaneousOpCode.Int32TruncateSaturateFloat32Signed: yield return new Int32TruncateSaturateFloat32Signed(); break;
                            case MiscellaneousOpCode.Int32TruncateSaturateFloat32Unsigned: yield return new Int32TruncateSaturateFloat32Unsigned(); break;
                            case MiscellaneousOpCode.Int32TruncateSaturateFloat64Signed: yield return new Int32TruncateSaturateFloat64Signed(); break;
                            case MiscellaneousOpCode.Int32TruncateSaturateFloat64Unsigned: yield return new Int32TruncateSaturateFloat64Unsigned(); break;
                            case MiscellaneousOpCode.Int64TruncateSaturateFloat32Signed: yield return new Int64TruncateSaturateFloat32Signed(); break;
                            case MiscellaneousOpCode.Int64TruncateSaturateFloat32Unsigned: yield return new Int64TruncateSaturateFloat32Unsigned(); break;
                            case MiscellaneousOpCode.Int64TruncateSaturateFloat64Signed: yield return new Int64TruncateSaturateFloat64Signed(); break;
                            case MiscellaneousOpCode.Int64TruncateSaturateFloat64Unsigned: yield return new Int64TruncateSaturateFloat64Unsigned(); break;
                        }
                        break;
                }
            }
        }
    }
}