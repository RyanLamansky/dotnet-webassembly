using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefEqual"/> instruction.
/// </summary>
[TestClass]
public class RefEqualTests
{
    /// <summary>A parameterless, i32-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test();
    }

    /// <summary>Tests <see cref="RefEqual"/> opcode and equality.</summary>
    [TestMethod]
    public void RefEqual_OpCodeAndEquality()
    {
        var instruction = new RefEqual();
        Assert.AreEqual(OpCode.RefEqual, instruction.OpCode);
        Assert.AreEqual(new RefEqual(), instruction);
        Assert.AreEqual(new RefEqual().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>ref.eq</c> reports 1 when both operands are the same reference.</summary>
    [TestMethod]
    public void RefEqual_SameReferenceIsEqual()
    {
        var module = GCTestHelper.MutableInt32ArrayModule(
            new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] },
            new FunctionBody
            {
                // Local 0 holds one array reference; comparing it to itself must yield 1.
                Locals = [new Local { Count = 1, RefType = new ReferenceType(0u, true) }],
                Code =
                [
                    new Int32Constant(0),
                    new Int32Constant(1),
                    new ArrayNew { Type = 0 },
                    new LocalTee(0),
                    new LocalGet(0),
                    new RefEqual(),
                    new End(),
                ],
            });

        Assert.AreEqual(1, module.ToInstance<Sample>().Exports.Test());
    }
}
