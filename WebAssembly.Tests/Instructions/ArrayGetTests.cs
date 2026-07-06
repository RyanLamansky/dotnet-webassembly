using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayGet"/> instruction.
/// </summary>
[TestClass]
public class ArrayGetTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int index);
    }

    /// <summary>Tests <see cref="ArrayGet"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayGet_OpCodeAndEquality()
    {
        var instruction = new ArrayGet { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayGet, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayGet { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayGet { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayGet { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.get</c> reads back the element an <c>array.new</c> broadcast to every slot.</summary>
    [TestMethod]
    public void ArrayGet_ReadsElement()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody(
                new Int32Constant(42),
                new Int32Constant(4),
                new ArrayNew { Type = 0 },
                new LocalGet(0),
                new ArrayGet { Type = 0 },
                new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(42, exports.Test(0));
        Assert.AreEqual(42, exports.Test(3));
    }
}
