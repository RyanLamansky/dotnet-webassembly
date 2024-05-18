using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="LocalGet"/> instruction.
/// </summary>
[TestClass]
public class LocalGetTests
{
    /// <summary>
    /// A simple test class.
    /// </summary>
    public abstract class ParameterTest
    {
        /// <summary>
        /// A simple test method.
        /// </summary>
        public abstract int Test(int value);
    }

    /// <summary>
    /// A simple test class.
    /// </summary>
    public abstract class ParameterTest2
    {
        /// <summary>
        /// A simple test method.
        /// </summary>
        public abstract int Test(int value1, int value2);
    }

    /// <summary>
    /// A simple test class.
    /// </summary>
    public abstract class ParameterTest3
    {
        /// <summary>
        /// A simple test method.
        /// </summary>
        public abstract int Test(int value1, int value2, int value3);
    }

    /// <summary>
    /// A simple test class.
    /// </summary>
    public abstract class ParameterTest4
    {
        /// <summary>
        /// A simple test method.
        /// </summary>
        public abstract int Test(int value1, int value2, int value3, int value4);
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="LocalGet"/> instruction to retrieve parameters.
    /// </summary>
    [TestMethod]
    public void GetLocal_Compiled_Parameters()
    {
        var compiled = AssemblyBuilder.CreateInstance<ParameterTest>("Test", WebAssemblyValueType.Int32, [WebAssemblyValueType.Int32],
            new LocalGet(0),
            new End()
            );

        Assert.AreEqual(2, compiled.Test(2));
        Assert.AreEqual(10, compiled.Test(10));
        Assert.AreEqual(1000, compiled.Test(1000));

        var compiled2 = AssemblyBuilder.CreateInstance<ParameterTest2>("Test", WebAssemblyValueType.Int32, [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            new LocalGet(1),
            new End()
            );

        Assert.AreEqual(3, compiled2.Test(2, 3));
        Assert.AreEqual(9, compiled2.Test(10, 9));
        Assert.AreEqual(999, compiled2.Test(1000, 999));

        var compiled3 = AssemblyBuilder.CreateInstance<ParameterTest3>("Test", WebAssemblyValueType.Int32,
            [
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int32,
                ],
            new LocalGet(2),
            new End()
            );

        Assert.AreEqual(3, compiled3.Test(1, 2, 3));
        Assert.AreEqual(9, compiled3.Test(11, 10, 9));
        Assert.AreEqual(999, compiled3.Test(1001, 1000, 999));

        var compiled4 = AssemblyBuilder.CreateInstance<ParameterTest4>("Test", WebAssemblyValueType.Int32,
            [
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int32,
                    WebAssemblyValueType.Int32,
                ],
            new LocalGet(3),
            new End()
            );

        Assert.AreEqual(3, compiled4.Test(0, 1, 2, 3));
        Assert.AreEqual(9, compiled4.Test(12, 11, 10, 9));
        Assert.AreEqual(999, compiled4.Test(1002, 1001, 1000, 999));
    }

    /// <summary>
    /// Tests the error message when a local is referenced that isn't defined for the <see cref="LocalGet"/> instruction.
    /// </summary>
    [TestMethod]
    public void GetLocal_Compiled_Parameter_OutOfRange()
    {
        var exception = Assert.ThrowsException<ModuleLoadException>(
            () => AssemblyBuilder.CreateInstance<ParameterTest>("Test", WebAssemblyValueType.Int32, [WebAssemblyValueType.Int32],
                new LocalGet(1),
                new End()
                ));
        Assert.AreEqual("At offset 37: Attempt to get local at index 1 but only 1 local was defined.", exception.Message);
    }
}
