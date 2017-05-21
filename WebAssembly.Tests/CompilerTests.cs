using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly
{
	using Compiled;
	using Instructions;

	/// <summary>
	/// Validates basic features of the <see cref="Compiler"/> class.
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

				var result = Compiler.FromBinary<object>(memory)();
				Assert.IsNotNull(result);
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

				compiled = Compiler.FromBinary<dynamic>(memory)();
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

				compiled = Compiler.FromBinary<HelloWorldExports>(memory)();
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

				compiled = Compiler.FromBinary<HelloWorldExportsWithConstructor>(memory)();
			}

			var exports = compiled.Exports;
			Assert.AreEqual(5, exports.SetByConstructor);
		}
	}
}