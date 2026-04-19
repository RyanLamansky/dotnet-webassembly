using System;
using System.Collections.Generic;
using WebAssembly.Instructions;
using WebAssembly.Runtime.Compilation;
using static System.Diagnostics.Debug;

namespace WebAssembly;

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
#if NETSTANDARD
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));
#else
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
#endif

        //As of the initial version, the set of operations valid for initializer expressions is extremely limited.
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
                case OpCode.RefNull: yield return new Instructions.RefNull(reader); break;
                case OpCode.RefFunc: yield return new Instructions.RefFunc(reader); break;
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
#if NETSTANDARD
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));
#else
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
#endif

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
                case OpCode.SelectWithType: yield return new Instructions.SelectWithType(reader); break;
                case OpCode.LocalGet: yield return new LocalGet(reader); break;
                case OpCode.LocalSet: yield return new LocalSet(reader); break;
                case OpCode.LocalTee: yield return new LocalTee(reader); break;
                case OpCode.GlobalGet: yield return new GlobalGet(reader); break;
                case OpCode.GlobalSet: yield return new GlobalSet(reader); break;
                case OpCode.TableGet: yield return new Instructions.TableGet(reader); break;
                case OpCode.TableSet: yield return new Instructions.TableSet(reader); break;
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

                case OpCode.RefNull: yield return new Instructions.RefNull(reader); break;
                case OpCode.RefIsNull: yield return new Instructions.RefIsNull(); break;
                case OpCode.RefFunc: yield return new Instructions.RefFunc(reader); break;

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
                        case MiscellaneousOpCode.MemoryInit: yield return new Instructions.MemoryInit(reader); break;
                        case MiscellaneousOpCode.DataDrop: yield return new Instructions.DataDrop(reader); break;
                        case MiscellaneousOpCode.MemoryCopy: yield return new Instructions.MemoryCopy(reader); break;
                        case MiscellaneousOpCode.MemoryFill: yield return new Instructions.MemoryFill(reader); break;
                        case MiscellaneousOpCode.TableInit: yield return new Instructions.TableInit(reader); break;
                        case MiscellaneousOpCode.ElemDrop: yield return new Instructions.ElemDrop(reader); break;
                        case MiscellaneousOpCode.TableCopy: yield return new Instructions.TableCopy(reader); break;
                        case MiscellaneousOpCode.TableGrow: yield return new Instructions.TableGrow(reader); break;
                        case MiscellaneousOpCode.TableSize: yield return new Instructions.TableSize(reader); break;
                        case MiscellaneousOpCode.TableFill: yield return new Instructions.TableFill(reader); break;
                    }
                    break;

                case OpCode.SimdOperationPrefix:
                    var simdOpCodeOffset = reader.Offset;
                    var simdOpCode = (SimdOpCode)reader.ReadVarUInt32();
                    switch (simdOpCode)
                    {
                        default: throw new ModuleLoadException($"Don't know how to parse SIMD opcode \"{simdOpCode}\".", simdOpCodeOffset);
                        case SimdOpCode.V128Load: yield return new Instructions.V128Load(reader); break;
                        case SimdOpCode.V128Load8X8Signed: yield return new Instructions.V128Load8X8Signed(reader); break;
                        case SimdOpCode.V128Load8X8Unsigned: yield return new Instructions.V128Load8X8Unsigned(reader); break;
                        case SimdOpCode.V128Load16X4Signed: yield return new Instructions.V128Load16X4Signed(reader); break;
                        case SimdOpCode.V128Load16X4Unsigned: yield return new Instructions.V128Load16X4Unsigned(reader); break;
                        case SimdOpCode.V128Load32X2Signed: yield return new Instructions.V128Load32X2Signed(reader); break;
                        case SimdOpCode.V128Load32X2Unsigned: yield return new Instructions.V128Load32X2Unsigned(reader); break;
                        case SimdOpCode.V128Load8Splat: yield return new Instructions.V128Load8Splat(reader); break;
                        case SimdOpCode.V128Load16Splat: yield return new Instructions.V128Load16Splat(reader); break;
                        case SimdOpCode.V128Load32Splat: yield return new Instructions.V128Load32Splat(reader); break;
                        case SimdOpCode.V128Load64Splat: yield return new Instructions.V128Load64Splat(reader); break;
                        case SimdOpCode.V128Store: yield return new Instructions.V128Store(reader); break;
                        case SimdOpCode.V128Const: yield return new Instructions.V128Const(reader); break;
                        // shuffle / swizzle
                        case SimdOpCode.Int8x16Shuffle: yield return new Instructions.Int8x16Shuffle(reader); break;
                        case SimdOpCode.Int8x16Swizzle: yield return new Instructions.Int8x16Swizzle(); break;
                        // splats
                        case SimdOpCode.Int8x16Splat: yield return new Instructions.Int8x16Splat(); break;
                        case SimdOpCode.Int16x8Splat: yield return new Instructions.Int16x8Splat(); break;
                        case SimdOpCode.Int32x4Splat: yield return new Instructions.Int32x4Splat(); break;
                        case SimdOpCode.Int64x2Splat: yield return new Instructions.Int64x2Splat(); break;
                        case SimdOpCode.Float32x4Splat: yield return new Instructions.Float32x4Splat(); break;
                        case SimdOpCode.Float64x2Splat: yield return new Instructions.Float64x2Splat(); break;
                        // extract lane
                        case SimdOpCode.Int8x16ExtractLaneSigned: yield return new Instructions.Int8x16ExtractLaneSigned(reader); break;
                        case SimdOpCode.Int8x16ExtractLaneUnsigned: yield return new Instructions.Int8x16ExtractLaneUnsigned(reader); break;
                        case SimdOpCode.Int16x8ExtractLaneSigned: yield return new Instructions.Int16x8ExtractLaneSigned(reader); break;
                        case SimdOpCode.Int16x8ExtractLaneUnsigned: yield return new Instructions.Int16x8ExtractLaneUnsigned(reader); break;
                        case SimdOpCode.Int32x4ExtractLane: yield return new Instructions.Int32x4ExtractLane(reader); break;
                        case SimdOpCode.Int64x2ExtractLane: yield return new Instructions.Int64x2ExtractLane(reader); break;
                        case SimdOpCode.Float32x4ExtractLane: yield return new Instructions.Float32x4ExtractLane(reader); break;
                        case SimdOpCode.Float64x2ExtractLane: yield return new Instructions.Float64x2ExtractLane(reader); break;
                        // replace lane
                        case SimdOpCode.Int8x16ReplaceLane: yield return new Instructions.Int8x16ReplaceLane(reader); break;
                        case SimdOpCode.Int16x8ReplaceLane: yield return new Instructions.Int16x8ReplaceLane(reader); break;
                        case SimdOpCode.Int32x4ReplaceLane: yield return new Instructions.Int32x4ReplaceLane(reader); break;
                        case SimdOpCode.Int64x2ReplaceLane: yield return new Instructions.Int64x2ReplaceLane(reader); break;
                        case SimdOpCode.Float32x4ReplaceLane: yield return new Instructions.Float32x4ReplaceLane(reader); break;
                        case SimdOpCode.Float64x2ReplaceLane: yield return new Instructions.Float64x2ReplaceLane(reader); break;
                        // v128 bitwise
                        case SimdOpCode.V128Not: yield return new Instructions.V128Not(); break;
                        case SimdOpCode.V128And: yield return new Instructions.V128And(); break;
                        case SimdOpCode.V128AndNot: yield return new Instructions.V128AndNot(); break;
                        case SimdOpCode.V128Or: yield return new Instructions.V128Or(); break;
                        case SimdOpCode.V128Xor: yield return new Instructions.V128Xor(); break;
                        // i8x16
                        case SimdOpCode.Int8x16Abs: yield return new Instructions.Int8x16Abs(); break;
                        case SimdOpCode.Int8x16Neg: yield return new Instructions.Int8x16Neg(); break;
                        case SimdOpCode.Int8x16Add: yield return new Instructions.Int8x16Add(); break;
                        case SimdOpCode.Int8x16Sub: yield return new Instructions.Int8x16Sub(); break;
                        case SimdOpCode.Int8x16AddSaturateSigned: yield return new Instructions.Int8x16AddSaturateSigned(); break;
                        case SimdOpCode.Int8x16AddSaturateUnsigned: yield return new Instructions.Int8x16AddSaturateUnsigned(); break;
                        case SimdOpCode.Int8x16SubSaturateSigned: yield return new Instructions.Int8x16SubSaturateSigned(); break;
                        case SimdOpCode.Int8x16SubSaturateUnsigned: yield return new Instructions.Int8x16SubSaturateUnsigned(); break;
                        case SimdOpCode.Int8x16MinSigned: yield return new Instructions.Int8x16MinSigned(); break;
                        case SimdOpCode.Int8x16MinUnsigned: yield return new Instructions.Int8x16MinUnsigned(); break;
                        case SimdOpCode.Int8x16MaxSigned: yield return new Instructions.Int8x16MaxSigned(); break;
                        case SimdOpCode.Int8x16MaxUnsigned: yield return new Instructions.Int8x16MaxUnsigned(); break;
                        // i16x8
                        case SimdOpCode.Int16x8Abs: yield return new Instructions.Int16x8Abs(); break;
                        case SimdOpCode.Int16x8Neg: yield return new Instructions.Int16x8Neg(); break;
                        case SimdOpCode.Int16x8Add: yield return new Instructions.Int16x8Add(); break;
                        case SimdOpCode.Int16x8Sub: yield return new Instructions.Int16x8Sub(); break;
                        case SimdOpCode.Int16x8Mul: yield return new Instructions.Int16x8Mul(); break;
                        case SimdOpCode.Int16x8AddSaturateSigned: yield return new Instructions.Int16x8AddSaturateSigned(); break;
                        case SimdOpCode.Int16x8AddSaturateUnsigned: yield return new Instructions.Int16x8AddSaturateUnsigned(); break;
                        case SimdOpCode.Int16x8SubSaturateSigned: yield return new Instructions.Int16x8SubSaturateSigned(); break;
                        case SimdOpCode.Int16x8SubSaturateUnsigned: yield return new Instructions.Int16x8SubSaturateUnsigned(); break;
                        case SimdOpCode.Int16x8MinSigned: yield return new Instructions.Int16x8MinSigned(); break;
                        case SimdOpCode.Int16x8MinUnsigned: yield return new Instructions.Int16x8MinUnsigned(); break;
                        case SimdOpCode.Int16x8MaxSigned: yield return new Instructions.Int16x8MaxSigned(); break;
                        case SimdOpCode.Int16x8MaxUnsigned: yield return new Instructions.Int16x8MaxUnsigned(); break;
                        // i32x4
                        case SimdOpCode.Int32x4Abs: yield return new Instructions.Int32x4Abs(); break;
                        case SimdOpCode.Int32x4Neg: yield return new Instructions.Int32x4Neg(); break;
                        case SimdOpCode.Int32x4Add: yield return new Instructions.Int32x4Add(); break;
                        case SimdOpCode.Int32x4Sub: yield return new Instructions.Int32x4Sub(); break;
                        case SimdOpCode.Int32x4Mul: yield return new Instructions.Int32x4Mul(); break;
                        case SimdOpCode.Int32x4MinSigned: yield return new Instructions.Int32x4MinSigned(); break;
                        case SimdOpCode.Int32x4MinUnsigned: yield return new Instructions.Int32x4MinUnsigned(); break;
                        case SimdOpCode.Int32x4MaxSigned: yield return new Instructions.Int32x4MaxSigned(); break;
                        case SimdOpCode.Int32x4MaxUnsigned: yield return new Instructions.Int32x4MaxUnsigned(); break;
                        // i64x2
                        case SimdOpCode.Int64x2Abs: yield return new Instructions.Int64x2Abs(); break;
                        case SimdOpCode.Int64x2Neg: yield return new Instructions.Int64x2Neg(); break;
                        case SimdOpCode.Int64x2Add: yield return new Instructions.Int64x2Add(); break;
                        case SimdOpCode.Int64x2Sub: yield return new Instructions.Int64x2Sub(); break;
                        case SimdOpCode.Int64x2Mul: yield return new Instructions.Int64x2Mul(); break;
                        // f32x4
                        case SimdOpCode.Float32x4Abs: yield return new Instructions.Float32x4Abs(); break;
                        case SimdOpCode.Float32x4Neg: yield return new Instructions.Float32x4Neg(); break;
                        case SimdOpCode.Float32x4Sqrt: yield return new Instructions.Float32x4Sqrt(); break;
                        case SimdOpCode.Float32x4Ceil: yield return new Instructions.Float32x4Ceil(); break;
                        case SimdOpCode.Float32x4Floor: yield return new Instructions.Float32x4Floor(); break;
                        case SimdOpCode.Float32x4Trunc: yield return new Instructions.Float32x4Trunc(); break;
                        case SimdOpCode.Float32x4Nearest: yield return new Instructions.Float32x4Nearest(); break;
                        case SimdOpCode.Float32x4Add: yield return new Instructions.Float32x4Add(); break;
                        case SimdOpCode.Float32x4Sub: yield return new Instructions.Float32x4Sub(); break;
                        case SimdOpCode.Float32x4Mul: yield return new Instructions.Float32x4Mul(); break;
                        case SimdOpCode.Float32x4Div: yield return new Instructions.Float32x4Div(); break;
                        case SimdOpCode.Float32x4Min: yield return new Instructions.Float32x4Min(); break;
                        case SimdOpCode.Float32x4Max: yield return new Instructions.Float32x4Max(); break;
                        case SimdOpCode.Float32x4Pmin: yield return new Instructions.Float32x4Pmin(); break;
                        case SimdOpCode.Float32x4Pmax: yield return new Instructions.Float32x4Pmax(); break;
                        // f64x2
                        case SimdOpCode.Float64x2Abs: yield return new Instructions.Float64x2Abs(); break;
                        case SimdOpCode.Float64x2Neg: yield return new Instructions.Float64x2Neg(); break;
                        case SimdOpCode.Float64x2Sqrt: yield return new Instructions.Float64x2Sqrt(); break;
                        case SimdOpCode.Float64x2Ceil: yield return new Instructions.Float64x2Ceil(); break;
                        case SimdOpCode.Float64x2Floor: yield return new Instructions.Float64x2Floor(); break;
                        case SimdOpCode.Float64x2Trunc: yield return new Instructions.Float64x2Trunc(); break;
                        case SimdOpCode.Float64x2Nearest: yield return new Instructions.Float64x2Nearest(); break;
                        case SimdOpCode.Float64x2Add: yield return new Instructions.Float64x2Add(); break;
                        case SimdOpCode.Float64x2Sub: yield return new Instructions.Float64x2Sub(); break;
                        case SimdOpCode.Float64x2Mul: yield return new Instructions.Float64x2Mul(); break;
                        case SimdOpCode.Float64x2Div: yield return new Instructions.Float64x2Div(); break;
                        case SimdOpCode.Float64x2Min: yield return new Instructions.Float64x2Min(); break;
                        case SimdOpCode.Float64x2Max: yield return new Instructions.Float64x2Max(); break;
                        case SimdOpCode.Float64x2Pmin: yield return new Instructions.Float64x2Pmin(); break;
                        case SimdOpCode.Float64x2Pmax: yield return new Instructions.Float64x2Pmax(); break;
                        // --- comparisons ---
                        case SimdOpCode.Int8x16Equal: yield return new Instructions.Int8x16Equal(); break;
                        case SimdOpCode.Int8x16NotEqual: yield return new Instructions.Int8x16NotEqual(); break;
                        case SimdOpCode.Int8x16LessThanSigned: yield return new Instructions.Int8x16LessThanSigned(); break;
                        case SimdOpCode.Int8x16LessThanUnsigned: yield return new Instructions.Int8x16LessThanUnsigned(); break;
                        case SimdOpCode.Int8x16GreaterThanSigned: yield return new Instructions.Int8x16GreaterThanSigned(); break;
                        case SimdOpCode.Int8x16GreaterThanUnsigned: yield return new Instructions.Int8x16GreaterThanUnsigned(); break;
                        case SimdOpCode.Int8x16LessThanOrEqualSigned: yield return new Instructions.Int8x16LessThanOrEqualSigned(); break;
                        case SimdOpCode.Int8x16LessThanOrEqualUnsigned: yield return new Instructions.Int8x16LessThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int8x16GreaterThanOrEqualSigned: yield return new Instructions.Int8x16GreaterThanOrEqualSigned(); break;
                        case SimdOpCode.Int8x16GreaterThanOrEqualUnsigned: yield return new Instructions.Int8x16GreaterThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int16x8Equal: yield return new Instructions.Int16x8Equal(); break;
                        case SimdOpCode.Int16x8NotEqual: yield return new Instructions.Int16x8NotEqual(); break;
                        case SimdOpCode.Int16x8LessThanSigned: yield return new Instructions.Int16x8LessThanSigned(); break;
                        case SimdOpCode.Int16x8LessThanUnsigned: yield return new Instructions.Int16x8LessThanUnsigned(); break;
                        case SimdOpCode.Int16x8GreaterThanSigned: yield return new Instructions.Int16x8GreaterThanSigned(); break;
                        case SimdOpCode.Int16x8GreaterThanUnsigned: yield return new Instructions.Int16x8GreaterThanUnsigned(); break;
                        case SimdOpCode.Int16x8LessThanOrEqualSigned: yield return new Instructions.Int16x8LessThanOrEqualSigned(); break;
                        case SimdOpCode.Int16x8LessThanOrEqualUnsigned: yield return new Instructions.Int16x8LessThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int16x8GreaterThanOrEqualSigned: yield return new Instructions.Int16x8GreaterThanOrEqualSigned(); break;
                        case SimdOpCode.Int16x8GreaterThanOrEqualUnsigned: yield return new Instructions.Int16x8GreaterThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int32x4Equal: yield return new Instructions.Int32x4Equal(); break;
                        case SimdOpCode.Int32x4NotEqual: yield return new Instructions.Int32x4NotEqual(); break;
                        case SimdOpCode.Int32x4LessThanSigned: yield return new Instructions.Int32x4LessThanSigned(); break;
                        case SimdOpCode.Int32x4LessThanUnsigned: yield return new Instructions.Int32x4LessThanUnsigned(); break;
                        case SimdOpCode.Int32x4GreaterThanSigned: yield return new Instructions.Int32x4GreaterThanSigned(); break;
                        case SimdOpCode.Int32x4GreaterThanUnsigned: yield return new Instructions.Int32x4GreaterThanUnsigned(); break;
                        case SimdOpCode.Int32x4LessThanOrEqualSigned: yield return new Instructions.Int32x4LessThanOrEqualSigned(); break;
                        case SimdOpCode.Int32x4LessThanOrEqualUnsigned: yield return new Instructions.Int32x4LessThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int32x4GreaterThanOrEqualSigned: yield return new Instructions.Int32x4GreaterThanOrEqualSigned(); break;
                        case SimdOpCode.Int32x4GreaterThanOrEqualUnsigned: yield return new Instructions.Int32x4GreaterThanOrEqualUnsigned(); break;
                        case SimdOpCode.Int64x2Equal: yield return new Instructions.Int64x2Equal(); break;
                        case SimdOpCode.Int64x2NotEqual: yield return new Instructions.Int64x2NotEqual(); break;
                        case SimdOpCode.Int64x2LessThanSigned: yield return new Instructions.Int64x2LessThanSigned(); break;
                        case SimdOpCode.Int64x2GreaterThanSigned: yield return new Instructions.Int64x2GreaterThanSigned(); break;
                        case SimdOpCode.Int64x2LessThanOrEqualSigned: yield return new Instructions.Int64x2LessThanOrEqualSigned(); break;
                        case SimdOpCode.Int64x2GreaterThanOrEqualSigned: yield return new Instructions.Int64x2GreaterThanOrEqualSigned(); break;
                        case SimdOpCode.Float32x4Equal: yield return new Instructions.Float32x4Equal(); break;
                        case SimdOpCode.Float32x4NotEqual: yield return new Instructions.Float32x4NotEqual(); break;
                        case SimdOpCode.Float32x4LessThan: yield return new Instructions.Float32x4LessThan(); break;
                        case SimdOpCode.Float32x4GreaterThan: yield return new Instructions.Float32x4GreaterThan(); break;
                        case SimdOpCode.Float32x4LessThanOrEqual: yield return new Instructions.Float32x4LessThanOrEqual(); break;
                        case SimdOpCode.Float32x4GreaterThanOrEqual: yield return new Instructions.Float32x4GreaterThanOrEqual(); break;
                        case SimdOpCode.Float64x2Equal: yield return new Instructions.Float64x2Equal(); break;
                        case SimdOpCode.Float64x2NotEqual: yield return new Instructions.Float64x2NotEqual(); break;
                        case SimdOpCode.Float64x2LessThan: yield return new Instructions.Float64x2LessThan(); break;
                        case SimdOpCode.Float64x2GreaterThan: yield return new Instructions.Float64x2GreaterThan(); break;
                        case SimdOpCode.Float64x2LessThanOrEqual: yield return new Instructions.Float64x2LessThanOrEqual(); break;
                        case SimdOpCode.Float64x2GreaterThanOrEqual: yield return new Instructions.Float64x2GreaterThanOrEqual(); break;
                        // --- shifts ---
                        case SimdOpCode.Int8x16ShiftLeft: yield return new Instructions.Int8x16ShiftLeft(); break;
                        case SimdOpCode.Int8x16ShiftRightSigned: yield return new Instructions.Int8x16ShiftRightSigned(); break;
                        case SimdOpCode.Int8x16ShiftRightUnsigned: yield return new Instructions.Int8x16ShiftRightUnsigned(); break;
                        case SimdOpCode.Int16x8ShiftLeft: yield return new Instructions.Int16x8ShiftLeft(); break;
                        case SimdOpCode.Int16x8ShiftRightSigned: yield return new Instructions.Int16x8ShiftRightSigned(); break;
                        case SimdOpCode.Int16x8ShiftRightUnsigned: yield return new Instructions.Int16x8ShiftRightUnsigned(); break;
                        case SimdOpCode.Int32x4ShiftLeft: yield return new Instructions.Int32x4ShiftLeft(); break;
                        case SimdOpCode.Int32x4ShiftRightSigned: yield return new Instructions.Int32x4ShiftRightSigned(); break;
                        case SimdOpCode.Int32x4ShiftRightUnsigned: yield return new Instructions.Int32x4ShiftRightUnsigned(); break;
                        case SimdOpCode.Int64x2ShiftLeft: yield return new Instructions.Int64x2ShiftLeft(); break;
                        case SimdOpCode.Int64x2ShiftRightSigned: yield return new Instructions.Int64x2ShiftRightSigned(); break;
                        case SimdOpCode.Int64x2ShiftRightUnsigned: yield return new Instructions.Int64x2ShiftRightUnsigned(); break;
                        // --- AllTrue / Bitmask / AnyTrue ---
                        case SimdOpCode.V128AnyTrue: yield return new Instructions.V128AnyTrue(); break;
                        case SimdOpCode.Int8x16AllTrue: yield return new Instructions.Int8x16AllTrue(); break;
                        case SimdOpCode.Int8x16Bitmask: yield return new Instructions.Int8x16Bitmask(); break;
                        case SimdOpCode.Int16x8AllTrue: yield return new Instructions.Int16x8AllTrue(); break;
                        case SimdOpCode.Int16x8Bitmask: yield return new Instructions.Int16x8Bitmask(); break;
                        case SimdOpCode.Int32x4AllTrue: yield return new Instructions.Int32x4AllTrue(); break;
                        case SimdOpCode.Int32x4Bitmask: yield return new Instructions.Int32x4Bitmask(); break;
                        case SimdOpCode.Int64x2AllTrue: yield return new Instructions.Int64x2AllTrue(); break;
                        case SimdOpCode.Int64x2Bitmask: yield return new Instructions.Int64x2Bitmask(); break;
                        // --- misc unary ---
                        case SimdOpCode.Int8x16Popcnt: yield return new Instructions.Int8x16Popcnt(); break;
                        case SimdOpCode.Int8x16AvgrUnsigned: yield return new Instructions.Int8x16AvgrUnsigned(); break;
                        case SimdOpCode.Int16x8AvgrUnsigned: yield return new Instructions.Int16x8AvgrUnsigned(); break;
                        // --- narrow ---
                        case SimdOpCode.Int8x16NarrowInt16x8Signed: yield return new Instructions.Int8x16NarrowInt16x8Signed(); break;
                        case SimdOpCode.Int8x16NarrowInt16x8Unsigned: yield return new Instructions.Int8x16NarrowInt16x8Unsigned(); break;
                        case SimdOpCode.Int16x8NarrowInt32x4Signed: yield return new Instructions.Int16x8NarrowInt32x4Signed(); break;
                        case SimdOpCode.Int16x8NarrowInt32x4Unsigned: yield return new Instructions.Int16x8NarrowInt32x4Unsigned(); break;
                        // --- extend ---
                        case SimdOpCode.Int16x8ExtendLowInt8x16Signed: yield return new Instructions.Int16x8ExtendLowInt8x16Signed(); break;
                        case SimdOpCode.Int16x8ExtendHighInt8x16Signed: yield return new Instructions.Int16x8ExtendHighInt8x16Signed(); break;
                        case SimdOpCode.Int16x8ExtendLowInt8x16Unsigned: yield return new Instructions.Int16x8ExtendLowInt8x16Unsigned(); break;
                        case SimdOpCode.Int16x8ExtendHighInt8x16Unsigned: yield return new Instructions.Int16x8ExtendHighInt8x16Unsigned(); break;
                        case SimdOpCode.Int32x4ExtendLowInt16x8Signed: yield return new Instructions.Int32x4ExtendLowInt16x8Signed(); break;
                        case SimdOpCode.Int32x4ExtendHighInt16x8Signed: yield return new Instructions.Int32x4ExtendHighInt16x8Signed(); break;
                        case SimdOpCode.Int32x4ExtendLowInt16x8Unsigned: yield return new Instructions.Int32x4ExtendLowInt16x8Unsigned(); break;
                        case SimdOpCode.Int32x4ExtendHighInt16x8Unsigned: yield return new Instructions.Int32x4ExtendHighInt16x8Unsigned(); break;
                        case SimdOpCode.Int64x2ExtendLowInt32x4Signed: yield return new Instructions.Int64x2ExtendLowInt32x4Signed(); break;
                        case SimdOpCode.Int64x2ExtendHighInt32x4Signed: yield return new Instructions.Int64x2ExtendHighInt32x4Signed(); break;
                        case SimdOpCode.Int64x2ExtendLowInt32x4Unsigned: yield return new Instructions.Int64x2ExtendLowInt32x4Unsigned(); break;
                        case SimdOpCode.Int64x2ExtendHighInt32x4Unsigned: yield return new Instructions.Int64x2ExtendHighInt32x4Unsigned(); break;
                        // --- extmul ---
                        case SimdOpCode.Int16x8ExtmulLowInt8x16Signed: yield return new Instructions.Int16x8ExtmulLowInt8x16Signed(); break;
                        case SimdOpCode.Int16x8ExtmulHighInt8x16Signed: yield return new Instructions.Int16x8ExtmulHighInt8x16Signed(); break;
                        case SimdOpCode.Int16x8ExtmulLowInt8x16Unsigned: yield return new Instructions.Int16x8ExtmulLowInt8x16Unsigned(); break;
                        case SimdOpCode.Int16x8ExtmulHighInt8x16Unsigned: yield return new Instructions.Int16x8ExtmulHighInt8x16Unsigned(); break;
                        case SimdOpCode.Int32x4ExtmulLowInt16x8Signed: yield return new Instructions.Int32x4ExtmulLowInt16x8Signed(); break;
                        case SimdOpCode.Int32x4ExtmulHighInt16x8Signed: yield return new Instructions.Int32x4ExtmulHighInt16x8Signed(); break;
                        case SimdOpCode.Int32x4ExtmulLowInt16x8Unsigned: yield return new Instructions.Int32x4ExtmulLowInt16x8Unsigned(); break;
                        case SimdOpCode.Int32x4ExtmulHighInt16x8Unsigned: yield return new Instructions.Int32x4ExtmulHighInt16x8Unsigned(); break;
                        case SimdOpCode.Int64x2ExtmulLowInt32x4Signed: yield return new Instructions.Int64x2ExtmulLowInt32x4Signed(); break;
                        case SimdOpCode.Int64x2ExtmulHighInt32x4Signed: yield return new Instructions.Int64x2ExtmulHighInt32x4Signed(); break;
                        case SimdOpCode.Int64x2ExtmulLowInt32x4Unsigned: yield return new Instructions.Int64x2ExtmulLowInt32x4Unsigned(); break;
                        case SimdOpCode.Int64x2ExtmulHighInt32x4Unsigned: yield return new Instructions.Int64x2ExtmulHighInt32x4Unsigned(); break;
                        // --- extadd pairwise ---
                        case SimdOpCode.Int16x8ExtaddPairwiseInt8x16Signed: yield return new Instructions.Int16x8ExtaddPairwiseInt8x16Signed(); break;
                        case SimdOpCode.Int16x8ExtaddPairwiseInt8x16Unsigned: yield return new Instructions.Int16x8ExtaddPairwiseInt8x16Unsigned(); break;
                        case SimdOpCode.Int32x4ExtaddPairwiseInt16x8Signed: yield return new Instructions.Int32x4ExtaddPairwiseInt16x8Signed(); break;
                        case SimdOpCode.Int32x4ExtaddPairwiseInt16x8Unsigned: yield return new Instructions.Int32x4ExtaddPairwiseInt16x8Unsigned(); break;
                        // --- Q15MulrSat / Dot ---
                        case SimdOpCode.Int16x8Q15MulrSatSigned: yield return new Instructions.Int16x8Q15MulrSatSigned(); break;
                        case SimdOpCode.Int32x4DotInt16x8Signed: yield return new Instructions.Int32x4DotInt16x8Signed(); break;
                        // --- bitselect ---
                        case SimdOpCode.V128Bitselect: yield return new Instructions.V128Bitselect(); break;
                        // --- trunc sat / convert / demote / promote ---
                        case SimdOpCode.Int32x4TruncSatFloat32x4Signed: yield return new Instructions.Int32x4TruncSatFloat32x4Signed(); break;
                        case SimdOpCode.Int32x4TruncSatFloat32x4Unsigned: yield return new Instructions.Int32x4TruncSatFloat32x4Unsigned(); break;
                        case SimdOpCode.Int32x4TruncSatFloat64x2SignedZero: yield return new Instructions.Int32x4TruncSatFloat64x2SignedZero(); break;
                        case SimdOpCode.Int32x4TruncSatFloat64x2UnsignedZero: yield return new Instructions.Int32x4TruncSatFloat64x2UnsignedZero(); break;
                        case SimdOpCode.Float32x4ConvertInt32x4Signed: yield return new Instructions.Float32x4ConvertInt32x4Signed(); break;
                        case SimdOpCode.Float32x4ConvertInt32x4Unsigned: yield return new Instructions.Float32x4ConvertInt32x4Unsigned(); break;
                        case SimdOpCode.Float64x2ConvertLowInt32x4Signed: yield return new Instructions.Float64x2ConvertLowInt32x4Signed(); break;
                        case SimdOpCode.Float64x2ConvertLowInt32x4Unsigned: yield return new Instructions.Float64x2ConvertLowInt32x4Unsigned(); break;
                        case SimdOpCode.Float32x4DemoteFloat64x2Zero: yield return new Instructions.Float32x4DemoteFloat64x2Zero(); break;
                        case SimdOpCode.Float64x2PromoteLowFloat32x4: yield return new Instructions.Float64x2PromoteLowFloat32x4(); break;
                    }
                    break;
            }
        }
    }
}
