using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float32NotEqual"/> instruction.
	/// </summary>
	[TestClass]
	public class Float32NotEqualTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float32NotEqual"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float32NotEqual_Compiled()
		{
			var exports = ComparisonTestBase<float>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Float32NotEqual(),
				new End());

			var values = Samples.Single;

			foreach (var comparand in values)
			{
				foreach (var value in values)
					Assert.AreEqual(comparand != value, exports.Test(comparand, value) != 0);

				foreach (var value in values)
					Assert.AreEqual(value != comparand, exports.Test(value, comparand) != 0);
			}
		}
	}
}