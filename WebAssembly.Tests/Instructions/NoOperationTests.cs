using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="NoOperation"/> instruction.
	/// </summary>
	[TestClass]
	public class NoOperationTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="NoOperation"/> instruction.
		/// </summary>
		[TestMethod]
		public void NoOperation_Compiled()
		{
			AssemblyBuilder.CreateInstance<dynamic>("Test", null, new NoOperation(), new End()).Test();
		}
	}
}