using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4RelaxedDotInt8x16Int7x16AddSigned"/> instruction.</summary>
[TestClass]
public class Int32x4RelaxedDotInt8x16Int7x16AddSignedTests
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
                // Fixture relaxed_dot_product line 41.
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(0, 1, 2, 3) },
                new Int32x4RelaxedDotInt8x16Int7x16AddSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic signed dot product plus accumulator (fixture line 41).</summary>
    [TestMethod]
    public void Int32x4RelaxedDotInt8x16Int7x16AddSigned_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(14, exports.GetInt(0));
        Assert.AreEqual(127, exports.GetInt(4));
        Assert.AreEqual(368, exports.GetInt(8));
        Assert.AreEqual(737, exports.GetInt(12));
    }

    /// <summary>Verifies the <see cref="Int32x4RelaxedDotInt8x16Int7x16AddSigned"/> opcode and equality.</summary>
    [TestMethod]
    public void Int32x4RelaxedDotInt8x16Int7x16AddSigned_OpCodeAndEquality()
    {
        var instruction = new Int32x4RelaxedDotInt8x16Int7x16AddSigned();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int32x4RelaxedDotInt8x16Int7x16AddSigned, instruction.SimdOpCode);
        Assert.AreEqual(new Int32x4RelaxedDotInt8x16Int7x16AddSigned(), instruction);
        Assert.AreEqual(new Int32x4RelaxedDotInt8x16Int7x16AddSigned().GetHashCode(), instruction.GetHashCode());
    }
}
