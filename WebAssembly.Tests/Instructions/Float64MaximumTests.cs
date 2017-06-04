using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
	/// <summary>
	/// Tests the <see cref="Float64Maximum"/> instruction.
	/// </summary>
	[TestClass]
	public class Float64MaximumTests
	{
		/// <summary>
		/// Tests compilation and execution of the <see cref="Float64Maximum"/> instruction.
		/// </summary>
		[TestMethod]
		public void Float64Maximum_Compiled()
		{
			var exports = CompilerTestBase2<double>.CreateInstance(
				new GetLocal(0),
				new GetLocal(1),
				new Float64Maximum(),
				new End());

			var values = new[]
			{
				0d,
				1d,
				-1d,
				-Math.PI,
				Math.PI,
				double.NaN,
				double.NegativeInfinity,
				double.PositiveInfinity,
				double.Epsilon,
				-double.Epsilon,
			};

			foreach (var comparand in values)
			{
				foreach (var value in values)
					Assert.AreEqual(Math.Max(comparand, value), exports.Test(comparand, value));

				foreach (var value in values)
					Assert.AreEqual(Math.Max(value, comparand), exports.Test(value, comparand));
			}
		}
	}
}