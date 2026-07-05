using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ReturnCall"/> instruction.
/// </summary>
[TestClass]
public class ReturnCallTests
{
    // Two functions that tail-call each other, counting a parameter down to zero and then returning 42.
    // Both share the exported entry's (i32) -> i32 signature.
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

        // func0: if (n == 0) return 42; else return_call func1(n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new ReturnCall(1),
            new End(),
            new End()));

        // func1: if (n == 0) return 42; else return_call func0(n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new ReturnCall(0),
            new End(),
            new End()));

        return module;
    }

    /// <summary>
    /// Tests that a shallow chain of <see cref="ReturnCall"/> tail calls produces the correct result.
    /// </summary>
    [TestMethod]
    public void ReturnCall_Compiled()
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
    public void ReturnCall_DeepRecursion_DoesNotOverflow()
    {
        var exports = CreateMutualRecursionModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(5_000_000));
    }

    /// <summary>
    /// Tests the <see cref="ReturnCall.Equals(Instruction)"/> and <see cref="ReturnCall.GetHashCode()"/> methods.
    /// </summary>
    [TestMethod]
    public void ReturnCall_Equals()
    {
        TestUtility.CreateInstances<ReturnCall>(out var a, out var b);

        a.Index = 1;
        TestUtility.AreNotEqual(a, b);
        b.Index = 1;
        TestUtility.AreEqual(a, b);
    }
}
