using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float64Floor"/> instruction.
	/// </summary>
	[TestClass]
	public class Float64FloorTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float64Floor"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float64Floor_Compiled()
		{
			var exports = CompilerTestBase<double>.CreateInstance(
				new GetLocal(0),
				new Float64Floor(),
				new End());

			foreach (var value in new[] { 1f, -1f, -Math.PI, Math.PI })
				Assert.AreEqual(Math.Floor(value), exports.Test(value));
		}
	}
}