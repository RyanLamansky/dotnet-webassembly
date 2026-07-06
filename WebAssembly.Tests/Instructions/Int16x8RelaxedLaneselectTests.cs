using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8RelaxedLaneselect"/> instruction.</summary>
[TestClass]
public class Int16x8RelaxedLaneselectTests
{
    /// <summary>Export for the test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the result byte at the given offset.</summary>
        public abstract int GetByte(int offset);
    }

    private static Export Build()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                // a = all 0xFF, b = all 0x00
                new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF] },
                new V128Const { Value = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
                // mask: first 8 bytes select a, last 8 select b
                new V128Const { Value = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00] },
                new Int16x8RelaxedLaneselect(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module.ToInstance<Export>().Exports;
    }

    /// <summary>Verifies the deterministic (bitselect) result.</summary>
    [TestMethod]
    public void Int16x8RelaxedLaneselect_IsCorrect()
    {
        var exports = Build();
        Assert.AreEqual(0xFF, exports.GetByte(0), "masked lanes select a");
        Assert.AreEqual(0x00, exports.GetByte(8), "unmasked lanes select b");
    }

    /// <summary>Verifies the <see cref="Int16x8RelaxedLaneselect"/> opcode and equality.</summary>
    [TestMethod]
    public void Int16x8RelaxedLaneselect_OpCodeAndEquality()
    {
        var instruction = new Int16x8RelaxedLaneselect();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int16x8RelaxedLaneselect, instruction.SimdOpCode);
        Assert.AreEqual(new Int16x8RelaxedLaneselect(), instruction);
        Assert.AreEqual(new Int16x8RelaxedLaneselect().GetHashCode(), instruction.GetHashCode());
    }
}
