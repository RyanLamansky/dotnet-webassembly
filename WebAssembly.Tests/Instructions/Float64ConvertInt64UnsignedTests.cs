using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64ConvertInt64Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Float64ConvertInt64UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64ConvertInt64Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64ConvertUnsignedInt64_Compiled()
    {
        var exports = ConversionTestBase<long, double>.CreateInstance(
            new LocalGet(0),
            new Float64ConvertInt64Unsigned(),
            new End());

        foreach (var value in Samples.UInt64)
            Assert.AreEqual(value, exports.Test((long)value));
    }
}
