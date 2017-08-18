using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebAssembly
{
	using Instructions;

	/// <summary>
	/// Validates basic features of the <see cref="Compile"/> class.
	/// </summary>
	[TestClass]
	public class CompilerTests
	{
		/// <summary>
		/// Tests a compilation of an empty assembly.
		/// </summary>
		[TestMethod]
		public void Compile_Empty()
		{
			var module = new Module();
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				var result = Compile.FromBinary<object>(memory)();
				Assert.IsNotNull(result);
			}
		}

		/// <summary>
		/// Tests a compilation of an assembly that contains a single exported function that does nothing.
		/// </summary>
		[TestMethod]
		public void Compile_MinimalExportedFunction()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(HelloWorldExports.Start)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new End(),
				},
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<dynamic>(memory)();
			}

			compiled.Exports.Start();
		}

		/// <summary>
		/// Tests a compilation of an assembly that contains a single internal function that does nothing.
		/// </summary>
		[TestMethod]
		public void Compile_MinimalInternalFunction()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
			});
			module.Functions.Add(new Function
			{
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new End(),
				},
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<dynamic>(memory)();
			}
		}

		/// <summary>
		/// Tests a very simple program with a single exported function that returns a number, executed dynamically.
		/// </summary>
		[TestMethod]
		public void Compile_HelloWorld_Dynamic()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = new[]
				{
					ValueType.Int32,
				}
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(HelloWorldExports.Start)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new Int32Constant { Value = 8 },
				new End(),
				},
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<dynamic>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(8, exports.Start());
		}

		/// <summary>
		/// A simple test class.
		/// </summary>
		public abstract class HelloWorldExports
		{
			/// <summary>
			/// A simple test method.
			/// </summary>
			/// <returns>Should always return "3".</returns>
			public abstract int Start();
		}

		/// <summary>
		/// Tests a very simple program with a single exported function that returns a number, executed statically via <see cref="HelloWorldExports"/>.
		/// </summary>
		[TestMethod]
		public void Compile_HelloWorld_Static()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = new[]
				{
					ValueType.Int32,
				}
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(HelloWorldExports.Start)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new Int32Constant { Value = 3 },
				new End(),
				},
			});

			Instance<HelloWorldExports> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<HelloWorldExports>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(3, exports.Start());
		}

		private sealed class ForwardReadOnlyStream : Stream
		{
			private readonly byte[] data;
			private int position;

			public ForwardReadOnlyStream(byte[] data)
			{
				this.data = data;
			}

			public override bool CanRead => true;

			public override bool CanSeek => false;

			public override bool CanWrite => false;

			public override long Length => throw new NotSupportedException();

			public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

			public override void Flush() => throw new NotSupportedException();

			public override int Read(byte[] buffer, int offset, int count)
			{
				count = Math.Min(count, this.data.Length - this.position);
				if (count == 0)
					return 0;

				Array.Copy(this.data, this.position, buffer, offset, count);
				this.position += count;
				return count;
			}

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

			public override void SetLength(long value) => throw new NotSupportedException();

			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}

		/// <summary>
		/// Tests a very simple program with a read-only forward-only stream to ensure reliable streaming compilation.
		/// </summary>
		[TestMethod]
		public void Compile_Streaming()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = new[]
				{
					ValueType.Int32,
				}
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = nameof(HelloWorldExports.Start)
			});
			module.Codes.Add(new FunctionBody
			{
				Code = new Instruction[]
				{
				new Int32Constant { Value = 3 },
				new End(),
				},
			});

			Instance<HelloWorldExports> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				using (var readOnly = new ForwardReadOnlyStream(memory.ToArray()))
				{
					compiled = Compile.FromBinary<HelloWorldExports>(readOnly)();
				}
			}

			var exports = compiled.Exports;
			Assert.AreEqual(3, exports.Start());
		}

		/// <summary>
		/// A simple test class.
		/// </summary>
		public abstract class HelloWorldExportsWithConstructor
		{
			internal int SetByConstructor;

			/// <summary>
			/// Creates a new <see cref="HelloWorldExportsWithConstructor"/> instance.
			/// </summary>
			protected HelloWorldExportsWithConstructor()
			{
				this.SetByConstructor = 5;
			}
		}

		/// <summary>
		/// Ensures that the parent class's constructor is called by the generated inheritor.
		/// </summary>
		[TestMethod]
		public void Compile_CallsParentConstructor()
		{
			var module = new Module();

			Instance<HelloWorldExportsWithConstructor> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<HelloWorldExportsWithConstructor>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(5, exports.SetByConstructor);
		}

		/// <summary>
		/// Tests the compiler when linear memory is used.
		/// </summary>
		[TestMethod]
		public void Compiler_Memory()
		{
			var module = new Module();
			module.Memories.Add(new Memory(1, 1));
			module.Exports.Add(new Export
			{
				Name = "Memory",
				Kind = ExternalKind.Memory,
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<dynamic>(memory)();
			}

			Runtime.UnmanagedMemory linearMemory;
			using (compiled)
			{
				Assert.IsNotNull(compiled);
				var exports = compiled.Exports;
				Assert.IsNotNull(exports);
				linearMemory = exports.Memory;
				Assert.IsNotNull(linearMemory);
				Assert.AreNotEqual(IntPtr.Zero, linearMemory.Start);

				for (var i = 0; i < Memory.PageSize; i += 8)
					Assert.AreEqual(0, Marshal.ReadInt64(linearMemory.Start + 8));
			}

			Assert.AreEqual(IntPtr.Zero, linearMemory.Start);
		}

		/// <summary>
		/// Defends against regression of https://github.com/RyanLamansky/dotnet-webassembly/issues/4 , which revealed a bug in the local parser.
		/// </summary>
		[TestMethod]
		public void Compiler_GithubIssue4_Locals()
		{
			var module = new Module();
			module.Types.Add(new Type
			{
				Returns = new[]
				{
					ValueType.Int32,
				}
			});
			module.Functions.Add(new Function
			{
			});
			module.Exports.Add(new Export
			{
				Name = "Test",
			});
			module.Codes.Add(new FunctionBody
			{
				Locals = new[]
				{
					new Local
					{
						Count = 1,
						Type = ValueType.Int32,
					}
				},
				Code = new Instruction[]
				{
					new GetLocal(0),
					new End(),
				}
			});

			Instance<dynamic> compiled;
			using (var memory = new MemoryStream())
			{
				module.WriteToBinary(memory);
				memory.Position = 0;

				compiled = Compile.FromBinary<dynamic>(memory)();
			}

			Assert.AreEqual(0, (int)compiled.Exports.Test());
		}
	}
}