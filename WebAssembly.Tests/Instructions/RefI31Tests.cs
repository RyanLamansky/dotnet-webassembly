using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefI31"/> instruction.
/// </summary>
[TestClass]
public class RefI31Tests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="RefI31"/> opcode and equality.</summary>
    [TestMethod]
    public void RefI31_OpCodeAndEquality()
    {
        var instruction = new RefI31();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.RefI31, instruction.GCOpCode);
        Assert.AreEqual(new RefI31(), instruction);
        Assert.AreEqual(new RefI31().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>ref.i31</c> boxes an i32 that <c>i31.get_s</c> reads back unchanged.</summary>
    [TestMethod]
    public void RefI31_RoundTripsThroughI31GetSigned()
    {
        var module = GCTestHelper.FunctionModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody(
                new LocalGet(0),
                new RefI31(),
                new I31GetSigned(),
                new End()));

        Assert.AreEqual(100, module.ToInstance<Sample>().Exports.Test(100));
    }
}
