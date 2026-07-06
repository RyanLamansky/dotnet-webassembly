using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16RelaxedSwizzle"/> instruction.</summary>
[TestClass]
public class Int8x16RelaxedSwizzleTests
{
    /// <summary>Export for Int8x16RelaxedSwizzle test.</summary>
    public abstract class Export
    {
        /// <summary>Returns the swizzle result byte at the given offset.</summary>
        public abstract int GetByte(int offset);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Export.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160] },
                // indices: lane 0 -> a[3]=40, lane 1 -> out of range (high bit) -> 0, others -> a[0]=10
                new V128Const { Value = [3, 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int8x16RelaxedSwizzle(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16RelaxedSwizzle produces the deterministic swizzle result.</summary>
    [TestMethod]
    public void Int8x16RelaxedSwizzle_IsCorrect()
    {
        var exports = BuildModule().ToInstance<Export>().Exports;
        Assert.AreEqual(40, exports.GetByte(0));
        Assert.AreEqual(0, exports.GetByte(1), "index with high bit set selects 0");
        Assert.AreEqual(10, exports.GetByte(2));
    }

    /// <summary>Verifies the <see cref="Int8x16RelaxedSwizzle"/> opcode and equality.</summary>
    [TestMethod]
    public void Int8x16RelaxedSwizzle_OpCodeAndEquality()
    {
        var instruction = new Int8x16RelaxedSwizzle();
        Assert.AreEqual(OpCode.SimdOperationPrefix, instruction.OpCode);
        Assert.AreEqual(SimdOpCode.Int8x16RelaxedSwizzle, instruction.SimdOpCode);
        Assert.AreEqual(new Int8x16RelaxedSwizzle(), instruction);
        Assert.AreEqual(new Int8x16RelaxedSwizzle().GetHashCode(), instruction.GetHashCode());
    }
}
