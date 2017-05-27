using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace WebAssembly.Compiled
{
	internal sealed class Parsed
	{
		public Parsed(Reader reader)
		{
			if (reader.ReadUInt32() != Module.Magic)
				throw new ModuleLoadException("File preamble magic value is incorrect.", 0);

			switch (reader.ReadUInt32())
			{
				case 0x1: //First release
				case 0xd: //Final pre-release, binary format is identical with first release.
					break;
				default:
					throw new ModuleLoadException("Unsupported version, only version 0x1 and 0xd are accepted.", 4);
			}

			Signature[] signatures = null;
			Signature[] functionSignatures = null;
			KeyValuePair<string, uint>[] exportedFunctions = null;
			Function[] functions = null;
			var previousSection = Section.None;

			while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
			{
				var payloadLength = reader.ReadVarUInt32();
				if (id != 0 && (Section)id < previousSection)
					throw new ModuleLoadException($"Sections out of order; section {(Section)id} encounterd after {previousSection}.", reader.Offset);

				switch ((Section)id)
				{
					case Section.Type:
						{
							signatures = new Signature[reader.ReadVarUInt32()];

							for (var i = 0; i < signatures.Length; i++)
								signatures[i] = new Signature(reader);
						}
						break;

					case Section.Function:
						{
							functionSignatures = new Signature[reader.ReadVarUInt32()];

							for (var i = 0; i < functionSignatures.Length; i++)
								functionSignatures[i] = signatures[reader.ReadVarUInt32()];
						}
						break;

					case Section.Memory:
						{
							var count = reader.ReadVarUInt32();
							if (count > 1)
								throw new ModuleLoadException("Multiple memory values are not supported.", reader.Offset);

							var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
							this.MemoryPagesMinimum = reader.ReadVarUInt32();
							if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
								this.MemoryPagesMaximum = reader.ReadVarUInt32();
							else
								this.MemoryPagesMaximum = this.MemoryPagesMinimum;
						}
						break;

					case Section.Export:
						{
							var totalExports = reader.ReadVarUInt32();
							var xFunctions = new List<KeyValuePair<string, uint>>((int)Math.Min(int.MaxValue, totalExports));

							for (var i = 0; i < totalExports; i++)
							{
								var name = reader.ReadString(reader.ReadVarUInt32());

								var kind = (ExternalKind)reader.ReadByte();
								switch (kind)
								{
									case ExternalKind.Function:
										xFunctions.Add(new KeyValuePair<string, uint>(name, reader.ReadVarUInt32()));
										break;
									default:
										throw new NotSupportedException($"Unsupported or unrecognized export kind {kind}.");
								}
							}

							exportedFunctions = xFunctions.ToArray();
						}
						break;

					case Section.Code:
						{
							functions = new Function[reader.ReadVarUInt32()];

							for (var i = 0; i < functions.Length; i++)
								functions[i] = new Function(reader, functionSignatures[i], reader.ReadVarUInt32());
						}
						break;

					default:
						throw new ModuleLoadException($"Unrecognized section type {id}.", reader.Offset);
				}

				previousSection = (Section)id;
			}

			this.Functions = functions;
			this.ExportedFunctions = exportedFunctions;
		}

		private readonly Function[] Functions;
		private readonly KeyValuePair<string, uint>[] ExportedFunctions;
		private readonly uint MemoryPagesMinimum;
		private readonly uint MemoryPagesMaximum;

		public ConstructorInfo Compile(System.Type instanceContainer, System.Type exportContainer)
		{
			var module = AssemblyBuilder.DefineDynamicAssembly(
				new AssemblyName("CompiledWebAssembly"),
				AssemblyBuilderAccess.RunAndCollect
				)
				.DefineDynamicModule("CompiledWebAssembly")
				;

			const TypeAttributes classAttributes =
				TypeAttributes.Public |
				TypeAttributes.Class |
				TypeAttributes.BeforeFieldInit
				;

			const MethodAttributes constructorAttributes =
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName
				;

			const MethodAttributes exportedFunctionAttributes =
				MethodAttributes.Public |
				MethodAttributes.Virtual |
				MethodAttributes.Final |
				MethodAttributes.HideBySig
				;

			const MethodAttributes rangeCheckAttributes =
				MethodAttributes.Private |
				MethodAttributes.Static |
				MethodAttributes.HideBySig
				;

			TypeInfo exports;
			var exportsBuilder = module.DefineType("CompiledExports", classAttributes, exportContainer);
			var linearMemoryStart = exportsBuilder.DefineField("☣ Linear Memory Start", typeof(void*), FieldAttributes.Private);
			var linearMemorySize = exportsBuilder.DefineField("☣ Linear Memory Size", typeof(uint), FieldAttributes.Private);

			var rangeCheckInt32 = exportsBuilder.DefineMethod(
				"☣ Range Check 32",
				rangeCheckAttributes,
				typeof(uint),
				new[] { typeof(uint), exportsBuilder.AsType() }
				);
			{
				var il = rangeCheckInt32.GetILGenerator();
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldfld, linearMemorySize);
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

			{
				var instanceConstructor = exportsBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, new System.Type[] {
					typeof(IntPtr),
					typeof(uint),
				});
				var il = instanceConstructor.GetILGenerator();
				{
					var usableConstructor = exportContainer.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
					if (usableConstructor != null)
					{
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Call, usableConstructor);
					}

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Stfld, linearMemoryStart);

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_2);
					il.Emit(OpCodes.Stfld, linearMemorySize);
				}
				il.Emit(OpCodes.Ret);

				var exportedFunctions = this.ExportedFunctions;

				if (exportedFunctions != null)
				{
					for (var i = 0; i < exportedFunctions.Length; i++)
					{
						var exported = exportedFunctions[i];
						var func = this.Functions[exported.Value];

						var method = exportsBuilder.DefineMethod(
							exported.Key,
							exportedFunctionAttributes,
							CallingConventions.HasThis,
							func.Signature.return_types.FirstOrDefault(),
							func.Signature.param_types
							);

						il = method.GetILGenerator();

						var context = new CompilationContext(il, func, linearMemoryStart, rangeCheckInt32);
						var instructions = func.Instructions;
						for (var j = 0; j < instructions.Length; j++)
							instructions[j].Compile(context);
					}
				}
			}

			exports = exportsBuilder.CreateTypeInfo();

			TypeInfo instance;
			{
				var instanceBuilder = module.DefineType("CompiledInstance", classAttributes, instanceContainer);
				var instanceConstructor = instanceBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, null);
				var il = instanceConstructor.GetILGenerator();
				var memoryAllocated = checked(this.MemoryPagesMaximum * Memory.PageSize);

				if (memoryAllocated > 0)
				{
					var start = il.DeclareLocal(typeof(IntPtr));
					il.Emit(OpCodes.Ldc_I4, memoryAllocated);
					il.Emit(OpCodes.Call, typeof(Marshal)
						.GetTypeInfo()
						.GetDeclaredMethods(nameof(Marshal.AllocHGlobal))
						.First(m =>
						{
							var parms = m.GetParameters();
							return parms.Length == 1 && parms[0].ParameterType == typeof(int);
						})
						);
					il.Emit(OpCodes.Stloc_0);

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldloc_0);
					il.Emit(OpCodes.Ldc_I4, memoryAllocated);
					il.Emit(OpCodes.Newobj, exports.DeclaredConstructors.First());

					il.Emit(OpCodes.Ldloc_0);
					il.Emit(OpCodes.Ldloc_0);
					il.Emit(OpCodes.Ldc_I4, memoryAllocated);
					il.Emit(OpCodes.Add);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Newobj, exports.DeclaredConstructors.First());

					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ldc_I4_0);
				}
				il.Emit(OpCodes.Call, instanceContainer
					.GetTypeInfo()
					.DeclaredConstructors
					.First(info => info.GetParameters()
					.FirstOrDefault()
					?.ParameterType == exportContainer
					)
					);
				il.Emit(OpCodes.Ret);

				instance = instanceBuilder.CreateTypeInfo();
			}

			return instance.DeclaredConstructors.First();
		}
	}
}