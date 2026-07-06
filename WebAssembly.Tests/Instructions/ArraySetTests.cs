using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArraySet"/> instruction.
/// </summary>
[TestClass]
public class ArraySetTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="ArraySet"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArraySet_OpCodeAndEquality()
    {
        var instruction = new ArraySet { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArraySet, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArraySet { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArraySet { Type = 4 }, instruction);
        Assert.AreEqual(new ArraySet { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.set</c> writes an element that <c>array.get</c> reads back.</summary>
    [TestMethod]
    public void ArraySet_WritesElement()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody
            {
                // Local 1 (a nullable ref to the array) holds the fresh, zero-initialized array.
                Locals = [new Local { Count = 1, RefType = new ReferenceType(0u, true) }],
                Code =
                [
                    new Int32Constant(3),
                    new ArrayNewDefault { Type = 0 },
                    new LocalSet(1),
                    new LocalGet(1),
                    new Int32Constant(1),
                    new LocalGet(0),
                    new ArraySet { Type = 0 },
                    new LocalGet(1),
                    new Int32Constant(1),
                    new ArrayGet { Type = 0 },
                    new End(),
                ],
            });

        Assert.AreEqual(77, module.ToInstance<Sample>().Exports.Test(77));
    }
}
