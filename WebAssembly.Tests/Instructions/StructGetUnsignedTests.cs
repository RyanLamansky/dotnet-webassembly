using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructGetUnsigned"/> instruction.
/// </summary>
[TestClass]
public class StructGetUnsignedTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="StructGetUnsigned"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void StructGetUnsigned_OpCodeAndEquality()
    {
        var instruction = new StructGetUnsigned { Type = 1, Field = 0 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructGetUnsigned, instruction.GCOpCode);
        Assert.AreEqual(new StructGetUnsigned { Type = 1, Field = 0 }, instruction);
        Assert.AreNotEqual(new StructGetUnsigned { Type = 1, Field = 1 }, instruction);
        Assert.AreEqual(new StructGetUnsigned { Type = 1, Field = 0 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>struct.get_u</c> zero-extends a packed i16 field (0xFFFF stays 65535).</summary>
    [TestMethod]
    public void StructGetUnsigned_ZeroExtendsPackedI16()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { Packed = PackedType.I16, IsMutable = true }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));
        module.Codes.Add(new FunctionBody(
            new LocalGet(0),
            new StructNew { Type = 0 },
            new StructGetUnsigned { Type = 0, Field = 0 },
            new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(65535, exports.Test(0xFFFF));
        Assert.AreEqual(65534, exports.Test(0xFFFE));
        Assert.AreEqual(1, exports.Test(0x10001)); // truncated to 16 bits
    }
}
