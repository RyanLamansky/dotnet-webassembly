using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly;

/// <summary>
/// Tests the <see cref="WebAssemblyType"/> class.
/// </summary>
[TestClass]
public class TypeTests
{
    /// <summary>
    /// Tests the <see cref="WebAssemblyType.Equals(object)"/> and <see cref="WebAssemblyType.GetHashCode()"/> methods.
    /// </summary>
    [TestMethod]
    public void Type_Equals()
    {
        TestUtility.CreateInstances<WebAssemblyType>(out var a, out var b);
        a.Form = (FunctionType)1;
        TestUtility.AreNotEqual(a, b);
        b.Form = (FunctionType)1;
        TestUtility.AreEqual(a, b);

        a.Parameters = new WebAssemblyValueType[] { };
        TestUtility.AreEqual(a, b);
        a.Parameters = new[] { WebAssemblyValueType.Int32 };
        TestUtility.AreNotEqual(a, b);
        b.Parameters = new[] { WebAssemblyValueType.Int32 };
        TestUtility.AreEqual(a, b);
        a.Parameters = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float32 };
        TestUtility.AreNotEqual(a, b);
        b.Parameters = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float64 };
        TestUtility.AreNotEqual(a, b);
        b.Parameters = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float32 };
        TestUtility.AreEqual(a, b);

        a.Returns = new WebAssemblyValueType[] { };
        TestUtility.AreEqual(a, b);
        a.Returns = new[] { WebAssemblyValueType.Int32 };
        TestUtility.AreNotEqual(a, b);
        b.Returns = new[] { WebAssemblyValueType.Int32 };
        TestUtility.AreEqual(a, b);
        a.Returns = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float32 };
        TestUtility.AreNotEqual(a, b);
        b.Returns = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float64 };
        TestUtility.AreNotEqual(a, b);
        b.Returns = new[] { WebAssemblyValueType.Int32, WebAssemblyValueType.Float32 };
        TestUtility.AreEqual(a, b);
    }
}
