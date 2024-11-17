using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Try"/> instruction.
/// </summary>
[TestClass]
public class CatchAllTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="CatchAll"/> instruction when there are no exceptions.
    /// </summary>
    [TestMethod]
    public void CatchAll_NoException()
    {
        Assert.AreEqual<int>(0, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Try(),
            new Int32Constant(0),
            new Return(),
            new CatchAll(),
            new Int32Constant(1),
            new Return(),
            new End(),

            new Int32Constant(2),
            new End()
        ).Test());
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="CatchAll"/> instruction when there is an exception.
    /// </summary>
    [TestMethod]
    public void CatchAll_Catch()
    {
        Assert.AreEqual<int>(0, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Try(),
            new Int32Constant(0),
            new Return(),
            new CatchAll(),
            new Int32Constant(1),
            new Return(),
            new End(),

            new Int32Constant(2),
            new End()
        ).Test());
    }
}
