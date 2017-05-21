using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Diagnostics.Debug;

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

		public readonly Function[] Functions;
		public readonly KeyValuePair<string, uint>[] ExportedFunctions;

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

			TypeInfo exports;
			var exportsBuilder = module.DefineType("CompiledExports", classAttributes, exportContainer);
			{
				var instanceConstructor = exportsBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, null);
				var il = instanceConstructor.GetILGenerator();
				{
					var usableConstructor = exportContainer.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
					if (usableConstructor != null)
					{
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Call, usableConstructor);
					}
				}
				il.Emit(OpCodes.Ret);

				var exportedFunctions = this.ExportedFunctions;

				if (exportedFunctions != null)
				{
					var labels = new Dictionary<uint, Label>();

					for (var i = 0; i < exportedFunctions.Length; i++)
					{
						labels.Clear();

						var exported = exportedFunctions[i];
						var func = this.Functions[exported.Value];

						var method = exportsBuilder.DefineMethod(
							exported.Key,
							exportedFunctionAttributes,
							CallingConventions.Standard,
							func.Signature.return_types.FirstOrDefault(),
							func.Signature.param_types
							);

						il = method.GetILGenerator();
						var depth = 1u;
						for (var j = 0; j < func.Instructions.Length; j++)
						{
							var instruction = func.Instructions[j];
							switch (instruction.OpCode)
							{
								default: throw new NotSupportedException($"Instruction {instruction.OpCode} is unknown or unsupported.");

								case OpCode.Block:
									labels.Add(depth++, il.DefineLabel());
									break;

								case OpCode.End:
									Assert(depth > 0);
									if (--depth == 0)
										il.Emit(OpCodes.Ret);
									else
									{
										il.MarkLabel(labels[depth]);
										labels.Remove(depth);
									}
									break;

								case OpCode.Branch:
									{
										var br = (Instructions.Branch)instruction;
										il.Emit(OpCodes.Br, labels[depth - br.Index - 1]);
									}
									break;

								case OpCode.BranchTable:
									{
										var br_table = (Instructions.BranchTable)instruction;
										il.Emit(OpCodes.Switch, br_table.Labels.Select(index => labels[depth - index - 1]).ToArray());
										il.Emit(OpCodes.Br, labels[depth - br_table.DefaultLabel - 1]);
									}
									break;

								case OpCode.Return:
									il.Emit(OpCodes.Ret);
									break;

								case OpCode.GetLocal:
									{
										var get_local = (Instructions.GetLocal)instruction;

										var localIndex = get_local.Index - func.Signature.param_types.Length;
										if (localIndex < 0)
										{
											//Referring to a parameter.
											switch (get_local.Index)
											{
												default:
													il.Emit(OpCodes.Ldarg, checked((int)get_local.Index));
													break;

												//Argument 0 is for the "this" parameter, allowing access to features unique to the WASM instance.
												case 0: il.Emit(OpCodes.Ldarg_1); break;
												case 1: il.Emit(OpCodes.Ldarg_2); break;
												case 2: il.Emit(OpCodes.Ldarg_3); break;
											}
										}
										else
										{
											//Referring to a local.
											switch (localIndex)
											{
												default:
													il.Emit(OpCodes.Ldloc, checked((int)localIndex));
													break;

												case 0: il.Emit(OpCodes.Ldloc_0); break;
												case 1: il.Emit(OpCodes.Ldloc_1); break;
												case 2: il.Emit(OpCodes.Ldloc_2); break;
												case 3: il.Emit(OpCodes.Ldloc_3); break;
											}
										}
									}
									break;

								case OpCode.Int32Constant:
									{
										var i32const = (Instructions.Int32Constant)instruction;
										switch (i32const.Value)
										{
											default:
												il.Emit(OpCodes.Ldc_I4, i32const.Value);
												break;

											case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
											case 0: il.Emit(OpCodes.Ldc_I4_0); break;
											case 1: il.Emit(OpCodes.Ldc_I4_1); break;
											case 2: il.Emit(OpCodes.Ldc_I4_2); break;
											case 3: il.Emit(OpCodes.Ldc_I4_3); break;
											case 4: il.Emit(OpCodes.Ldc_I4_4); break;
											case 5: il.Emit(OpCodes.Ldc_I4_5); break;
											case 6: il.Emit(OpCodes.Ldc_I4_6); break;
											case 7: il.Emit(OpCodes.Ldc_I4_7); break;
											case 8: il.Emit(OpCodes.Ldc_I4_8); break;
										}
									}
									break;
							}

							if (depth == 0)
								break;
						}
					}
				}
			}

			exports = exportsBuilder.CreateTypeInfo();

			TypeInfo instance;
			{
				var instanceBuilder = module.DefineType("CompiledInstance", classAttributes, instanceContainer);
				var instanceConstructor = instanceBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, null);
				var il = instanceConstructor.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, exports.DeclaredConstructors.First());
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