using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="LocalSet"/> instruction.
/// </summary>
[TestClass]
public class LocalSetTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="LocalSet"/> instruction.
    /// </summary>
    [TestMethod]
    public void SetLocal_Compiled()
    {
        var exports = CompilerTestBase<int>.CreateInstance(
            new LocalGet(0),
            new Int32Constant(1),
            new Int32Add(),
            new LocalSet(0),
            new LocalGet(0),
            new Int32Constant(1),
            new Int32Add(),
            new End());

        Assert.AreEqual(2, exports.Test(0));
        Assert.AreEqual(3, exports.Test(1));
    }

    /// <summary>
    /// Tests the error message when a local is referenced that isn't defined for the <see cref="LocalSet"/> instruction.
    /// </summary>
    [TestMethod]
    public void GetLocal_Compiled_Parameter_OutOfRange()
    {
        var exception = Assert.ThrowsException<ModuleLoadException>(
            () => CompilerTestBase<int>.CreateInstance(
                new Int32Constant(42),
                new LocalSet(1),
                new End()
                ));
        Assert.AreEqual("At offset 39: Attempt to set local at index 1 but only 1 local was defined.", exception.Message);
    }
}
