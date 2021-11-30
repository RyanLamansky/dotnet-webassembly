using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float64PromoteFloat32"/> instruction.
/// </summary>
[TestClass]
public class Float64PromoteFloat32Tests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float64PromoteFloat32"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float64PromoteFloat32_Compiled()
    {
        var exports = ConversionTestBase<float, double>.CreateInstance(
            new LocalGet(0),
            new Float64PromoteFloat32(),
            new End());

        foreach (var value in Samples.Single)
            Assert.AreEqual(value, exports.Test(value));
    }
}
