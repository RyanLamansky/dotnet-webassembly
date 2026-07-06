using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4RelaxedTruncFloat32x4Signed"/> instruction.</summary>
[TestClass]
public class Int32x4RelaxedTruncFloat32x4SignedTests
{
    /// <summary>Export for the test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the i32 lane at the given byte offset of the result.</summary>
        public abstract int GetInt(int offset);
    }

    private static Export Build()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetInt) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(1.9f, -1.9f, 3e9f, float.NaN) },
                new Int32x4RelaxedTruncFloat32x4Signed(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic (trunc_sat) result.</summary>
    [TestMethod]
    public void Int32x4RelaxedTruncFloat32x4Signed_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(1, exports.GetInt(0));
        Assert.AreEqual(-1, exports.GetInt(4));
        Assert.AreEqual(int.MaxValue, exports.GetInt(8), "positive overflow saturates");
        Assert.AreEqual(0, exports.GetInt(12), "NaN yields 0");
    }
}
