using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Select"/> instruction.
/// </summary>
[TestClass]
public class SelectTests
{
    /// <summary>
    /// Tests the <see cref="Select"/> instruction.
    /// </summary>
    /// <typeparam name="T">The value to be returned.</typeparam>
    public abstract class SelectTester<T>
        where T : struct
    {
        /// <summary>
        /// Tests the <see cref="Select"/> instruction.
        /// </summary>
        public abstract T Test(T a, T b, int c);
    }

    static Instance<SelectTester<T>> CreateTester<T>(WebAssemblyValueType type)
        where T : struct
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters =
            [
                    type,
                    type,
                    WebAssemblyValueType.Int32,
            ],
            Returns =
            [
                    type,
                ],
        });
        module.Functions.Add(new Function
        {
        });
        module.Exports.Add(new Export
        {
            Name = nameof(SelectTester<T>.Test),
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                    new LocalGet(0),
                    new LocalGet(1),
                    new LocalGet(2),
                    new Select(),
                    new End(),
            ],
        });

        return module.ToInstance<SelectTester<T>>();
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Int32"/> input.
    /// </summary>
    [TestMethod]
    public void Select_Compiled_Int32()
    {
        using var compiled = CreateTester<int>(WebAssemblyValueType.Int32);
        var exports = compiled.Exports;
        Assert.AreEqual(1, exports.Test(1, 2, 3));
        Assert.AreEqual(2, exports.Test(1, 2, 0));
        Assert.AreEqual(4, exports.Test(4, 5, 1));
        Assert.AreEqual(5, exports.Test(4, 5, 0));
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Int64"/> input.
    /// </summary>
    [TestMethod]
    public void Select_Compiled_Int64()
    {
        using var compiled = CreateTester<long>(WebAssemblyValueType.Int64);
        var exports = compiled.Exports;
        Assert.AreEqual(1, exports.Test(1, 2, 3));
        Assert.AreEqual(2, exports.Test(1, 2, 0));
        Assert.AreEqual(4, exports.Test(4, 5, 1));
        Assert.AreEqual(5, exports.Test(4, 5, 0));
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Float32"/> input.
    /// </summary>
    [TestMethod]
    public void Select_Compiled_Float32()
    {
        using var compiled = CreateTester<float>(WebAssemblyValueType.Float32);
        var exports = compiled.Exports;
        Assert.AreEqual(1, exports.Test(1, 2, 3));
        Assert.AreEqual(2, exports.Test(1, 2, 0));
        Assert.AreEqual(4, exports.Test(4, 5, 1));
        Assert.AreEqual(5, exports.Test(4, 5, 0));
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.Float64"/> input.
    /// </summary>
    [TestMethod]
    public void Select_Compiled_Float64()
    {
        using var compiled = CreateTester<double>(WebAssemblyValueType.Float64);
        var exports = compiled.Exports;
        Assert.AreEqual(1, exports.Test(1, 2, 3));
        Assert.AreEqual(2, exports.Test(1, 2, 0));
        Assert.AreEqual(4, exports.Test(4, 5, 1));
        Assert.AreEqual(5, exports.Test(4, 5, 0));
    }

    /// <summary>
    /// Picks one of two v128 operands and returns a byte from the result.
    /// </summary>
    public abstract class V128SelectExport
    {
        /// <summary>Selects the first operand when <paramref name="condition"/> is non-zero, then returns the byte at <paramref name="offset"/>.</summary>
        public abstract int Test(int condition, int offset);
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Select"/> instruction with <see cref="WebAssemblyValueType.V128"/> input.
    /// Regression test: the untyped <c>select</c> over v128 operands (emitted by Emscripten SIMD codegen) previously
    /// threw <see cref="ModuleLoadException"/> because <see cref="Select"/> had no v128 arm.
    /// </summary>
    [TestMethod]
    public void Select_Compiled_V128()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(V128SelectExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0), // store address
                new V128Const { Value = [0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA] },
                new V128Const { Value = [0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB] },
                new LocalGet(0), // condition
                new Select(),
                new V128Store(),
                new LocalGet(1), // byte offset to read back
                new Int32Load8Unsigned(),
                new End(),
            ],
        });

        using var compiled = module.ToInstance<V128SelectExport>();
        var exports = compiled.Exports;
        Assert.AreEqual(0xAA, exports.Test(1, 0)); // non-zero condition -> first operand
        Assert.AreEqual(0xBB, exports.Test(0, 0)); // zero condition -> second operand
        Assert.AreEqual(0xAA, exports.Test(7, 15)); // any non-zero, last byte
        Assert.AreEqual(0xBB, exports.Test(0, 9));
    }
}
