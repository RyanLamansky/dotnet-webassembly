using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructNew"/> instruction.
/// </summary>
[TestClass]
public class StructNewTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="StructNew"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void StructNew_OpCodeAndEquality()
    {
        var instruction = new StructNew { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructNew, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new StructNew { Type = 3 }, instruction);
        Assert.AreNotEqual(new StructNew { Type = 4 }, instruction);
        Assert.AreEqual(new StructNew { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>struct.new</c> allocates a struct from an operand and <c>struct.get</c> reads it back.</summary>
    [TestMethod]
    public void StructNew_RoundTripsAField()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));
        module.Codes.Add(new FunctionBody(
            new LocalGet(0),
            new StructNew { Type = 0 },
            new StructGet { Type = 0, Field = 0 },
            new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(42, exports.Test(42));
        Assert.AreEqual(-7, exports.Test(-7));
    }
}
