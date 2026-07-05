using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="BranchOnNonNull"/> instruction.
/// </summary>
[TestClass]
public class BranchOnNonNullTests
{
    /// <summary>
    /// A null reference falls through (the reference is dropped) rather than taking the branch.
    /// </summary>
    [TestMethod]
    public void BranchOnNonNull_NullFallsThrough()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        // block (result funcref) { ref.null func; br_on_non_null 0; ref.null func }  then ref.is_null -> 1
        module.Codes.Add(new FunctionBody(
            new Block(BlockType.FuncRef),
                new RefNull(WebAssemblyHeapType.Func),
                new BranchOnNonNull(0),
                new RefNull(WebAssemblyHeapType.Func), // fall-through supplies the block's funcref result
            new End(),
            new RefIsNull(),
            new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(1, exports.Test());
    }

    /// <summary>
    /// A non-null reference takes the branch, carrying the reference as the block's result.
    /// </summary>
    [TestMethod]
    public void BranchOnNonNull_NonNullBranches()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        // block (result funcref) { ref.func 0; br_on_non_null 0; ref.null func }  then ref.is_null -> 0
        module.Codes.Add(new FunctionBody(
            new Block(BlockType.FuncRef),
                new RefFunc(0),
                new BranchOnNonNull(0),
                new RefNull(WebAssemblyHeapType.Func),
            new End(),
            new RefIsNull(),
            new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(0, exports.Test());
    }

    /// <summary>Tests <see cref="BranchOnNonNull"/> equality and round-tripping of the label index.</summary>
    [TestMethod]
    public void BranchOnNonNull_Equals()
    {
        var a = new BranchOnNonNull { Index = 2 };
        Assert.AreEqual(2u, a.Index);
        Assert.AreEqual(new BranchOnNonNull(2), a);
        Assert.AreNotEqual(new BranchOnNonNull(3), a);
        Assert.AreEqual(new BranchOnNonNull(2).GetHashCode(), a.GetHashCode());
    }
}
