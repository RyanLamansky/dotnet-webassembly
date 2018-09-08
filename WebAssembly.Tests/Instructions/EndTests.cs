using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="End"/> instruction.
	/// </summary>
	[TestClass]
	public class EndTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction.
		/// </summary>
		[TestMethod]
		public void End_Compiled()
		{
			AssemblyBuilder.CreateInstance<dynamic>("Test", null, new End()).Test();
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction where a value is returned.
		/// </summary>
		[TestMethod]
		public void End_Compiled_SingleReturn()
		{
			Assert.AreEqual<int>(5,
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
					new Int32Constant(5),
					new End()
					).Test());
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction where multiple values are returned when one is expected.
		/// </summary>
		[TestMethod]
		public void End_Compiled_IncorrectStack_Expect1Actual2()
		{
			var exception = Assert.ThrowsException<StackSizeIncorrectException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
					new Int32Constant(5),
					new Int32Constant(6),
					new End()
					).Test());

			Assert.AreEqual(1, exception.Expected);
			Assert.AreEqual(2, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction where no values are returned when one is expected.
		/// </summary>
		[TestMethod]
		public void End_Compiled_IncorrectStack_Expect1Actual0()
		{
			var exception = Assert.ThrowsException<StackSizeIncorrectException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
					new End()
					).Test());

			Assert.AreEqual(1, exception.Expected);
			Assert.AreEqual(0, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction where one value is returned when none are expected.
		/// </summary>
		[TestMethod]
		public void End_Compiled_IncorrectStack_Expect0Actual1()
		{
			var exception = Assert.ThrowsException<StackSizeIncorrectException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", null,
					new Int32Constant(5),
					new End()
					).Test());

			Assert.AreEqual(0, exception.Expected);
			Assert.AreEqual(1, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockInt32()
		{
			Assert.AreEqual<int>(5,
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
					new Block(BlockType.Int32),
					new Int32Constant(5),
					new End(),
					new End()
					).Test());
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value of the wrong type.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockInt32_WrongType()
		{
			var exception = Assert.ThrowsException<StackTypeInvalidException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
					new Block(BlockType.Int32),
					new Int64Constant(5),
					new End(),
					new Int32WrapInt64(),
					new End()
					).Test());

			Assert.AreEqual(ValueType.Int32, exception.Expected);
			Assert.AreEqual(ValueType.Int64, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockInt64()
		{
			Assert.AreEqual<long>(5,
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int64,
					new Block(BlockType.Int64),
					new Int64Constant(5),
					new End(),
					new End()
					).Test());
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value of the wrong type.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockInt64_WrongType()
		{
			var exception = Assert.ThrowsException<StackTypeInvalidException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int64,
					new Block(BlockType.Int64),
					new Int32Constant(5),
					new End(),
					new Int64ExtendSignedInt32(),
					new End()
					).Test());

			Assert.AreEqual(ValueType.Int64, exception.Expected);
			Assert.AreEqual(ValueType.Int32, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockFloat32()
		{
			Assert.AreEqual<float>(5,
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float32,
					new Block(BlockType.Float32),
					new Float32Constant(5),
					new End(),
					new End()
					).Test());
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value of the wrong type.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockFloat32_WrongType()
		{
			var exception = Assert.ThrowsException<StackTypeInvalidException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float32,
					new Block(BlockType.Float32),
					new Float64Constant(5),
					new End(),
					new Float32DemoteFloat64(),
					new End()
					).Test());

			Assert.AreEqual(ValueType.Float32, exception.Expected);
			Assert.AreEqual(ValueType.Float64, exception.Actual);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockFloat64()
		{
			Assert.AreEqual<double>(5,
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float64,
					new Block(BlockType.Float64),
					new Float64Constant(5),
					new End(),
					new End()
					).Test());
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="End"/> instruction when used with <see cref="Block"/> to retain a value of the wrong type.
		/// </summary>
		[TestMethod]
		public void End_Compiled_BlockFloat64_WrongType()
		{
			var exception = Assert.ThrowsException<StackTypeInvalidException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float64,
					new Block(BlockType.Float64),
					new Float32Constant(5),
					new End(),
					new Float64PromoteFloat32(),
					new End()
					).Test());

			Assert.AreEqual(ValueType.Float64, exception.Expected);
			Assert.AreEqual(ValueType.Float32, exception.Actual);
		}
	}
}