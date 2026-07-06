using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8RelaxedQ15MulrSigned"/> instruction.</summary>
[TestClass]
public class Int16x8RelaxedQ15MulrSignedTests
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
                new V128Const { Value = RelaxedSimdTestHelper.Bytes((short)16384, (short)-32768) },
                new V128Const { Value = RelaxedSimdTestHelper.Bytes((short)16384, (short)-32768) },
                new Int16x8RelaxedQ15MulrSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load16Signed(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic (q15mulr_sat_s) result, including saturation.</summary>
    [TestMethod]
    public void Int16x8RelaxedQ15MulrSigned_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(8192, exports.GetShort(0));
        Assert.AreEqual(32767, exports.GetShort(2), "(-32768)^2 rounded saturates to i16 max");
    }

    /// <summary>Verifies the <see cref="Int16x8RelaxedQ15MulrSigned"/> opcode and equality.</summary>
    [TestMethod]
    public void Int16x8RelaxedQ15MulrSigned_OpCodeAndEquality()
    {
        var instruction = new Int16x8RelaxedQ15MulrSigned();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int16x8RelaxedQ15MulrSigned, instruction.SimdOpCode);
        Assert.AreEqual(new Int16x8RelaxedQ15MulrSigned(), instruction);
        Assert.AreEqual(new Int16x8RelaxedQ15MulrSigned().GetHashCode(), instruction.GetHashCode());
    }
}
