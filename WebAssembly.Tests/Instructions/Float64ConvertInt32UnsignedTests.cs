using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64ConvertInt32Unsigned"/> instruction.
/// </summary>
[TestClass]
public class Float64ConvertInt32UnsignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64ConvertInt32Unsigned"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64ConvertUnsignedInt32_Compiled()
    {
        var exports = ConversionTestBase<int, double>.CreateInstance(
            new LocalGet(0),
            new Float64ConvertInt32Unsigned(),
            new End());

        foreach (var value in Samples.UInt32)
            Assert.AreEqual(value, exports.Test((int)value));
    }
}
