using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
	internal sealed class CompilationContext
	{
		private readonly ILGenerator generator;
		private readonly Func<HelperMethod, MethodInfo> getHelper;

		public CompilationContext(
			ILGenerator generator,
			Compiled.Function function,
			FieldBuilder linearMemoryStart,
			Func<HelperMethod, MethodInfo> getHelper,
			ValueType[] locals
			)
		{
			Assert(generator != null);
			Assert(function != null);
			Assert(linearMemoryStart != null);
			Assert(getHelper != null);
			Assert(locals != null);

			this.generator = generator;
			this.Function = function;
			this.LinearMemoryStart = linearMemoryStart;
			this.getHelper = getHelper;
			this.Locals = locals;
		}

		public MethodInfo this[HelperMethod method] => getHelper(method);

		public readonly Compiled.Function Function;

		public readonly FieldBuilder LinearMemoryStart;

		public readonly ValueType[] Locals;

		public uint Depth = 1u;

		public OpCode Previous = OpCode.NoOperation;

		public Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

		public HashSet<Label> LoopLabels = new HashSet<Label>();

		public Stack<ValueType> Stack = new Stack<ValueType>();

		public Label DefineLabel() => generator.DefineLabel();

		public void MarkLabel(Label loc) => generator.MarkLabel(loc);

		public void Emit(System.Reflection.Emit.OpCode opcode) => generator.Emit(opcode);

		public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => generator.Emit(opcode, label);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => generator.Emit(opcode, labels);

		public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => generator.Emit(opcode, field);

		public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => generator.Emit(opcode, meth);

		public void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con) => generator.Emit(opcode, con);
	}
}