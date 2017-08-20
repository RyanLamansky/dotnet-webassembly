using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly
{
	internal sealed class CompilationContext
	{
		public readonly TypeBuilder ExportsBuilder;
		private ILGenerator generator;

		public CompilationContext(
			TypeBuilder exportsBuilder,
			FieldBuilder memory,
			Signature[] functionSignatures,
			MethodInfo[] methods,
			Signature[] types,
			Compile.Indirect[] functionElements,
			ModuleBuilder module,
			Compile.GlobalInfo[] globalGetters,
			Compile.GlobalInfo[] globalSetters
			)
		{
			Assert(exportsBuilder != null);
			Assert(functionSignatures != null);
			Assert(methods != null);
			Assert(types != null);
			Assert(module != null);

			this.ExportsBuilder = exportsBuilder;
			this.Memory = memory;
			this.FunctionSignatures = functionSignatures;
			this.Methods = methods;
			this.Types = types;
			this.GlobalGetters = globalGetters;
			this.GlobalSetters = globalSetters;

			if (functionElements == null)
				return;

			//Capture the information about indirectly-callable functions.
			var indirectBuilder = module.DefineType("☣ Indirect",
				TypeAttributes.Public | //Change to something more appropriate once behavior is validated.
				TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.BeforeFieldInit,
				typeof(System.ValueType)
				);

			var indirectTypeFieldBuilder = indirectBuilder.DefineField(
				"☣ Type",
				typeof(uint),
				FieldAttributes.Public | FieldAttributes.InitOnly);

			var indirectMethodFieldBuilder = indirectBuilder.DefineField(
				"☣ Method",
				typeof(IntPtr),
				FieldAttributes.Public | FieldAttributes.InitOnly);

			var indirectConstructorBuilder = indirectBuilder.DefineConstructor(
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.HasThis,
				new[]
				{
					typeof(uint),
					typeof(IntPtr),
				}
				);

			var il = indirectConstructorBuilder.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, indirectTypeFieldBuilder);

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Stfld, indirectMethodFieldBuilder);

			il.Emit(OpCodes.Ret);

			var indirectLocationsFieldBuilder = indirectBuilder.DefineField(
				"☣ Locations",
				typeof(IntPtr),
				FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);

			this.generator = il = indirectBuilder.DefineTypeInitializer().GetILGenerator();

			Instructions.Int32Constant.Emit(this, functionElements.Length);
			il.Emit(OpCodes.Newarr, indirectBuilder.AsType());

			for (var i = 0; i < functionElements.Length; i++)
			{
				var fe = functionElements[i];
				il.Emit(OpCodes.Dup);
				Instructions.Int32Constant.Emit(this, i);
				Instructions.Int32Constant.Emit(this, checked((int)fe.type));
				il.Emit(OpCodes.Ldftn, fe.function);
				il.Emit(OpCodes.Newobj, indirectConstructorBuilder);
				il.Emit(OpCodes.Stelem, indirectBuilder.AsType());
			}

			il.Emit(OpCodes.Stsfld, indirectLocationsFieldBuilder);
			il.Emit(OpCodes.Ret);

			var indirectGetFunctionPointer = indirectBuilder.DefineMethod(
				"☣ Get Function Pointer",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
				CallingConventions.Standard,
				typeof(IntPtr),
				new[]
				{
					typeof(uint),
					typeof(uint),
				}
				);

			il = indirectGetFunctionPointer.GetILGenerator();
			var value = il.DeclareLocal(indirectBuilder.AsType());
			var indexOutOfRange = il.DeclareLocal(typeof(IndexOutOfRangeException));
			var endTry = il.BeginExceptionBlock();
			il.Emit(OpCodes.Ldsfld, indirectLocationsFieldBuilder);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldelem, indirectBuilder.AsType());
			il.Emit(OpCodes.Stloc_0);
			il.Emit(OpCodes.Leave_S, endTry);

			il.BeginCatchBlock(typeof(IndexOutOfRangeException));
			il.Emit(OpCodes.Stloc_1);
			il.Emit(OpCodes.Ldstr, "index");
			il.Emit(OpCodes.Ldloc_1);
			il.Emit(OpCodes.Newobj, typeof(IndexOutOfRangeException).GetTypeInfo().DeclaredConstructors.First(c =>
			{
				var parms = c.GetParameters();
				return
					parms.Length == 2 &&
					parms[0].ParameterType == typeof(string) &&
					parms[1].ParameterType == typeof(Exception)
					;
			}));
			il.Emit(OpCodes.Throw);
			il.EndExceptionBlock();

			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldfld, indirectTypeFieldBuilder);
			var match = il.DefineLabel();
			il.Emit(OpCodes.Beq_S, match);

			il.Emit(OpCodes.Ldstr, "Type mismatch");
			il.Emit(OpCodes.Ldstr, "type");
			il.Emit(OpCodes.Newobj, typeof(ArgumentException).GetTypeInfo().DeclaredConstructors.First(c =>
			{
				var parms = c.GetParameters();
				return
					parms.Length == 2 &&
					parms[0].ParameterType == typeof(string) &&
					parms[1].ParameterType == typeof(string)
					;
			}));
			il.Emit(OpCodes.Throw);

			il.MarkLabel(match);
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ldfld, indirectMethodFieldBuilder);
			il.Emit(OpCodes.Ret);

			this.helperMethods.Add(HelperMethod.GetFunctionPointer, indirectGetFunctionPointer);
			indirectBuilder.CreateTypeInfo();
			this.generator = null;
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

		public readonly Signature[] FunctionSignatures;

		public readonly MethodInfo[] Methods;

		public readonly Signature[] Types;

		public readonly Compile.GlobalInfo[] GlobalGetters;

		public readonly Compile.GlobalInfo[] GlobalSetters;

		internal const MethodAttributes HelperMethodAttributes =
			MethodAttributes.Private |
			MethodAttributes.Static |
			MethodAttributes.HideBySig
			;

		private readonly Dictionary<HelperMethod, MethodBuilder> helperMethods = new Dictionary<HelperMethod, MethodBuilder>();

		public MethodInfo this[HelperMethod helper]
		{
			get
			{
				Assert(this.helperMethods != null);

				if (this.helperMethods.TryGetValue(helper, out var builder))
					return builder;

				Fail("Attempted to obtain an unknown helper method.");
				return null;
			}
		}

		public MethodInfo this[HelperMethod helper, Func<HelperMethod, CompilationContext, MethodBuilder> creator]
		{
			get
			{
				Assert(creator != null);
				Assert(this.helperMethods != null);
				Assert(this.ExportsBuilder != null);

				if (this.helperMethods.TryGetValue(helper, out var builder))
					return builder;

				this.helperMethods.Add(helper, builder = creator(helper, this));
				Assert(builder != null, "Helper method creator returned null.");
				return builder;
			}
		}

		public Signature Signature;

		public readonly FieldBuilder Memory;

		public ValueType[] Locals;

		public uint Depth;

		public OpCode Previous;

		public readonly Dictionary<uint, Label> Labels = new Dictionary<uint, Label>();

		public readonly HashSet<Label> LoopLabels = new HashSet<Label>();

		public readonly Stack<ValueType> Stack = new Stack<ValueType>();

		private LocalBuilder indirectPointerLocal;

		public LocalBuilder IndirectPointerLocal
		{
			get
			{
				if (this.indirectPointerLocal != null)
					return this.indirectPointerLocal;

				return this.indirectPointerLocal = this.generator.DeclareLocal(typeof(IntPtr));
			}
		}

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

		public void Emit(System.Reflection.Emit.OpCode opcode, byte arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, int arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, long arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, float arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, double arg) => generator.Emit(opcode, arg);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label label) => generator.Emit(opcode, label);

		public void Emit(System.Reflection.Emit.OpCode opcode, Label[] labels) => generator.Emit(opcode, labels);

		public void Emit(System.Reflection.Emit.OpCode opcode, FieldInfo field) => generator.Emit(opcode, field);

		public void Emit(System.Reflection.Emit.OpCode opcode, MethodInfo meth) => generator.Emit(opcode, meth);

		public void Emit(System.Reflection.Emit.OpCode opcode, ConstructorInfo con) => generator.Emit(opcode, con);

		public void EmitCalli(System.Type returnType, System.Type[] parameterTypes) => generator.EmitCalli(OpCodes.Calli, CallingConventions.Standard, returnType, parameterTypes, null);

		public LocalBuilder DeclareLocal(System.Type localType) => generator.DeclareLocal(localType);
	}
}