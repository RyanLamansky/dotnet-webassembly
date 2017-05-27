using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
	internal sealed class CompilationContext
	{
		private readonly ILGenerator generator;

		public CompilationContext(
			ILGenerator generator,
			Compiled.Function function,
			FieldBuilder linearMemoryStart,
			MethodBuilder rangeCheckInt32
			)
		{
			Assert(generator != null);
			Assert(function != null);
			Assert(linearMemoryStart != null);
			Assert(rangeCheckInt32 != null);

			this.generator = generator;
			this.Function = function;
			this.LinearMemoryStart = linearMemoryStart;
			this.RangeCheckInt32 = rangeCheckInt32;
		}

		public readonly Compiled.Function Function;

		public readonly FieldBuilder LinearMemoryStart;

		public readonly MethodBuilder RangeCheckInt32;

		public uint Depth = 1u;

		public Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

		public HashSet<Label> LoopLabels = new HashSet<Label>();

		public Label DefineLabel() => generator.DefineLabel();

		public void MarkLabel(Label loc) => generator.MarkLabel(loc);

		public void Emit(System.Reflection.Emit.OpCode opcode) => generator.Emit(opcode);

		public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => generator.Emit(opcode, label);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => generator.Emit(opcode, labels);

		public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => generator.Emit(opcode, field);

		public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => generator.Emit(opcode, meth);
	}
}