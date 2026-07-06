using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="I31GetSigned"/> instruction.
/// </summary>
[TestClass]
public class I31GetSignedTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="I31GetSigned"/> opcode and equality.</summary>
    [TestMethod]
    public void I31GetSigned_OpCodeAndEquality()
    {
        var instruction = new I31GetSigned();
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.I31GetSigned, instruction.GCOpCode);
        Assert.AreEqual(new I31GetSigned(), instruction);
        Assert.AreEqual(new I31GetSigned().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>i31.get_s</c> sign-extends the 31-bit payload of a <c>ref.i31</c>.</summary>
    [TestMethod]
    public void I31GetSigned_SignExtends()
    {
        var module = GCTestHelper.FunctionModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody(
                new LocalGet(0),
                new RefI31(),
                new I31GetSigned(),
                new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(100, exports.Test(100));
        Assert.AreEqual(-1, exports.Test(-1));
    }
}
