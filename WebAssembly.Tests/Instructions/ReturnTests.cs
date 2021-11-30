using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Return"/> instruction.
/// </summary>
[TestClass]
public class ReturnTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction.
    /// </summary>
    [TestMethod]
    public void Return_Compiled()
    {
        AssemblyBuilder.CreateInstance<dynamic>("Test", null, new Return(), new End()).Test();
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction with a value.
    /// </summary>
    [TestMethod]
    public void Return_Compiled_WithValue()
    {
        Assert.AreEqual<int>(4, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
            new Int32Constant(4),
            new Return(),
            new End()
            ).Test());
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction where two values are returned when one is expected.
    /// </summary>
    [TestMethod]
    public void Return_Compiled_IncorrectStack_Expect1Actual2()
    {
        Assert.AreEqual<int>(2, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
                new Int32Constant(1),
                new Int32Constant(2),
                new Return(),
                new End()
                ).Test());
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction where three values are returned when one is expected.
    /// </summary>
    [TestMethod]
    public void Return_Compiled_IncorrectStack_Expect1Actual3()
    {
        Assert.AreEqual<int>(3, AssemblyBuilder.CreateInstance<dynamic>("Test", WebAssemblyValueType.Int32,
                new Int32Constant(1),
                new Int32Constant(2),
                new Int32Constant(3),
                new Return(),
                new End()
                ).Test());
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction where one value are returned when none are expected.
    /// </summary>
    [TestMethod]
    public void Return_Compiled_IncorrectStack_Expect0Actual1()
    {
        AssemblyBuilder.CreateInstance<dynamic>("Test", null,
                new Int32Constant(1),
                new Return(),
                new End()
                ).Test();
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Return"/> instruction where two values are returned when none are expected.
    /// </summary>
    [TestMethod]
    public void Return_Compiled_IncorrectStack_Expect0Actual2()
    {
        AssemblyBuilder.CreateInstance<dynamic>("Test", null,
                new Int32Constant(1),
                new Int32Constant(2),
                new Return(),
                new End()
                ).Test();
    }
}
