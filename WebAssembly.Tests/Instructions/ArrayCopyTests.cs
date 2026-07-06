using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ArrayCopy"/> instruction.
/// </summary>
[TestClass]
public class ArrayCopyTests
{
    /// <summary>A one-argument, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="ArrayCopy"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void ArrayCopy_OpCodeAndEquality()
    {
        var instruction = new ArrayCopy { DestinationType = 0, SourceType = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.ArrayCopy, instruction.GCOpCode);
        Assert.AreEqual(0u, instruction.DestinationType);
        Assert.AreEqual(1u, instruction.SourceType);
        Assert.AreEqual(new ArrayCopy { DestinationType = 0, SourceType = 1 }, instruction);
        Assert.AreNotEqual(new ArrayCopy { DestinationType = 0, SourceType = 2 }, instruction);
        Assert.AreNotEqual(new ArrayCopy { DestinationType = 1, SourceType = 1 }, instruction);
        Assert.AreEqual(new ArrayCopy { DestinationType = 0, SourceType = 1 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>array.copy</c> moves elements from a source array into a destination array.</summary>
    [TestMethod]
    public void ArrayCopy_CopiesElements()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody
            {
                // Local 1 is the zero-initialized destination; local 2 is the value-filled source.
                Locals = [new Local { Count = 2, RefType = new ReferenceType(0u, true) }],
                Code =
                [
                    new Int32Constant(4),
                    new ArrayNewDefault { Type = 0 },
                    new LocalSet(1),
                    new LocalGet(0),
                    new Int32Constant(4),
                    new ArrayNew { Type = 0 },
                    new LocalSet(2),
                    new LocalGet(1),
                    new Int32Constant(0),
                    new LocalGet(2),
                    new Int32Constant(0),
                    new Int32Constant(4),
                    new ArrayCopy { DestinationType = 0, SourceType = 0 },
                    new LocalGet(1),
                    new Int32Constant(2),
                    new ArrayGet { Type = 0 },
                    new End(),
                ],
            });

        Assert.AreEqual(51, module.ToInstance<Sample>().Exports.Test(51));
    }
}
