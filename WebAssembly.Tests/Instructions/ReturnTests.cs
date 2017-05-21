using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Return"/> instruction.
	/// </summary>
	[TestClass]
	public class ReturnTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Return"/> instruction.
		/// </summary>
		[TestMethod]
		public void Return_Compiled()
		{
			AssemblyBuilder.CreateInstance<dynamic>("Test", null, new Return(), new End()).Test();
		}

		/// <summary>
		/// Tests compilation and execution of the <see cref="Return"/> instruction with a value.
		/// </summary>
		[TestMethod]
		public void Return_Compiled_WithValue()
		{
			Assert.AreEqual<int>(4, AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int32,
				new Int32Constant(4),
				new Return(),
				new End()
				).Test());
		}
	}
}