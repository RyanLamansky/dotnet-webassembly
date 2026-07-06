using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayNewFixed"/> instruction.
/// </summary>
[TestClass]
public class ArrayNewFixedTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int index);
    }

    /// <summary>Tests <see cref="ArrayNewFixed"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayNewFixed_OpCodeAndEquality()
    {
        var instruction = new ArrayNewFixed { Type = 3, Size = 2 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayNewFixed, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(2u, instruction.Size);
        Assert.AreEqual(new ArrayNewFixed { Type = 3, Size = 2 }, instruction);
        Assert.AreNotEqual(new ArrayNewFixed { Type = 3, Size = 3 }, instruction);
        Assert.AreNotEqual(new ArrayNewFixed { Type = 4, Size = 2 }, instruction);
        Assert.AreEqual(new ArrayNewFixed { Type = 3, Size = 2 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.new_fixed</c> allocates an array from fixed operands (last on top).</summary>
    [TestMethod]
    public void ArrayNewFixed_StoresOperandsInOrder()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Array, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = false }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));
        module.Codes.Add(new FunctionBody(
            new Int32Constant(10),
            new Int32Constant(20),
            new ArrayNewFixed { Type = 0, Size = 2 },
            new LocalGet(0),
            new ArrayGet { Type = 0 },
            new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(10, exports.Test(0));
        Assert.AreEqual(20, exports.Test(1));
    }
}
