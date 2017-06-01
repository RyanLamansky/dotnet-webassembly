using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Int64Constant"/> instruction.
	/// </summary>
	[TestClass]
	public class Int64ConstantTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Int64Constant"/> instruction.
		/// </summary>
		[TestMethod]
		public void Int64Constant_Compiled()
		{
			foreach (var sample in new[]
			{
				-1, 0, 1, 2, 3, 4, 5, 6, 7, 8, //Dedicated .NET Opcodes
				byte.MaxValue,
				short.MinValue,
				short.MaxValue,
				ushort.MaxValue,
				int.MinValue,
				int.MaxValue,
				uint.MaxValue,
				long.MinValue,
				long.MaxValue,
			})
			{
				Assert.AreEqual<long>(sample, AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Int64,
					new Int64Constant(sample),
					new End()
					).Test());
			}
		}
	}
}