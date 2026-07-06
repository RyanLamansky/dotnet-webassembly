using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayFill"/> instruction.
/// </summary>
[TestClass]
public class ArrayFillTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="ArrayFill"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void ArrayFill_OpCodeAndEquality()
    {
        var instruction = new ArrayFill { Type = 3 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayFill, instruction.GCOpCode);
        Assert.AreEqual(3u, instruction.Type);
        Assert.AreEqual(new ArrayFill { Type = 3 }, instruction);
        Assert.AreNotEqual(new ArrayFill { Type = 4 }, instruction);
        Assert.AreEqual(new ArrayFill { Type = 3 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.fill</c> broadcasts a value across a range that <c>array.get</c> reads back.</summary>
    [TestMethod]
    public void ArrayFill_FillsRange()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody
            {
                // Local 1 (a nullable ref to the array) holds the fresh, zero-initialized array.
                Locals = [new Local { Count = 1, RefType = new ReferenceType(0u, true) }],
                Code =
                [
                    new Int32Constant(4),
                    new ArrayNewDefault { Type = 0 },
                    new LocalSet(1),
                    new LocalGet(1),
                    new Int32Constant(0),
                    new LocalGet(0),
                    new Int32Constant(4),
                    new ArrayFill { Type = 0 },
                    new LocalGet(1),
                    new Int32Constant(2),
                    new ArrayGet { Type = 0 },
                    new End(),
                ],
            });

        Assert.AreEqual(63, module.ToInstance<Sample>().Exports.Test(63));
    }
}
