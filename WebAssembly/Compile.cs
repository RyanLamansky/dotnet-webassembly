using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly
{
	/// <summary>
	/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
	/// </summary>
	public static class Compile
	{
		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="path">The path to the file that contains a WebAssembly binary stream.</param>
		/// <param name="imports">Functionality to integrate into the WebAssembly instance.</param>
		/// <returns>The module.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> cannot be null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is an empty string (""), contains only white space, or contains one or more invalid characters; or,
		/// <paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
		/// </exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
		/// <exception cref="FileNotFoundException">The file indicated by <paramref name="path"/> could not be found.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, such as being on an unmapped drive.</exception>
		/// <exception cref="PathTooLongException">
		/// The specified path, file name, or both exceed the system-defined maximum length.
		/// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="ModuleLoadException">An error was encountered while reading the WebAssembly file.</exception>
		public static Func<Instance<TExports>> FromBinary<TExports>(string path, IEnumerable<RuntimeImport> imports = null)
		where TExports : class
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan))
			{
				return FromBinary<TExports>(stream, imports);
			}
		}

		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <param name="imports">Functionality to integrate into the WebAssembly instance.</param>
		/// <returns>A function that creates instances on demand.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
		public static Func<Instance<TExports>> FromBinary<TExports>(Stream input, IEnumerable<RuntimeImport> imports = null)
		where TExports : class
		{
			var exportInfo = typeof(TExports).GetTypeInfo();
			if (!exportInfo.IsPublic && !exportInfo.IsNestedPublic)
				throw new CompilerException($"Export type {exportInfo.FullName} must be public so that the compiler can inherit it.");

			ConstructorInfo constructor;
			using (var reader = new Reader(input))
			{
				try
				{
					constructor = FromBinary(reader, typeof(Instance<TExports>), typeof(TExports), imports);
				}
				catch (OverflowException x)
#if DEBUG
				when (!System.Diagnostics.Debugger.IsAttached)
#endif
				{
					throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
				}
				catch (EndOfStreamException x)
#if DEBUG
				when (!System.Diagnostics.Debugger.IsAttached)
#endif
				{
					throw new ModuleLoadException("Stream ended unexpectedly.", reader.Offset, x);
				}
				catch (Exception x) when (
				!(x is CompilerException)
				&& !(x is ModuleLoadException)
#if DEBUG
				&& !System.Diagnostics.Debugger.IsAttached
#endif
				)
				{
					throw new ModuleLoadException(x.Message, reader.Offset, x);
				}
			}

			return () => (Instance<TExports>)constructor.Invoke(null);
		}

		private struct Local
		{
			public Local(Reader reader)
			{
				this.Count = reader.ReadVarUInt32();
				this.Type = (ValueType)reader.ReadVarInt7();
			}

			public readonly uint Count;
			public readonly ValueType Type;
		}

		internal struct Indirect
		{
			public Indirect(uint type, MethodBuilder function)
			{
				this.type = type;
				this.function = function;
			}

			public readonly uint type;
			public readonly MethodBuilder function;
		}

		internal sealed class GlobalInfo
		{
			public readonly ValueType Type;
			public readonly bool IsMutable;
			public readonly MethodBuilder Builder;

			public GlobalInfo(ValueType type, bool isMutable, MethodBuilder builder)
			{
				this.Type = type;
				this.IsMutable = isMutable;
				this.Builder = builder;
			}

#if DEBUG
			public sealed override string ToString() => $"{this.Type} {this.IsMutable}";
#endif
		}

		private static ConstructorInfo FromBinary(
			Reader reader,
			System.Type instanceContainer,
			System.Type exportContainer,
			IEnumerable<RuntimeImport> imports
			)
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

			uint memoryPagesMinimum = 0;
			uint memoryPagesMaximum = 0;

			Signature[] signatures = null;
			Signature[] functionSignatures = null;
			KeyValuePair<string, uint>[] exportedFunctions = null;
			var previousSection = Section.None;

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

			const MethodAttributes internalFunctionAttributes =
				MethodAttributes.Assembly |
				MethodAttributes.Static |
				MethodAttributes.HideBySig
				;

			const MethodAttributes exportedFunctionAttributes =
				MethodAttributes.Public |
				MethodAttributes.Virtual |
				MethodAttributes.Final |
				MethodAttributes.HideBySig
				;

			var exportsBuilder = module.DefineType("CompiledExports", classAttributes, exportContainer);
			FieldBuilder memory = null;

			ILGenerator instanceConstructorIL;
			{
				var instanceConstructor = exportsBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, System.Type.EmptyTypes);
				instanceConstructorIL = instanceConstructor.GetILGenerator();
				{
					var usableConstructor = exportContainer.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
					if (usableConstructor != null)
					{
						instanceConstructorIL.Emit(OpCodes.Ldarg_0);
						instanceConstructorIL.Emit(OpCodes.Call, usableConstructor);
					}
				}
			}

			var exports = exportsBuilder.AsType();
			var importedFunctions = 0;
			MethodInfo[] internalFunctions = null;
			Indirect[] functionElements = null;
			GlobalInfo[] globalGetters = null;
			GlobalInfo[] globalSetters = null;
			CompilationContext context = null;
			MethodInfo startFunction = null;

			while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
			{
				var payloadLength = reader.ReadVarUInt32();
				if (id != 0 && (Section)id < previousSection)
					throw new ModuleLoadException($"Sections out of order; section {(Section)id} encounterd after {previousSection}.", reader.Offset);

				switch ((Section)id)
				{
					case Section.None:
						{
							var preNameOffset = reader.Offset;
							reader.ReadString(reader.ReadVarUInt32()); //Name
							reader.ReadBytes(payloadLength - checked((uint)(reader.Offset - preNameOffset))); //Content
						}
						break;

					case Section.Type:
						{
							signatures = new Signature[reader.ReadVarUInt32()];

							for (var i = 0; i < signatures.Length; i++)
								signatures[i] = new Signature(reader, (uint)i);
						}
						break;

					case Section.Import:
						{
							if (imports == null)
								imports = Enumerable.Empty<RuntimeImport>();

							var importsByName = imports.ToDictionary(import => new Tuple<string, string>(import.ModuleName, import.FieldName));

							var count = reader.ReadVarUInt32();
							var functionImports = new List<MethodInfo>(checked((int)count));

							for (var i = 0; i < count; i++)
							{
								var moduleName = reader.ReadString(reader.ReadVarUInt32());
								var fieldName = reader.ReadString(reader.ReadVarUInt32());

								if (!importsByName.TryGetValue(new Tuple<string, string>(moduleName, fieldName), out var import))
									throw new CompilerException($"Import not found for {moduleName}::{fieldName}.");

								var kind = (ExternalKind)reader.ReadByte();

								switch (kind)
								{
									case ExternalKind.Function:
										var typeIndex = reader.ReadVarUInt32();
										if (!(import is FunctionImport functionImport))
											throw new CompilerException($"{moduleName}::{fieldName} is expected to be a function, but provided import was not.");

										if (!signatures[typeIndex].Equals(functionImport.Type))
											throw new CompilerException($"{moduleName}::{fieldName} did not match the required type signature.");

										functionImports.Add(functionImport.Method);
										break;

									case ExternalKind.Table:
									case ExternalKind.Memory:
									case ExternalKind.Global:
										throw new ModuleLoadException($"Imported external kind of {kind} is not currently supported.", reader.Offset);

									default:
										throw new ModuleLoadException($"Imported external kind of {kind} is not recognized.", reader.Offset);
								}
							}

							importedFunctions = functionImports.Count;
							internalFunctions = functionImports.ToArray();
						}
						break;

					case Section.Function:
						{
							functionSignatures = new Signature[reader.ReadVarUInt32()];
							var importedFunctionCount = internalFunctions == null ? 0 : internalFunctions.Length;
							if (importedFunctionCount != 0)
								Array.Resize(ref internalFunctions, checked(importedFunctionCount + functionSignatures.Length));
							else
								internalFunctions = new MethodInfo[functionSignatures.Length];

							for (var i = 0; i < functionSignatures.Length; i++)
							{
								var signature = functionSignatures[i] = signatures[reader.ReadVarUInt32()];
								var parms = signature.ParameterTypes.Concat(new[] { exports }).ToArray();
								internalFunctions[importedFunctionCount + i] = exportsBuilder.DefineMethod(
									$"👻 {i}",
									internalFunctionAttributes,
									CallingConventions.Standard,
									signature.ReturnTypes.FirstOrDefault(),
									parms
									);
							}
						}
						break;

					case Section.Table:
						{
							var count = reader.ReadVarUInt32();
							for (var i = 0; i < count; i++)
							{
								var elementType = (ElementType)reader.ReadVarInt7();
								switch (elementType)
								{
									default:
										throw new ModuleLoadException($"Element type {elementType} not supported.", reader.Offset);

									case ElementType.AnyFunction:
										var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
										functionElements = new Indirect[reader.ReadVarUInt32()];
										if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
											reader.ReadVarUInt32(); //Not used.
										break;
								}
							}
						}
						break;

					case Section.Memory:
						{
							var count = reader.ReadVarUInt32();
							if (count > 1)
								throw new ModuleLoadException("Multiple memory values are not supported.", reader.Offset);

							var setFlags = (ResizableLimits.Flags)reader.ReadVarUInt32();
							memoryPagesMinimum = reader.ReadVarUInt32();
							if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
								memoryPagesMaximum = Math.Min(reader.ReadVarUInt32(), uint.MaxValue / Memory.PageSize);
							else
								memoryPagesMaximum = uint.MaxValue / Memory.PageSize;

							memory = exportsBuilder.DefineField("☣ Memory", typeof(Runtime.UnmanagedMemory), FieldAttributes.Private | FieldAttributes.InitOnly);

							instanceConstructorIL.Emit(OpCodes.Ldarg_0);
							Instructions.Int32Constant.Emit(instanceConstructorIL, (int)memoryPagesMinimum);
							Instructions.Int32Constant.Emit(instanceConstructorIL, (int)memoryPagesMaximum);
							instanceConstructorIL.Emit(OpCodes.Newobj, typeof(uint?).GetTypeInfo().DeclaredConstructors.Where(info =>
							{
								var parms = info.GetParameters();
								return parms.Length == 1 && parms[0].ParameterType == typeof(uint);
							}).First());
							instanceConstructorIL.Emit(OpCodes.Newobj, typeof(Runtime.UnmanagedMemory).GetTypeInfo().DeclaredConstructors.Where(info =>
							{
								var parms = info.GetParameters();
								return parms.Length == 2 && parms[0].ParameterType == typeof(uint) && parms[1].ParameterType == typeof(uint?);
							}).First());
							instanceConstructorIL.Emit(OpCodes.Stfld, memory);

							exportsBuilder.AddInterfaceImplementation(typeof(IDisposable));

							var dispose = exportsBuilder.DefineMethod(
								"Dispose",
								MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
								CallingConventions.HasThis,
								typeof(void),
								System.Type.EmptyTypes
								);

							var disposeIL = dispose.GetILGenerator();
							disposeIL.Emit(OpCodes.Ldarg_0);
							disposeIL.Emit(OpCodes.Ldfld, memory);
							disposeIL.Emit(OpCodes.Call, typeof(Runtime.UnmanagedMemory)
								.GetTypeInfo()
								.DeclaredMethods
								.Where(info =>
								info.ReturnType == typeof(void)
								&& info.GetParameters().Length == 0
								&& info.Name == nameof(Runtime.UnmanagedMemory.Dispose))
								.First());
							disposeIL.Emit(OpCodes.Ret);
						}
						break;

					case Section.Global:
						{
							var count = reader.ReadVarUInt32();
							globalGetters = new GlobalInfo[count];
							globalSetters = new GlobalInfo[count];

							context = new CompilationContext(
								exportsBuilder,
								memory,
								functionSignatures,
								internalFunctions,
								signatures,
								functionElements,
								module,
								globalGetters,
								globalSetters
								);

							var emptySignature = Signature.Empty;

							for (var i = 0; i < globalGetters.Length; i++)
							{
								var contentType = (ValueType)reader.ReadVarInt7();
								var isMutable = reader.ReadVarUInt1() == 1;

								var getter = exportsBuilder.DefineMethod(
									$"🌍 Get {i}",
									internalFunctionAttributes,
									CallingConventions.Standard,
									contentType.ToSystemType(),
									isMutable ? new[] { exports } : null
									);

								globalGetters[i] = new GlobalInfo(contentType, isMutable, getter);

								var il = getter.GetILGenerator();
								var getterSignature = new Signature(contentType);

								if (isMutable == false)
								{
									context.Reset(
										il,
										getterSignature,
										getterSignature.RawParameterTypes
										);

									foreach (var instruction in Instruction.ParseInitializerExpression(reader))
									{
										instruction.Compile(context);
										context.Previous = instruction.OpCode;
									}
								}
								else //Mutable
								{
									var field = exportsBuilder.DefineField(
										$"🌍 {i}",
										contentType.ToSystemType(),
										FieldAttributes.Private | (isMutable ? 0 : FieldAttributes.InitOnly)
										);

									il.Emit(OpCodes.Ldarg_0);
									il.Emit(OpCodes.Ldfld, field);
									il.Emit(OpCodes.Ret);

									var setter = exportsBuilder.DefineMethod(
									$"🌍 Set {i}",
										internalFunctionAttributes,
										CallingConventions.Standard,
										typeof(void),
										new[] { contentType.ToSystemType(), exports }
										);

									il = setter.GetILGenerator();
									il.Emit(OpCodes.Ldarg_1);
									il.Emit(OpCodes.Ldarg_0);
									il.Emit(OpCodes.Stfld, field);
									il.Emit(OpCodes.Ret);

									globalSetters[i] = new GlobalInfo(contentType, isMutable, setter);

									context.Reset(
										instanceConstructorIL,
										emptySignature,
										emptySignature.RawParameterTypes
										);

									context.EmitLoadThis();
									var ended = false;

									foreach (var instruction in Instruction.ParseInitializerExpression(reader))
									{
										if (ended)
											throw new CompilerException("Only a single End is allowed within an initializer expression.");

										if (instruction.OpCode == OpCode.End)
										{
											context.Emit(OpCodes.Stfld, field);
											ended = true;
											continue;
										}

										instruction.Compile(context);
										context.Previous = instruction.OpCode;
									}
								}
							}
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
								var index = reader.ReadVarUInt32();
								switch (kind)
								{
									case ExternalKind.Function:
										xFunctions.Add(new KeyValuePair<string, uint>(name, index));
										break;
									case ExternalKind.Memory:
										if (index != 0)
											throw new ModuleLoadException($"Exported memory must be of index 0, found {index}.", reader.Offset);
										if (memory == null)
											throw new CompilerException("Cannot export linear memory when linear memory is not defined.");

										var memoryGetter = exportsBuilder.DefineMethod("get_" + name,
											MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
											CallingConventions.HasThis,
											typeof(Runtime.UnmanagedMemory),
											System.Type.EmptyTypes
											);
										var getterIL = memoryGetter.GetILGenerator();
										getterIL.Emit(OpCodes.Ldarg_0);
										getterIL.Emit(OpCodes.Ldfld, memory);
										getterIL.Emit(OpCodes.Ret);

										exportsBuilder.DefineProperty(name, PropertyAttributes.None, typeof(Runtime.UnmanagedMemory), System.Type.EmptyTypes)
											.SetGetMethod(memoryGetter);
										break;
									default:
										throw new NotSupportedException($"Unsupported or unrecognized export kind {kind}.");
								}
							}

							exportedFunctions = xFunctions.ToArray();
						}
						break;

					case Section.Start:
						{
							var preReadOffset = reader.Offset;
							var startIndex = reader.ReadVarInt32();
							if (startIndex >= internalFunctions.Length)
								throw new ModuleLoadException($"Start function of index {startIndex} exceeds available functions of {internalFunctions.Length}", preReadOffset);

							startFunction = internalFunctions[startIndex];
						}
						break;

					case Section.Element:
						{
							if (functionElements == null)
								throw new ModuleLoadException("Element section found without an associated table section.", reader.Offset);

							var count = reader.ReadVarUInt32();
							for (var i = 0; i < count; i++)
							{
								var index = reader.ReadVarUInt32();
								if (index != 0)
									throw new ModuleLoadException($"Index value of anything other than 0 is not supported, {index} found.", reader.Offset);

								{
									var initializer = Instruction.ParseInitializerExpression(reader).ToArray();
									if (initializer.Length != 2 || !(initializer[0] is Instructions.Int32Constant c) || c.Value != 0 || !(initializer[1] is Instructions.End))
										throw new ModuleLoadException("Initializer expression support for the Element section is limited to a single Int32 constant of 0 followed by end.", reader.Offset);
								}

								var elements = reader.ReadVarUInt32();
								if (elements != functionElements.Length)
									throw new ModuleLoadException($"Element count {elements} does not match the indication provided by the earlier table {functionElements.Length}.", reader.Offset);

								for (var j = 0; j < functionElements.Length; j++)
								{
									var functionIndex = reader.ReadVarUInt32();
									functionElements[j] = new Indirect(
										functionSignatures[functionIndex].TypeIndex,
										(MethodBuilder)internalFunctions[importedFunctions + functionIndex]
										);
								}
							}
						}
						break;

					case Section.Code:
						{
							var functionBodies = reader.ReadVarUInt32();

							if (functionBodies > 0 && functionSignatures == null)
								throw new ModuleLoadException("Code section is invalid when Function section is missing.", reader.Offset);
							if (functionBodies != functionSignatures.Length)
								throw new ModuleLoadException($"Code section has {functionBodies} functions described but {functionSignatures.Length} were expected.", reader.Offset);

							if (context == null) //Might have been created by the Global section, if present.
							{
								context = new CompilationContext(
									exportsBuilder,
									memory,
									functionSignatures,
									internalFunctions,
									signatures,
									functionElements,
									module,
									globalGetters,
									globalSetters
									);
							}

							for (var functionBodyIndex = 0; functionBodyIndex < functionBodies; functionBodyIndex++)
							{
								var signature = functionSignatures[functionBodyIndex];
								var byteLength = reader.ReadVarUInt32();
								var startingOffset = reader.Offset;

								var locals = new Local[reader.ReadVarUInt32()];
								for (var localIndex = 0; localIndex < locals.Length; localIndex++)
									locals[localIndex] = new Local(reader);

								var il = ((MethodBuilder)internalFunctions[importedFunctions + functionBodyIndex]).GetILGenerator();

								context.Reset(
									il,
									signature,
									signature.RawParameterTypes.Concat(
										locals
										.SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type))
										).ToArray()
									);

								foreach (var local in locals.SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type)))
								{
									il.DeclareLocal(local.ToSystemType());
								}

								foreach (var instruction in Instruction.Parse(reader))
								{
									instruction.Compile(context);
									context.Previous = instruction.OpCode;
								}

								if (reader.Offset - startingOffset != byteLength)
									throw new ModuleLoadException($"Instruction sequence reader ended after readering {reader.Offset - startingOffset} characters, expected {byteLength}.", reader.Offset);
							}
						}
						break;

					default:
						throw new ModuleLoadException($"Unrecognized section type {(Section)id}.", reader.Offset);
				}

				previousSection = (Section)id;
			}

			if (exportedFunctions != null)
			{
				for (var i = 0; i < exportedFunctions.Length; i++)
				{
					var exported = exportedFunctions[i];
					var signature = functionSignatures[exported.Value - importedFunctions];

					var method = exportsBuilder.DefineMethod(
						exported.Key,
						exportedFunctionAttributes,
						CallingConventions.HasThis,
						signature.ReturnTypes.FirstOrDefault(),
						signature.ParameterTypes
						);

					var il = method.GetILGenerator();
					for (var parm = 0; parm < signature.ParameterTypes.Length; parm++)
						il.Emit(OpCodes.Ldarg, parm + 1);

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Call, internalFunctions[exported.Value]);
					il.Emit(OpCodes.Ret);
				}
			}

			if (startFunction != null)
			{
				instanceConstructorIL.Emit(OpCodes.Ldarg_0);
				instanceConstructorIL.Emit(OpCodes.Call, startFunction);
			}

			instanceConstructorIL.Emit(OpCodes.Ret); //Finish the constructor.
			var exportInfo = exportsBuilder.CreateTypeInfo();

			TypeInfo instance;
			{
				var instanceBuilder = module.DefineType("CompiledInstance", classAttributes, instanceContainer);
				var instanceConstructor = instanceBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, null);
				var il = instanceConstructor.GetILGenerator();
				var memoryAllocated = checked(memoryPagesMaximum * Memory.PageSize);

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Newobj, exportInfo.DeclaredConstructors.First());
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

			module.CreateGlobalFunctions();
			return instance.DeclaredConstructors.First();
		}
	}
}