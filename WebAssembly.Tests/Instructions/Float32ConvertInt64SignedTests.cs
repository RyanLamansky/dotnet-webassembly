using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float32ConvertInt64Signed"/> instruction.
/// </summary>
[TestClass]
public class Float32ConvertInt64SignedTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float32ConvertInt64Signed"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float32ConvertSignedInt64_Compiled()
    {
        var exports = ConversionTestBase<long, float>.CreateInstance(
            new LocalGet(0),
            new Float32ConvertInt64Signed(),
            new End());

        foreach (var value in Samples.Int64)
            Assert.AreEqual(value, exports.Test(value));
    }
}
