using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ReturnCallIndirect"/> instruction.
/// </summary>
[TestClass]
public class ReturnCallIndirectTests
{
    // Two functions that tail-call each other indirectly through a table, counting a parameter down to zero
    // and then returning 42. Table slot 0 holds func0, slot 1 holds func1.
    private static Module CreateMutualRecursionModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function(0));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase<int>.Test), 0));

        // func0: if (n == 0) return 42; else return_call_indirect table[1](n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new Int32Constant(1), new ReturnCallIndirect(0),
            new End(),
            new End()));

        // func1: if (n == 0) return 42; else return_call_indirect table[0](n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new Int32Constant(0), new ReturnCallIndirect(0),
            new End(),
            new End()));

        module.Tables.Add(new Table(2, 2));
        module.Elements.Add(new Element(0, 0, 1));

        return module;
    }

    /// <summary>
    /// Tests that a shallow chain of <see cref="ReturnCallIndirect"/> tail calls produces the correct result.
    /// </summary>
    [TestMethod]
    public void ReturnCallIndirect_Compiled()
    {
        var exports = CreateMutualRecursionModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(0));
        Assert.AreEqual(42, exports.Test(1));
        Assert.AreEqual(42, exports.Test(10));
    }

    /// <summary>
    /// Proves the CLR performs a real tail call: five million mutual-recursion frames would overflow the stack
    /// if the <c>tail.</c> prefix were not honored.
    /// </summary>
    [TestMethod]
    public void ReturnCallIndirect_DeepRecursion_DoesNotOverflow()
    {
        var exports = CreateMutualRecursionModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(5_000_000));
    }

    /// <summary>
    /// Tests the <see cref="ReturnCallIndirect.Equals(Instruction)"/> and <see cref="ReturnCallIndirect.GetHashCode()"/> methods.
    /// </summary>
    [TestMethod]
    public void ReturnCallIndirect_Equals()
    {
        TestUtility.CreateInstances<ReturnCallIndirect>(out var a, out var b);

        a.Type = 1;
        TestUtility.AreNotEqual(a, b);
        b.Type = 1;
        TestUtility.AreEqual(a, b);

        a.Table = 1;
        TestUtility.AreNotEqual(a, b);
        b.Table = 1;
        TestUtility.AreEqual(a, b);
    }
}
