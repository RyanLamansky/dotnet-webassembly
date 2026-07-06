using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4RelaxedMax"/> instruction.</summary>
[TestClass]
public class Float32x4RelaxedMaxTests
{
    /// <summary>Export for the test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the f32 lane at the given byte offset of the result.</summary>
        public abstract float GetFloat(int offset);
    }

    private static Export Build()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Float32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetFloat) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(1f, 5f, 3f, float.NaN) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(2f, 4f, 7f, 6f) },
                new Float32x4RelaxedMax(),
                new V128Store(),
                new LocalGet(0),
                new Float32Load(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic (f32x4.max) result, including NaN propagation.</summary>
    [TestMethod]
    public void Float32x4RelaxedMax_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(2f, exports.GetFloat(0));
        Assert.AreEqual(5f, exports.GetFloat(4));
        Assert.AreEqual(7f, exports.GetFloat(8));
        Assert.IsTrue(float.IsNaN(exports.GetFloat(12)), "NaN propagates");
    }
}
