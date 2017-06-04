using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace WebAssembly
{
	using Compiled;

	/// <summary>
	/// Provides compilation functionality.  Use <see cref="Module"/> for robust inspection and modification capability.
	/// </summary>
	public static class Compiler
	{
		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
		/// </summary>
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
		public static Func<Instance<TExports>> FromBinary<TExports>(string path)
		where TExports : class
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024, FileOptions.SequentialScan))
			{
				return FromBinary<TExports>(stream);
			}
		}

		/// <summary>
		/// Uses streaming compilation to create an executable <see cref="Instance{TExports}"/> from a binary WebAssembly source.
		/// </summary>
		/// <param name="input">The source of data.  The stream is left open after reading is complete.</param>
		/// <returns>A function that creates instances on demand.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> cannot be null.</exception>
		public static Func<Instance<TExports>> FromBinary<TExports>(Stream input)
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
					constructor = FromBinary(reader, typeof(Instance<TExports>), typeof(TExports));
				}
				catch (OverflowException x)
				{
					throw new ModuleLoadException("Overflow encountered.", reader.Offset, x);
				}
				catch (EndOfStreamException x)
				{
					throw new ModuleLoadException("Stream ended unexpectedly.", reader.Offset, x);
				}
				catch (Exception x) when (!(x is CompilerException) && !(x is ModuleLoadException))
				{
					throw new ModuleLoadException(x.Message, reader.Offset, x);
				}
			}

			return () => (Instance<TExports>)constructor.Invoke(null);
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
			Compiled.Function[] functions = null;
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
							memoryPagesMinimum = reader.ReadVarUInt32();
							if ((setFlags & ResizableLimits.Flags.Maximum) != 0)
								memoryPagesMaximum = reader.ReadVarUInt32();
							else
								memoryPagesMaximum = memoryPagesMinimum;
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
							functions = new Compiled.Function[reader.ReadVarUInt32()];

							for (var i = 0; i < functions.Length; i++)
								functions[i] = new Compiled.Function(reader, functionSignatures[i], reader.ReadVarUInt32());
						}
						break;

					default:
						throw new ModuleLoadException($"Unrecognized section type {id}.", reader.Offset);
				}

				previousSection = (Section)id;
			}

			return FromParsed(exportedFunctions, functions, memoryPagesMaximum, instanceContainer, exportContainer);
		}

		private static ConstructorInfo FromParsed(
			KeyValuePair<string, uint>[] exportedFunctions,
			Compiled.Function[] functions,
			uint memoryPagesMaximum,
			System.Type instanceContainer,
			System.Type exportContainer)
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
			var linearMemoryStart = exportsBuilder.DefineField("☣ Linear Memory Start", typeof(void*), FieldAttributes.Private);
			var linearMemorySize = exportsBuilder.DefineField("☣ Linear Memory Size", typeof(uint), FieldAttributes.Private);

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

				if (exportedFunctions != null)
				{
					var context = new CompilationContext(exportsBuilder, linearMemoryStart, linearMemorySize);

					for (var i = 0; i < exportedFunctions.Length; i++)
					{
						var exported = exportedFunctions[i];
						var func = functions[exported.Value];

						var method = exportsBuilder.DefineMethod(
							exported.Key,
							exportedFunctionAttributes,
							CallingConventions.HasThis,
							func.Signature.ReturnTypes.FirstOrDefault(),
							func.Signature.ParameterTypes
							);

						il = method.GetILGenerator();

						context.Reset(
							il,
							func,
							func.Signature.RawParameterTypes.Concat(
								func
								.Locals
								.SelectMany(locals => Enumerable.Range(0, checked((int)locals.Count)).Select(_ => locals.Type))
								).ToArray()
							);
						var instructions = func.Instructions;
						for (var j = 0; j < instructions.Length; j++)
						{
							var instruction = instructions[j];
							instruction.Compile(context);
							context.Previous = instruction.OpCode;
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