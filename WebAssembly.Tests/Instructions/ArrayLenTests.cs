using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayLen"/> instruction.
/// </summary>
[TestClass]
public class ArrayLenTests
{
    /// <summary>A parameterless, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test();
    }

    /// <summary>Tests <see cref="ArrayLen"/> opcode and equality.</summary>
    [TestMethod]
    public void ArrayLen_OpCodeAndEquality()
    {
        var instruction = new ArrayLen();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayLen, instruction.GCOpCode);
        Assert.AreEqual(new ArrayLen(), instruction);
        Assert.AreEqual(new ArrayLen().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.len</c> reports the element count of a freshly allocated array.</summary>
    [TestMethod]
    public void ArrayLen_ReportsElementCount()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody(
                new Int32Constant(0),
                new Int32Constant(5),
                new ArrayNew { Type = 0 },
                new ArrayLen(),
                new End()));

        Assert.AreEqual(5, module.ToInstance<Sample>().Exports.Test());
    }
}
