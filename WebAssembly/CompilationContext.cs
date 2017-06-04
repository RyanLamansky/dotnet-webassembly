using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
	using Compiled;

	internal sealed class CompilationContext
	{
		private readonly TypeBuilder exportsBuilder;
		private ILGenerator generator;

		public CompilationContext(TypeBuilder exportsBuilder, FieldBuilder linearMemoryStart, FieldBuilder linearMemorySize)
		{
			Assert(exportsBuilder != null);
			Assert(linearMemoryStart != null);
			Assert(linearMemorySize != null);

			this.exportsBuilder = exportsBuilder;
			this.LinearMemoryStart = linearMemoryStart;
			this.LinearMemorySize = linearMemorySize;
		}

		public void Reset(
			ILGenerator generator,
			Signature signature,
			ValueType[] locals
			)
		{
			Assert(generator != null);
			Assert(signature != null);
			Assert(locals != null);

			this.generator = generator;
			this.Signature = signature;
			this.Locals = locals;

			this.Depth = 1;
			this.Previous = OpCode.NoOperation;
			this.Labels.Clear();
			this.LoopLabels.Clear();
			this.Stack.Clear();
		}

		private readonly Dictionary<HelperMethod, MethodBuilder> helperMethods = new Dictionary<HelperMethod, MethodBuilder>();

		public MethodInfo this[HelperMethod helper]
		{
			get
			{
				Assert(this.helperMethods != null);

				if (this.helperMethods.TryGetValue(helper, out var builder))
					return builder;

				const MethodAttributes helperMethodAttributes =
					MethodAttributes.Private |
					MethodAttributes.Static |
					MethodAttributes.HideBySig
					;

				var exportsBuilder = this.exportsBuilder;

				switch (helper)
				{
					case HelperMethod.RangeCheckInt32:
						builder = exportsBuilder.DefineMethod(
							"☣ Range Check Int32",
							helperMethodAttributes,
							typeof(uint),
							new[] { typeof(uint), exportsBuilder.AsType() }
							);
						{
							var il = builder.GetILGenerator();
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ldfld, this.LinearMemorySize);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldc_I4_4);
							il.Emit(OpCodes.Add_Ovf_Un);
							var outOfRange = il.DefineLabel();
							il.Emit(OpCodes.Blt_Un_S, outOfRange);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ret);
							il.MarkLabel(outOfRange);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ldc_I4_4);
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
						}
						helperMethods.Add(HelperMethod.RangeCheckInt32, builder);
						return builder;

					case HelperMethod.SelectInt32:
						builder = exportsBuilder.DefineMethod(
							"☣ Select Int32",
							helperMethodAttributes,
							typeof(int),
							new[]
							{
								typeof(int),
								typeof(int),
								typeof(int),
							}
							);
						{
							var il = builder.GetILGenerator();
							il.Emit(OpCodes.Ldarg_2);
							var @true = il.DefineLabel();
							il.Emit(OpCodes.Brtrue_S, @true);
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ret);
							il.MarkLabel(@true);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ret);
						}
						helperMethods.Add(HelperMethod.SelectInt32, builder);
						return builder;

					case HelperMethod.SelectInt64:
						builder = exportsBuilder.DefineMethod(
							"☣ Select Int64",
							helperMethodAttributes,
							typeof(long),
							new[]
							{
								typeof(long),
								typeof(long),
								typeof(int),
							}
							);
						{
							var il = builder.GetILGenerator();
							il.Emit(OpCodes.Ldarg_2);
							var @true = il.DefineLabel();
							il.Emit(OpCodes.Brtrue_S, @true);
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ret);
							il.MarkLabel(@true);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ret);
						}
						helperMethods.Add(HelperMethod.SelectInt64, builder);
						return builder;

					case HelperMethod.SelectFloat32:
						builder = exportsBuilder.DefineMethod(
							"☣ Select Float32",
							helperMethodAttributes,
							typeof(float),
							new[]
							{
								typeof(float),
								typeof(float),
								typeof(int),
							}
							);
						{
							var il = builder.GetILGenerator();
							il.Emit(OpCodes.Ldarg_2);
							var @true = il.DefineLabel();
							il.Emit(OpCodes.Brtrue_S, @true);
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ret);
							il.MarkLabel(@true);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ret);
						}
						helperMethods.Add(HelperMethod.SelectFloat32, builder);
						return builder;

					case HelperMethod.SelectFloat64:
						builder = exportsBuilder.DefineMethod(
							"☣ Select Float64",
							helperMethodAttributes,
							typeof(double),
							new[]
							{
								typeof(double),
								typeof(double),
								typeof(int),
							}
							);
						{
							var il = builder.GetILGenerator();
							il.Emit(OpCodes.Ldarg_2);
							var @true = il.DefineLabel();
							il.Emit(OpCodes.Brtrue_S, @true);
							il.Emit(OpCodes.Ldarg_1);
							il.Emit(OpCodes.Ret);
							il.MarkLabel(@true);
							il.Emit(OpCodes.Ldarg_0);
							il.Emit(OpCodes.Ret);
						}
						helperMethods.Add(HelperMethod.SelectFloat64, builder);
						return builder;
				}

				Fail("Attempted to obtain an unknown helper method.");
				return null;
			}
		}

		public Signature Signature;

		public readonly FieldBuilder LinearMemoryStart;

		public readonly FieldBuilder LinearMemorySize;

		public ValueType[] Locals;

		public uint Depth;

		public OpCode Previous;

		public readonly Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

		public readonly HashSet<Label> LoopLabels = new HashSet<Label>();

		public readonly Stack<ValueType> Stack = new Stack<ValueType>();

		public Label DefineLabel() => generator.DefineLabel();

		public void MarkLabel(Label loc) => generator.MarkLabel(loc);

		public void EmitLoadThis()
		{
			var arg = checked((ushort)this.Signature.ParameterTypes.Length);
			System.Reflection.Emit.OpCode opCode;
			switch (arg)
			{
				case 0: opCode = OpCodes.Ldarg_0; break;
				case 1: opCode = OpCodes.Ldarg_1; break;
				case 2: opCode = OpCodes.Ldarg_2; break;
				case 3: opCode = OpCodes.Ldarg_3; break;
				default:
					if (arg <= byte.MaxValue)
						generator.Emit(OpCodes.Ldarg_S, (byte)arg);
					else
						generator.Emit(OpCodes.Ldarg, arg);
					return;
			}
			generator.Emit(opCode);
		}

		public void Emit(System.Reflection.Emit.OpCode opcode) => generator.Emit(opcode);

		public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, float arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, double arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => generator.Emit(opcode, label);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => generator.Emit(opcode, labels);

		public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => generator.Emit(opcode, field);

		public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => generator.Emit(opcode, meth);

		public void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con) => generator.Emit(opcode, con);
	}
}