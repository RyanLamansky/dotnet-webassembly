using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="I31GetUnsigned"/> instruction.
/// </summary>
[TestClass]
public class I31GetUnsignedTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="I31GetUnsigned"/> opcode and equality.</summary>
    [TestMethod]
    public void I31GetUnsigned_OpCodeAndEquality()
    {
        var instruction = new I31GetUnsigned();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.I31GetUnsigned, instruction.GCOpCode);
        Assert.AreEqual(new I31GetUnsigned(), instruction);
        Assert.AreEqual(new I31GetUnsigned().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>i31.get_u</c> zero-extends the 31-bit payload of a <c>ref.i31</c>.</summary>
    [TestMethod]
    public void I31GetUnsigned_ZeroExtends()
    {
        var module = GCTestHelper.FunctionModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody(
                new LocalGet(0),
                new RefI31(),
                new I31GetUnsigned(),
                new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(100, exports.Test(100));
        // ref.i31 keeps the low 31 bits; get_u reads them without sign extension.
        Assert.AreEqual(0x7FFFFFFF, exports.Test(-1));
    }
}
