using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4RelaxedTruncFloat64x2UnsignedZero"/> instruction.</summary>
[TestClass]
public class Int32x4RelaxedTruncFloat64x2UnsignedZeroTests
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
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(2.5, 5e9) },
                new Int32x4RelaxedTruncFloat64x2UnsignedZero(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic (trunc_sat_zero) result.</summary>
    [TestMethod]
    public void Int32x4RelaxedTruncFloat64x2UnsignedZero_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(2, exports.GetInt(0));
        Assert.AreEqual(unchecked((int)uint.MaxValue), exports.GetInt(4), "positive overflow saturates");
        Assert.AreEqual(0, exports.GetInt(8), "upper lanes are zero");
        Assert.AreEqual(0, exports.GetInt(12), "upper lanes are zero");
    }

    /// <summary>Verifies the <see cref="Int32x4RelaxedTruncFloat64x2UnsignedZero"/> opcode and equality.</summary>
    [TestMethod]
    public void Int32x4RelaxedTruncFloat64x2UnsignedZero_OpCodeAndEquality()
    {
        var instruction = new Int32x4RelaxedTruncFloat64x2UnsignedZero();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int32x4RelaxedTruncFloat64x2UnsignedZero, instruction.SimdOpCode);
        Assert.AreEqual(new Int32x4RelaxedTruncFloat64x2UnsignedZero(), instruction);
        Assert.AreEqual(new Int32x4RelaxedTruncFloat64x2UnsignedZero().GetHashCode(), instruction.GetHashCode());
    }
}
