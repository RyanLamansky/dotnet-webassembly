using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Tests for multi-value (WASM 2.0) function return support.
/// </summary>
[TestClass]
public class MultiValueTests
{
    /// <summary>Export type returning two i32 values.</summary>
    public abstract class TwoInt32Returns
    {
        /// <summary>Test method.</summary>
        public abstract (int, int) Test();
    }

    /// <summary>Export type returning three values of mixed types.</summary>
    public abstract class ThreeMixedReturns
    {
        /// <summary>Test method.</summary>
        public abstract (int, long, float) Test();
    }

    /// <summary>Export type returning two i32 values, taking one parameter.</summary>
    public abstract class TwoInt32ReturnsWithParam
    {
        /// <summary>Test method.</summary>
        public abstract (int, int) Test(int x);
    }

    /// <summary>Export type returning eight i32 values.</summary>
    public abstract class EightInt32Returns
    {
        /// <summary>Test method.</summary>
        public abstract (int, int, int, int, int, int, int, int) Test();
    }

    /// <summary>Export type returning a single i32 sum.</summary>
    public abstract class SumExport
    {
        /// <summary>Computes a sum.</summary>
        public abstract int Sum();
    }

    /// <summary>
    /// A function returning two i32 values should produce both on the caller's stack.
    /// </summary>
    [TestMethod]
    public void MultiValue_TwoInt32Returns()
    {
        var exports = AssemblyBuilder.CreateInstance<TwoInt32Returns>(
            nameof(TwoInt32Returns.Test),
            [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            [],
            new Int32Constant(42),
            new Int32Constant(99),
            new End());

        var (a, b) = exports.Test();
        Assert.AreEqual(42, a);
        Assert.AreEqual(99, b);
    }

    /// <summary>
    /// A function with three returns of mixed numeric types.
    /// </summary>
    [TestMethod]
    public void MultiValue_ThreeMixedTypes()
    {
        var exports = AssemblyBuilder.CreateInstance<ThreeMixedReturns>(
            nameof(ThreeMixedReturns.Test),
            [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64, WebAssemblyValueType.Float32],
            [],
            new Int32Constant(1),
            new Int64Constant(2),
            new Float32Constant(3.14f),
            new End());

        var (i, l, f) = exports.Test();
        Assert.AreEqual(1, i);
        Assert.AreEqual(2L, l);
        Assert.AreEqual(3.14f, f, 0.0001f);
    }

    /// <summary>
    /// A multi-value function using explicit Return instruction.
    /// </summary>
    [TestMethod]
    public void MultiValue_ExplicitReturn()
    {
        var exports = AssemblyBuilder.CreateInstance<TwoInt32Returns>(
            nameof(TwoInt32Returns.Test),
            [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            [],
            new Int32Constant(10),
            new Int32Constant(20),
            new Return(),
            new End());

        var (a, b) = exports.Test();
        Assert.AreEqual(10, a);
        Assert.AreEqual(20, b);
    }

    /// <summary>
    /// A multi-value function accepting a parameter.
    /// </summary>
    [TestMethod]
    public void MultiValue_WithParameter()
    {
        var exports = AssemblyBuilder.CreateInstance<TwoInt32ReturnsWithParam>(
            nameof(TwoInt32ReturnsWithParam.Test),
            [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            [WebAssemblyValueType.Int32],
            new LocalGet(0),
            new LocalGet(0),
            new Int32Constant(1),
            new Int32Add(),
            new End());

        var (a, b) = exports.Test(5);
        Assert.AreEqual(5, a);
        Assert.AreEqual(6, b);
    }

    /// <summary>
    /// Calling a multi-value function from another function pushes multiple values onto the caller's stack.
    /// </summary>
    [TestMethod]
    public void MultiValue_CallMultiValueFunction()
    {
        var module = new Module();

        // type 0: () -> (i32, i32)
        module.Types.Add(new WebAssemblyType
        {
            Returns = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
        });
        // type 1: () -> i32
        module.Types.Add(new WebAssemblyType
        {
            Returns = [WebAssemblyValueType.Int32],
        });

        module.Functions.Add(new Function { Type = 0 }); // func 0: pair
        module.Functions.Add(new Function { Type = 1 }); // func 1: sum

        module.Exports.Add(new Export { Name = nameof(SumExport.Sum), Index = 1 });

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(7),
                new Int32Constant(8),
                new End()
            ]
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Call(0),   // pushes 7 and 8 onto stack
                new Int32Add(),
                new End()
            ]
        });

        var instance = module.ToInstance<SumExport>();
        Assert.AreEqual(15, instance.Exports.Sum());
    }

    /// <summary>
    /// A multi-value function returning more than seven values uses nested ValueTuple packing.
    /// </summary>
    [TestMethod]
    public void MultiValue_EightInt32Returns()
    {
        var exports = AssemblyBuilder.CreateInstance<EightInt32Returns>(
            nameof(EightInt32Returns.Test),
            [
                WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32,
                WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32
            ],
            [],
            new Int32Constant(1),
            new Int32Constant(2),
            new Int32Constant(3),
            new Int32Constant(4),
            new Int32Constant(5),
            new Int32Constant(6),
            new Int32Constant(7),
            new Int32Constant(8),
            new End());

        var result = exports.Test();
        Assert.AreEqual((1, 2, 3, 4, 5, 6, 7, 8), result);
    }
}
