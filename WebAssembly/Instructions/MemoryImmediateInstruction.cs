using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Common features of instructions that access linear memory.
	/// </summary>
	public abstract class MemoryImmediateInstruction : Instruction
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

		internal MemoryImmediateInstruction()
		{
		}

		internal MemoryImmediateInstruction(Reader reader)
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
		public override bool Equals(Instruction other) =>
			other is MemoryImmediateInstruction instruction
			&& instruction.OpCode == this.OpCode
			&& instruction.Flags == this.Flags
			&& instruction.Offset == this.Offset
			;

		/// <summary>
		/// Returns a simple hash code based on the value of the instruction.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode() => HashCode.Combine((int)this.OpCode, (int)this.Flags, (int)this.Offset);

		internal virtual ValueType Type => throw new NotImplementedException($"{this.OpCode} does not currently support compilation.");

		internal virtual byte Size => throw new NotImplementedException($"{this.OpCode} does not currently support compilation.");

		internal virtual System.Reflection.Emit.OpCode EmittedOpCode => throw new NotImplementedException($"{this.OpCode} does not currently support compilation.");

		internal sealed override void Compile(CompilationContext context)
		{
			var stack = context.Stack;
			if (stack.Count == 0)
				throw new StackTooSmallException(this.OpCode, 1, 0);

			var type = stack.Pop();
			if (type != ValueType.Int32)
				throw new StackTypeInvalidException(this.OpCode, ValueType.Int32, type);

			if (this.Offset != 0)
			{
				Int32Constant.Emit(context, (int)this.Offset);
				context.Emit(OpCodes.Add_Ovf_Un);
			}

			context.EmitLoadThis();
			HelperMethod helper;
			switch (this.Size)
			{
				default:
#if DEBUG
					Fail("Invalid size.");
					return;
#endif
				case 1: helper = HelperMethod.RangeCheck8; break;
				case 2: helper = HelperMethod.RangeCheck16; break;
				case 4: helper = HelperMethod.RangeCheck32; break;
				case 8: helper = HelperMethod.RangeCheck64; break;
			}
			context.Emit(OpCodes.Call, context[helper, CreateRangeCheck]);

			context.EmitLoadThis();
			context.Emit(OpCodes.Ldfld, context.LinearMemoryStart);
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

			if (alignment != 4)
				context.Emit(OpCodes.Unaligned, alignment);

			context.Emit(this.EmittedOpCode);

			stack.Push(this.Type);
		}

		static MethodBuilder CreateRangeCheck(HelperMethod helper, CompilationContext context)
		{
			byte size;
			System.Reflection.Emit.OpCode opCode;
			switch (helper)
			{
				default:
#if DEBUG
					Fail("Invalid size.");
					return null;
#endif
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

			var builder = context.ExportsBuilder.DefineMethod(
				$"☣ Range Check {size}",
				CompilationContext.HelperMethodAttributes,
				typeof(uint),
				new[] { typeof(uint), context.ExportsBuilder.AsType() }
				);
			var il = builder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldfld, context.LinearMemorySize);
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