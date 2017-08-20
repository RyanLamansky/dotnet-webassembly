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
			var exception = ExceptionAssert.Expect<StackSizeIncorrectException>(() =>
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
			var exception = ExceptionAssert.Expect<StackSizeIncorrectException>(() =>
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
			var exception = ExceptionAssert.Expect<StackSizeIncorrectException>(() =>
				AssemblyBuilder.CreateInstance<dynamic>("Test", null,
					new Int32Constant(5),
					new End()
					).Test());

			Assert.AreEqual(0, exception.Expected);
			Assert.AreEqual(1, exception.Actual);
		}
	}
}