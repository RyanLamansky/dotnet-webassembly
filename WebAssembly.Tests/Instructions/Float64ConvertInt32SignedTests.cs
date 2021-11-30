using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64ConvertInt32Signed"/> instruction.
/// </summary>
[TestClass]
public class Float64ConvertInt32SignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64ConvertInt32Signed"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64ConvertSignedInt32_Compiled()
    {
        var exports = ConversionTestBase<int, double>.CreateInstance(
            new LocalGet(0),
            new Float64ConvertInt32Signed(),
            new End());

        foreach (var value in Samples.Int32)
            Assert.AreEqual(value, exports.Test(value));
    }
}
