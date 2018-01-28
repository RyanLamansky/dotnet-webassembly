using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebAssembly
{
	/// <summary>
	/// Tests the <see cref="Data"/> class for proper behaviors.
	/// </summary>
	[TestClass]
	public class DataTests
	{
		/// <summary>
		/// Ensures that <see cref="Data"/> instances have full mutability when read from a file.
		/// </summary>
		[TestMethod]
		public void Data_MutabilityFromBinaryFile()
		{
			var module = new Module
			{
				Data = new[]
				{
					new Data
					{
						InitializerExpression = new Instruction[]
						{
							new Instructions.Int32Constant(0),
							new Instructions.End(),
						},
					},
				},
			}.BinaryRoundTrip();

			Assert.IsNotNull(module.Data);
			Assert.AreEqual(1, module.Data.Count);

			var data = module.Data[0];
			Assert.IsNotNull(data);

			var initializerExpression = data.InitializerExpression;
			Assert.IsNotNull(initializerExpression);
			Assert.AreEqual(2, initializerExpression.Count);

			//Testing mutability here.
			initializerExpression.Clear();
			initializerExpression.Add(new Instructions.Int32Constant(0));
		}
	}
}
