using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2RelaxedMultiplyAdd"/> instruction.</summary>
[TestClass]
public class Float64x2RelaxedMultiplyAddTests
{
    /// <summary>Export for the test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the f64 lane at the given byte offset of the result.</summary>
        public abstract double GetDouble(int offset);
    }

    private static Export Build()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Float64] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetDouble) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(2d, 3d) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(10d, 10d) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(1d, 2d) },
                new Float64x2RelaxedMultiplyAdd(),
                new V128Store(),
                new LocalGet(0),
                new Float64Load(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies a*b + c per lane.</summary>
    [TestMethod]
    public void Float64x2RelaxedMultiplyAdd_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(21d, exports.GetDouble(0));
        Assert.AreEqual(32d, exports.GetDouble(8));
    }
}
