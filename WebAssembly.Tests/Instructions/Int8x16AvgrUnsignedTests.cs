using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16AvgrUnsigned"/> instruction.</summary>
[TestClass]
public class Int8x16AvgrUnsignedTests
{
    /// <summary>Verifies Int8x16AvgrUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16AvgrUnsigned_IsCorrect()
    {
        var exports = SimdTestRunner.Compile(
            new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new V128Const { Value = [3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
            new Int8x16AvgrUnsigned());
        Assert.AreEqual(2, exports.GetByte(0));
    }
}
