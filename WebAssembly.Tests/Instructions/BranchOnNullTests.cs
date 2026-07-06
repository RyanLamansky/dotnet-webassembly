using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="BranchOnNull"/> instruction.
/// </summary>
[TestClass]
public class BranchOnNullTests
{
    /// <summary>
    /// A null reference takes the branch, carrying the block's result value; the non-null fall-through is not reached.
    /// </summary>
    [TestMethod]
    public void BranchOnNull_NullBranches()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        // block (result i32) { i32.const 1; ref.null func; br_on_null 0; drop; drop; i32.const 2 }
        module.Codes.Add(new FunctionBody(
            new Block(BlockType.Int32),
                new Int32Constant(1),
                new RefNull(WebAssemblyHeapType.Func),
                new BranchOnNull(0),
                new Drop(),          // (unreachable) drop the non-null reference
                new Drop(),          // drop the i32
                new Int32Constant(2),
            new End(),
            new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(1, exports.Test());
    }

    /// <summary>
    /// A non-null reference falls through, remaining on the stack (branch not taken).
    /// </summary>
    [TestMethod]
    public void BranchOnNull_NonNullFallsThrough()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        // block (result i32) { i32.const 1; ref.func 0; br_on_null 0; drop; drop; i32.const 2 }
        module.Codes.Add(new FunctionBody(
            new Block(BlockType.Int32),
                new Int32Constant(1),
                new RefFunc(0),
                new BranchOnNull(0),
                new Drop(),          // drop the non-null reference (fall-through)
                new Drop(),          // drop the i32
                new Int32Constant(2),
            new End(),
            new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(2, exports.Test());
    }

    /// <summary>Tests <see cref="BranchOnNull"/> equality and round-tripping of the label index.</summary>
    [TestMethod]
    public void BranchOnNull_Equals()
    {
        var a = new BranchOnNull { Index = 2 };
        Assert.AreEqual(2u, a.Index);
        Assert.AreEqual(new BranchOnNull(2), a);
        Assert.AreNotEqual(new BranchOnNull(3), a);
        Assert.AreEqual(new BranchOnNull(2).GetHashCode(), a.GetHashCode());
    }
}
