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
	}
}