using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace WebAssembly
{
	/// <summary>
	/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
	/// </summary>
	public static class Compile
	{
		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports, TImports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <typeparam name="TExports">Contains the exported features of the assembly.</typeparam>
		/// <typeparam name="TImports">Contains features imported into the assembly.</typeparam>
		/// <param name="path">The path to the file that contains a WebAssembly binary stream.</param>
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
		public static Func<Instance<TExports, TImports>> FromBinary<TExports, TImports>(string path)
			where TExports : class
			where TImports : class
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan))
			{
				return FromBinary<TExports, TImports>(stream);
			}
		}

		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports, TImports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <typeparam name="TExports">Contains the exported features of the assembly.</typeparam>
		/// <typeparam name="TImports">Contains features imported into the assembly.</typeparam>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <returns>A function that creates instances on demand.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
		public static Func<Instance<TExports, TImports>> FromBinary<TExports, TImports>(Stream input)
			where TExports : class
			where TImports : class
		{
			var exportInfo = typeof(TExports).GetTypeInfo();
			if (!exportInfo.IsPublic && !exportInfo.IsNestedPublic)
				throw new CompilerException($"Export type {exportInfo.FullName} must be public so that the compiler can inherit it.");

			ConstructorInfo constructor;
			using (var reader = new Reader(input))
			{
				try
				{
					constructor = FromBinary(reader, typeof(Instance<TExports, TImports>), typeof(TExports));
				}
				catch (OverflowException x)
				{
					throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
				}
				catch (EndOfStreamException x)
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

			return () => (Instance<TExports, TImports>)constructor.Invoke(null);
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
			System.Type exportContainer)
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
			var linearMemoryStart = exportsBuilder.DefineField("☣ Linear Memory Start", typeof(void*), FieldAttributes.Private);
			var linearMemorySize = exportsBuilder.DefineField("☣ Linear Memory Size", typeof(uint), FieldAttributes.Private);

			ILGenerator instanceConstructorIL;
			{
				var instanceConstructor = exportsBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, new System.Type[] {
					typeof(IntPtr), //linearMemoryStart
					typeof(uint), //linearMemorySize
				});
				instanceConstructorIL = instanceConstructor.GetILGenerator();
				{
					var usableConstructor = exportContainer.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
					if (usableConstructor != null)
					{
						instanceConstructorIL.Emit(OpCodes.Ldarg_0);
						instanceConstructorIL.Emit(OpCodes.Call, usableConstructor);
					}

					instanceConstructorIL.Emit(OpCodes.Ldarg_0);
					instanceConstructorIL.Emit(OpCodes.Ldarg_1);
					instanceConstructorIL.Emit(OpCodes.Stfld, linearMemoryStart);

					instanceConstructorIL.Emit(OpCodes.Ldarg_0);
					instanceConstructorIL.Emit(OpCodes.Ldarg_2);
					instanceConstructorIL.Emit(OpCodes.Stfld, linearMemorySize);
				}
			}

			var exports = exportsBuilder.AsType();
			MethodBuilder[] internalFunctions = null;
			Indirect[] functionElements = null;
			GlobalInfo[] globalGetters = null;
			GlobalInfo[] globalSetters = null;
			CompilationContext context = null;

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
								signatures[i] = new Signature(reader, (uint)i);
						}
						break;

					case Section.Function:
						{
							functionSignatures = new Signature[reader.ReadVarUInt32()];
							internalFunctions = new MethodBuilder[functionSignatures.Length];

							for (var i = 0; i < functionSignatures.Length; i++)
							{
								var signature = functionSignatures[i] = signatures[reader.ReadVarUInt32()];
								var parms = signature.ParameterTypes.Concat(new[] { exports }).ToArray();
								internalFunctions[i] = exportsBuilder.DefineMethod(
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
								memoryPagesMaximum = reader.ReadVarUInt32();
							else
								memoryPagesMaximum = memoryPagesMinimum;
						}
						break;

					case Section.Global:
						{
							var count = reader.ReadVarUInt32();
							globalGetters = new GlobalInfo[count];
							globalSetters = new GlobalInfo[count];

							context = new CompilationContext(
								exportsBuilder,
								linearMemoryStart,
								linearMemorySize,
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
									functionElements[j] = new Indirect(functionSignatures[functionIndex].TypeIndex, internalFunctions[functionIndex]);
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
									linearMemoryStart,
									linearMemorySize,
									functionSignatures,
									internalFunctions,
									signatures,
									functionElements,
									module,
									globalGetters,
									globalSetters
									);
							}

							for (var i = 0; i < functionBodies; i++)
							{
								var signature = functionSignatures[i];
								var byteLength = reader.ReadVarUInt32();
								var startingOffset = reader.Offset;

								var locals = new Local[reader.ReadVarUInt32()];
								for (var l = 0; l < locals.Length; i++)
									locals[l] = new Local(reader);

								var il = internalFunctions[i].GetILGenerator();

								context.Reset(
									il,
									signature,
									signature.RawParameterTypes.Concat(
										locals
										.SelectMany(local => Enumerable.Range(0, checked((int)local.Count)).Select(_ => local.Type))
										).ToArray()
									);

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
					var signature = functionSignatures[exported.Value];

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

			instanceConstructorIL.Emit(OpCodes.Ret); //Finish the constructor.
			var exportInfo = exportsBuilder.CreateTypeInfo();

			TypeInfo instance;
			{
				var instanceBuilder = module.DefineType("CompiledInstance", classAttributes, instanceContainer);
				var instanceConstructor = instanceBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard, null);
				var il = instanceConstructor.GetILGenerator();
				var memoryAllocated = checked(memoryPagesMaximum * Memory.PageSize);

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
					il.Emit(OpCodes.Newobj, exportInfo.DeclaredConstructors.First());

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
					il.Emit(OpCodes.Newobj, exportInfo.DeclaredConstructors.First());

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

			module.CreateGlobalFunctions();
			return instance.DeclaredConstructors.First();
		}
	}
}