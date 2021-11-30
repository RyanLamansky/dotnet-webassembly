using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="LocalTee"/> instruction.
/// </summary>
[TestClass]
public class LocalTeeTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="LocalTee"/> instruction.
    /// </summary>
    [TestMethod]
    public void TeeLocal_Compiled()
    {
        var exports = CompilerTestBase<int>.CreateInstance(
            new LocalGet(0),
            new LocalTee(0),
            new End());

        Assert.AreEqual(3, exports.Test(3));
        Assert.AreEqual(-1, exports.Test(-1));
    }
}
