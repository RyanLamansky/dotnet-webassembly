using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="SelectWithType"/> instruction.
/// </summary>
[TestClass]
public class SelectWithTypeTests
{
    /// <summary>Export for a typed select returning i32.</summary>
    public abstract class SelectInt32Export
    {
        /// <summary>Returns a if condition != 0, else b.</summary>
        public abstract int Test(int a, int b, int condition);
    }

    /// <summary>Export for a typed select returning f64.</summary>
    public abstract class SelectFloat64Export
    {
        /// <summary>Returns a if condition != 0, else b.</summary>
        public abstract double Test(double a, double b, int condition);
    }

    /// <summary>Export for a typed select on funcref.</summary>
    public abstract class SelectFuncRefExport
    {
        /// <summary>Returns a if condition != 0, else b.</summary>
        public abstract System.Delegate? Test(int condition);
    }

    /// <summary>
    /// Tests that select t* with i32 type returns the correct operand.
    /// </summary>
    [TestMethod]
    public void SelectWithType_Int32_ReturnsCorrectOperand()
    {
        var exports = AssemblyBuilder.CreateInstance<SelectInt32Export>(
            nameof(SelectInt32Export.Test),
            WebAssemblyValueType.Int32,
            [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            new LocalGet(0),
            new LocalGet(1),
            new LocalGet(2),
            new SelectWithType(WebAssemblyValueType.Int32),
            new End());

        Assert.AreEqual(10, exports.Test(10, 20, 1));
        Assert.AreEqual(20, exports.Test(10, 20, 0));
    }

    /// <summary>
    /// Tests that select t* with f64 type returns the correct operand.
    /// </summary>
    [TestMethod]
    public void SelectWithType_Float64_ReturnsCorrectOperand()
    {
        var exports = AssemblyBuilder.CreateInstance<SelectFloat64Export>(
            nameof(SelectFloat64Export.Test),
            WebAssemblyValueType.Float64,
            [WebAssemblyValueType.Float64, WebAssemblyValueType.Float64, WebAssemblyValueType.Int32],
            new LocalGet(0),
            new LocalGet(1),
            new LocalGet(2),
            new SelectWithType(WebAssemblyValueType.Float64),
            new End());

        Assert.AreEqual(1.5, exports.Test(1.5, 2.5, 1));
        Assert.AreEqual(2.5, exports.Test(1.5, 2.5, 0));
    }

    /// <summary>
    /// Tests that select t* with funcref type compiles and returns null for both null operands.
    /// </summary>
    [TestMethod]
    public void SelectWithType_FuncRef_ReturnsCorrectRef()
    {
        var exports = AssemblyBuilder.CreateInstance<SelectFuncRefExport>(
            nameof(SelectFuncRefExport.Test),
            WebAssemblyValueType.FuncRef,
            [WebAssemblyValueType.Int32],
            new RefNull(WebAssemblyValueType.FuncRef),
            new RefNull(WebAssemblyValueType.FuncRef),
            new LocalGet(0),
            new SelectWithType(WebAssemblyValueType.FuncRef),
            new End());

        Assert.IsNull(exports.Test(1));
        Assert.IsNull(exports.Test(0));
    }
}
