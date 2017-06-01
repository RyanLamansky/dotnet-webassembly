using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float64Constant"/> instruction.
	/// </summary>
	[TestClass]
	public class Float64ConstantTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float64Constant"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float64Constant_Compiled()
		{
			foreach (var sample in new double[]
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
				Math.PI,
				-Math.PI,
			})
			{
				Assert.AreEqual<double>(sample, AssemblyBuilder.CreateInstance<dynamic>("Test", ValueType.Float64,
					new Float64Constant(sample),
					new End()
					).Test());
			}
		}
	}
}