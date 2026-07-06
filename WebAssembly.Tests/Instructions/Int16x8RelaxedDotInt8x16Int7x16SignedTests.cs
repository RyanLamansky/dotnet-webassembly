using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8RelaxedDotInt8x16Int7x16Signed"/> instruction.</summary>
[TestClass]
public class Int16x8RelaxedDotInt8x16Int7x16SignedTests
{
    /// <summary>Export for the test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the sign-extended i16 lane at the given byte offset of the result.</summary>
        public abstract int GetShort(int offset);
    }

    private static Export Build()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetShort) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(new sbyte[] { 3, 4, -128, -128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes(new sbyte[] { 5, 6, -128, -128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) },
                new Int16x8RelaxedDotInt8x16Int7x16Signed(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load16Signed(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic signed dot product with saturating pairwise add.</summary>
    [TestMethod]
    public void Int16x8RelaxedDotInt8x16Int7x16Signed_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(39, exports.GetShort(0), "3*5 + 4*6");
        Assert.AreEqual(32767, exports.GetShort(2), "(-128*-128)*2 = 32768 saturates to i16 max");
        Assert.AreEqual(0, exports.GetShort(4));
    }

    /// <summary>Verifies the <see cref="Int16x8RelaxedDotInt8x16Int7x16Signed"/> opcode and equality.</summary>
    [TestMethod]
    public void Int16x8RelaxedDotInt8x16Int7x16Signed_OpCodeAndEquality()
    {
        var instruction = new Int16x8RelaxedDotInt8x16Int7x16Signed();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int16x8RelaxedDotInt8x16Int7x16Signed, instruction.SimdOpCode);
        Assert.AreEqual(new Int16x8RelaxedDotInt8x16Int7x16Signed(), instruction);
        Assert.AreEqual(new Int16x8RelaxedDotInt8x16Int7x16Signed().GetHashCode(), instruction.GetHashCode());
    }
}
