using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Float32Multiply"/> instruction.
/// </summary>
[TestClass]
public class Float32MultiplyTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Float32Multiply"/> instruction.
    /// </summary>
    [TestMethod]
    public void Float32Multiply_Compiled()
    {
        var exports = CompilerTestBase<float>.CreateInstance(
            new LocalGet(0),
            new Float32Constant(3),
            new Float32Multiply(),
            new End());

        Assert.AreEqual(0, exports.Test(0));
        Assert.AreEqual(9, exports.Test(3));
        Assert.AreEqual(-6, exports.Test(-2));
    }
}
