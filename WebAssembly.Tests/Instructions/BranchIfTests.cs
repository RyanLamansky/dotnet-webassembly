using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="BranchIf"/> instruction.
/// </summary>
[TestClass]
public class BranchIfTests
{
    /// <summary>
    /// Tests compilation and execution of the <see cref="BranchIf"/> instruction.
    /// </summary>
    [TestMethod]
    public void BranchIf_Compiled()
    {
        var exports = CompilerTestBase<int>.CreateInstance(
            new Block(BlockType.Empty),
            new LocalGet(0),
            new BranchIf(0),
            new Int32Constant(2),
            new Return(),
            new End(),
            new Int32Constant(1),
            new End());

        Assert.AreEqual(2, exports.Test(0));
        Assert.AreEqual(1, exports.Test(1));
    }

    /// <summary>
    /// Tests compilation of the <see cref="BranchIf"/> and <see cref="Loop"/> instructions that yields a value with no way for it to end.
    /// </summary>
    [TestMethod]
    public void BranchIf_LoopInfiniteWithValue()
    {
        _ = AssemblyBuilder.CreateInstance<object>("Test",
            WebAssemblyValueType.Int32,
            new Loop(BlockType.Int32),
            new Int32Constant(3),
            new Int32Constant(1),
            new BranchIf(),
            new End(),
            new End());
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="BranchIf"/> and <see cref="Loop"/> instructions that yields a value.
    /// </summary>
    [TestMethod]
    public void BranchIf_LoopBreakWithValue()
    {
        var exports = AssemblyBuilder.CreateInstance<dynamic>("Test",
            WebAssemblyValueType.Int32,
            new Loop(BlockType.Int32),
                new Int32Constant(3),
                new Int32Constant(0),
                new BranchIf(),
                new End(),
                new End());

        Assert.AreEqual<int>(3, exports.Test());
    }

    /// <summary>
    /// Tests that unreachable <see cref="BranchIf"/> still preserves the not-taken stack effect.
    /// </summary>
    [TestMethod]
    public void BranchIf_UnreachablePreservesNotTakenStackEffect()
    {
        var exception = Assert.ThrowsException<StackTypeInvalidException>(() =>
            AssemblyBuilder.CreateInstance<dynamic>("Test",
                WebAssemblyValueType.Int64,
                new Unreachable(),
                new BranchIf(0),
                new Int64ExtendInt32Unsigned(),
                new End()).Test());

        Assert.AreEqual(WebAssemblyValueType.Int32, exception.Expected);
        Assert.AreEqual(WebAssemblyValueType.Int64, exception.Actual);
    }
}
