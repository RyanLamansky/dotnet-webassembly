using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayNew"/> instruction.
/// </summary>
[TestClass]
public class ArrayNewTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="ArrayNew"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayNew_OpCodeAndEquality()
    {
        var instruction = new ArrayNew { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayNew, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayNew { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayNew { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayNew { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.new</c> allocates an array from an operand and <c>array.get</c> reads an element back.</summary>
    [TestMethod]
    public void ArrayNew_RoundTripsAnElement()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Array, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));
        module.Codes.Add(new FunctionBody(
            new LocalGet(0),
            new Int32Constant(4),
            new ArrayNew { Type = 0 },
            new Int32Constant(2),
            new ArrayGet { Type = 0 },
            new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(42, exports.Test(42));
        Assert.AreEqual(-7, exports.Test(-7));
    }
}
