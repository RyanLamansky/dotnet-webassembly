using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly
{
	/// <summary>
	/// Tests the <see cref="Type"/> class.
	/// </summary>
	[TestClass]
	public class TypeTests
	{
		/// <summary>
		/// Tests the <see cref="Type.Equals(object)"/> and <see cref="Type.GetHashCode()"/> methods.
		/// </summary>
		[TestMethod]
		public void Type_Equals()
		{
			TestUtility.CreateInstances<Type>(out var a, out var b);
			a.Form = (FunctionType)1;
			TestUtility.AreNotEqual(a, b);
			b.Form = (FunctionType)1;
			TestUtility.AreEqual(a, b);

			a.Parameters = new ValueType[] { };
			TestUtility.AreEqual(a, b);
			a.Parameters = new [] { ValueType.Int32 };
			TestUtility.AreNotEqual(a, b);
			b.Parameters = new[] { ValueType.Int32 };
			TestUtility.AreEqual(a, b);
			a.Parameters = new[] { ValueType.Int32, ValueType.Float32};
			TestUtility.AreNotEqual(a, b);
			b.Parameters = new[] { ValueType.Int32, ValueType.Float64 };
			TestUtility.AreNotEqual(a, b);
			b.Parameters = new[] { ValueType.Int32, ValueType.Float32 };
			TestUtility.AreEqual(a, b);

			a.Returns = new ValueType[] { };
			TestUtility.AreEqual(a, b);
			a.Returns = new[] { ValueType.Int32 };
			TestUtility.AreNotEqual(a, b);
			b.Returns = new[] { ValueType.Int32 };
			TestUtility.AreEqual(a, b);
			a.Returns = new[] { ValueType.Int32, ValueType.Float32 };
			TestUtility.AreNotEqual(a, b);
			b.Returns = new[] { ValueType.Int32, ValueType.Float64 };
			TestUtility.AreNotEqual(a, b);
			b.Returns = new[] { ValueType.Int32, ValueType.Float32 };
			TestUtility.AreEqual(a, b);
		}
	}
}