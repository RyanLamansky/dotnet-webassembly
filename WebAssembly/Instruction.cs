using System;
using System.Collections.Generic;

namespace WebAssembly
{
	using Instructions;

	/// <summary>
	/// A combination of <see cref="OpCode"/> and its associated parameters.
	/// </summary>
	public abstract class Instruction
	{
		/// <summary>
		/// Creates a new <see cref="Instruction"/> instance.
		/// </summary>
		internal Instruction()
		{
		}

		/// <summary>
		/// Gets the <see cref="OpCode"/> associated with this instruction.
		/// </summary>
		public abstract OpCode OpCode { get; }

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
				var opCode = (OpCode)reader.ReadByte();
				switch (opCode)
				{
					default: throw new ModuleLoadException($"Don't know how to parse opcode \"{opCode}\".", reader.Offset);
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

			while (true)
			{
				var opCode = (OpCode)reader.ReadByte();
				switch (opCode)
				{
					default: throw new ModuleLoadException($"Don't know how to parse opcode \"{opCode}\".", reader.Offset);
					case OpCode.Unreachable: yield return new Unreachable(); break;
					case OpCode.NoOperation: yield return new NoOperation(); break;
					case OpCode.Block: yield return new Block(reader); break;
					case OpCode.Loop: yield return new Loop(reader); break;
					case OpCode.If: yield return new If(reader); break;
					case OpCode.Else: yield return new Else(); break;
					case OpCode.End: yield return new End(); break;
					case OpCode.Branch: yield return new Branch(); break;
					case OpCode.BranchIf: yield return new BranchIf(); break;
					case OpCode.BranchTable: yield return new BranchTable(); break;
					case OpCode.Return: yield return new Return(); break;
					case OpCode.Call: yield return new Call(); break;
					case OpCode.CallIndirect: yield return new CallIndirect(); break;
					case OpCode.Drop: yield return new Drop(); break;
					case OpCode.Select: yield return new Select(); break;
					case OpCode.GetLocal: yield return new GetLocal(); break;
					case OpCode.SetLocal: yield return new SetLocal(); break;
					case OpCode.TeeLocal: yield return new TeeLocal(); break;
					case OpCode.GetGlobal: yield return new GetGlobal(); break;
					case OpCode.SetGlobal: yield return new SetGlobal(); break;
					case OpCode.Int32Load: yield return new Int32Load(); break;
					case OpCode.Int64Load: yield return new Int64Load(); break;
					case OpCode.Float32Load: yield return new Float32Load(); break;
					case OpCode.Float64Load: yield return new Float64Load(); break;
					case OpCode.Int32Load8Signed: yield return new Int32Load8Signed(); break;
					case OpCode.Int32Load8Unsigned: yield return new Int32Load8Unsigned(); break;
					case OpCode.Int32Load16Signed: yield return new Int32Load16Signed(); break;
					case OpCode.Int32Load16Unsigned: yield return new Int32Load16Unsigned(); break;
					case OpCode.Int64Load8Signed: yield return new Int64Load8Signed(); break;
					case OpCode.Int64Load8Unsigned: yield return new Int64Load8Unsigned(); break;
					case OpCode.Int64Load16Signed: yield return new Int64Load16Signed(); break;
					case OpCode.Int64Load16Unsigned: yield return new Int64Load16Unsigned(); break;
					case OpCode.Int64Load32Signed: yield return new Int64Load32Signed(); break;
					case OpCode.Int64Load32Unsigned: yield return new Int64Load32Unsigned(); break;
					case OpCode.Int32Store: yield return new Int32Store(); break;
					case OpCode.Int64Store: yield return new Int64Store(); break;
					case OpCode.Float32Store: yield return new Float32Store(); break;
					case OpCode.Float64Store: yield return new Float64Store(); break;
					case OpCode.Int32Store8: yield return new Int32Store8(); break;
					case OpCode.Int32Store16: yield return new Int32Store16(); break;
					case OpCode.Int64Store8: yield return new Int64Store8(); break;
					case OpCode.Int64Store16: yield return new Int64Store16(); break;
					case OpCode.Int64Store32: yield return new Int64Store32(); break;
					case OpCode.CurrentMemory: yield return new CurrentMemory(); break;
					case OpCode.GrowMemory: yield return new GrowMemory(); break;
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
					case OpCode.Int32TruncateSignedFloat32: yield return new Int32TruncateSignedFloat32(); break;
					case OpCode.Int32TruncateUnsignedFloat32: yield return new Int32TruncateUnsignedFloat32(); break;
					case OpCode.Int32TruncateSignedFloat64: yield return new Int32TruncateSignedFloat64(); break;
					case OpCode.Int32TruncateUnsignedFloat64: yield return new Int32TruncateUnsignedFloat64(); break;
					case OpCode.Int64ExtendSignedInt32: yield return new Int64ExtendSignedInt32(); break;
					case OpCode.Int64ExtendUnsignedInt32: yield return new Int64ExtendUnsignedInt32(); break;
					case OpCode.Int64TruncateSignedFloat32: yield return new Int64TruncateSignedFloat32(); break;
					case OpCode.Int64TruncateUnsignedFloat32: yield return new Int64TruncateUnsignedFloat32(); break;
					case OpCode.Int64TruncateSignedFloat64: yield return new Int64TruncateSignedFloat64(); break;
					case OpCode.Int64TruncateUnsignedFloat64: yield return new Int64TruncateUnsignedFloat64(); break;
					case OpCode.Float32ConvertSignedInt32: yield return new Float32ConvertSignedInt32(); break;
					case OpCode.Float32ConvertUnsignedInt32: yield return new Float32ConvertUnsignedInt32(); break;
					case OpCode.Float32ConvertSignedInt64: yield return new Float32ConvertSignedInt64(); break;
					case OpCode.Float32ConvertUnsignedInt64: yield return new Float32ConvertUnsignedInt64(); break;
					case OpCode.Float32DemoteFloat64: yield return new Float32DemoteFloat64(); break;
					case OpCode.Float64ConvertSignedInt32: yield return new Float64ConvertSignedInt32(); break;
					case OpCode.Float64ConvertUnsignedInt32: yield return new Float64ConvertUnsignedInt32(); break;
					case OpCode.Float64ConvertSignedInt64: yield return new Float64ConvertSignedInt64(); break;
					case OpCode.Float64ConvertUnsignedInt64: yield return new Float64ConvertUnsignedInt64(); break;
					case OpCode.Float64PromoteFloat32: yield return new Float64PromoteFloat32(); break;
					case OpCode.Int32ReinterpretFloat32: yield return new Int32ReinterpretFloat32(); break;
					case OpCode.Int64ReinterpretFloat64: yield return new Int64ReinterpretFloat64(); break;
					case OpCode.Float32ReinterpretInt32: yield return new Float32ReinterpretInt32(); break;
					case OpCode.Float64ReinterpretInt32: yield return new Float64ReinterpretInt32(); break;
				}
			}
		}
	}
}