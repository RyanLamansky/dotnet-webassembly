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

    /// <summary>Export for a typed select on v128, reading a byte of the result back from memory.</summary>
    public abstract class SelectV128Export
    {
        /// <summary>Selects the first operand when <paramref name="condition"/> is non-zero, then returns the byte at <paramref name="offset"/>.</summary>
        public abstract int Test(int condition, int offset);
    }

    /// <summary>
    /// Tests that <c>select t</c> with an explicit v128 annotation compiles and returns the correct operand.
    /// Regression test: <see cref="SelectWithType"/> previously had no v128 arm and threw on this case.
    /// </summary>
    [TestMethod]
    public void SelectWithType_V128_ReturnsCorrectOperand()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(SelectV128Export.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0), // store address
                new V128Const { Value = [0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA] },
                new V128Const { Value = [0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB] },
                new LocalGet(0), // condition
                new SelectWithType(WebAssemblyValueType.V128),
                new V128Store(),
                new LocalGet(1), // byte offset to read back
                new Int32Load8Unsigned(),
                new End(),
            ],
        });

        using var compiled = module.ToInstance<SelectV128Export>();
        var exports = compiled.Exports;
        Assert.AreEqual(0xAA, exports.Test(1, 0));
        Assert.AreEqual(0xBB, exports.Test(0, 0));
        Assert.AreEqual(0xAA, exports.Test(7, 15));
        Assert.AreEqual(0xBB, exports.Test(0, 9));
    }
}
