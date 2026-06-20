using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly;

namespace WebAssembly.Instructions;

/// <summary>
/// Exports for <see cref="IfTests.If_MultiValue_NoElse_WithInnerBranch_Compiles"/>.
/// </summary>
public abstract class IfMultiValueExports
{
    /// <summary>
    /// Returns <paramref name="a"/> when <paramref name="outerCondition"/> is 0 (the if's pass-through else) or when
    /// <paramref name="innerCondition"/> is non-zero (an inner branch to the block), otherwise <c>a + 1</c>.
    /// </summary>
    public abstract int Run(int a, int outerCondition, int innerCondition);
}

/// <summary>
/// Tests the <see cref="If"/> instruction.
/// </summary>
[TestClass]
public class IfTests
{
    /// <summary>
    /// A multi-value <c>if</c> with no <c>else</c> whose parameters equal its results, containing an inner
    /// <c>br_if</c> back to the block. The branch path, the implicit-else (false) path, and the then fall-through
    /// path must all reach the end label with a consistent IL stack.
    /// </summary>
    [TestMethod]
    public void If_MultiValue_NoElse_WithInnerBranch_Compiles()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType // 0: the exported function
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Types.Add(new WebAssemblyType // 1: the if block signature (param i32 i32) -> (result i32 i32)
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(IfMultiValueExports.Run) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),                // a   \ the two if-block parameters
                new LocalGet(0),                // a   /
                new LocalGet(1),                // outer condition (consumed by the if)
                new If { TypeIndex = 1 },
                    new LocalGet(2),            // inner condition
                    new BranchIf(0),            // carries [a, a] to the block end when non-zero
                    new Int32Constant(1),
                    new Int32Add(),             // fall-through results: [a, a + 1]
                new End(),                      // end if -> [i32, i32]
                new Int32Add(),                 // combine the two results
                new End(),                      // end function
            ],
        });

        var exports = module.ToInstance<IfMultiValueExports>().Exports;

        Assert.AreEqual(10, exports.Run(5, 0, 0)); // outer false: implicit else passes the parameters through (a + a)
        Assert.AreEqual(10, exports.Run(5, 1, 1)); // inner branch taken (a + a)
        Assert.AreEqual(11, exports.Run(5, 1, 0)); // then fall-through (a + (a + 1))
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="If"/> instruction.
    /// </summary>
    [TestMethod]
    public void If_Compiled()
    {
        var exports = CompilerTestBase<int>.CreateInstance(
            new LocalGet(0),
            new If(),
            new Int32Constant(3),
            new Return(),
            new End(),
            new Int32Constant(2),
            new End());

        Assert.AreEqual(2, exports.Test(0));
        Assert.AreEqual(3, exports.Test(1));
    }
}
