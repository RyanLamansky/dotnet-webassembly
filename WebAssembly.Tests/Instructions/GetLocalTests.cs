using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="GetLocal"/> instruction.
	/// </summary>
	[TestClass]
	public class GetLocalTests
	{
		/// <summary>
		/// A simple test class.
		/// </summary>
		public abstract class ParameterTest
		{
			/// <summary>
			/// A simple test method.
			/// </summary>
			public abstract int Test(int value);
		}

		/// <summary>
		/// A simple test class.
		/// </summary>
		public abstract class ParameterTest2
		{
			/// <summary>
			/// A simple test method.
			/// </summary>
			public abstract int Test(int value1, int value2);
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="GetLocal"/> instruction to retrieve parameters.
		/// </summary>
		[TestMethod]
		public void GetLocal_Compiled_Parameters()
		{
			var compiled = AssemblyBuilder.CreateInstance<ParameterTest>("Test", ValueType.Int32, new[] { ValueType.Int32 },
				new GetLocal(0),
				new End()
				);

			Assert.AreEqual(2, compiled.Test(2));
			Assert.AreEqual(10, compiled.Test(10));
			Assert.AreEqual(1000, compiled.Test(1000));

			var compiled2 = AssemblyBuilder.CreateInstance<ParameterTest2>("Test", ValueType.Int32, new[] { ValueType.Int32, ValueType.Int32 },
				new GetLocal(1),
				new End()
				);

			Assert.AreEqual(3, compiled2.Test(2, 3));
			Assert.AreEqual(9, compiled2.Test(10, 9));
			Assert.AreEqual(999, compiled2.Test(1000, 999));
		}
	}
}