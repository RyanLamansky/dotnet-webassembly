using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ReturnCallRef"/> instruction.
/// </summary>
[TestClass]
public class ReturnCallRefTests
{
    // Two functions that tail-call each other through typed function references (ref.func + return_call_ref),
    // counting a parameter down to zero and then returning 42. A declarative element segment makes both functions
    // referenceable by ref.func.
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

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        declared.Elements.Add(1);
        module.Elements.Add(declared);

        // func0: if (n == 0) return 42; else return_call_ref (ref.func 1)(n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new RefFunc(1), new ReturnCallRef(0),
            new End(),
            new End()));

        // func1: if (n == 0) return 42; else return_call_ref (ref.func 0)(n - 1)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
            new Int32Constant(42),
            new Else(),
            new LocalGet(0), new Int32Constant(1), new Int32Subtract(), new RefFunc(0), new ReturnCallRef(0),
            new End(),
            new End()));

        return module;
    }

    /// <summary>
    /// Tests that a shallow chain of <see cref="ReturnCallRef"/> tail calls produces the correct result.
    /// </summary>
    [TestMethod]
    public void ReturnCallRef_Compiled()
    {
        var exports = CreateMutualRecursionModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(0));
        Assert.AreEqual(42, exports.Test(1));
        Assert.AreEqual(42, exports.Test(10));
    }

    /// <summary>
    /// Proves the CLR performs a real tail call: five million mutual-recursion frames would overflow the stack
    /// if the <c>tail.</c> prefix were not honored (must hold on every framework × config).
    /// </summary>
    [TestMethod]
    public void ReturnCallRef_DeepRecursion_DoesNotOverflow()
    {
        var exports = CreateMutualRecursionModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(5_000_000));
    }

    // As CreateMutualRecursionModule, but each return_call_ref sits inside a try_table (catch_all) nested in a block,
    // so the tail call is emitted from within a protected region — the spill + post-body trampoline path in
    // ReturnCallRef (context.InProtectedRegion). The frame is still replaced, so deep recursion stays constant-stack.
    private static Module CreateMutualRecursionInTryTableModule()
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

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        declared.Elements.Add(1);
        module.Elements.Add(declared);

        static FunctionBody Body(uint otherFunction)
        {
            var tryTable = new TryTable(BlockType.Empty);
            tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0)); // -> the enclosing block

            // if (n == 0) return 42; else { block { try_table (catch_all) { return_call_ref (ref.func other)(n-1) } } 42 }
            return new FunctionBody(
                new LocalGet(0), new Int32EqualZero(), new If(BlockType.Int32),
                    new Int32Constant(42),
                new Else(),
                    new Block(BlockType.Empty),
                        tryTable,
                            new LocalGet(0), new Int32Constant(1), new Int32Subtract(),
                            new RefFunc(otherFunction), new ReturnCallRef(0),
                        new End(), // end try_table
                    new End(), // end block
                    new Int32Constant(42), // unreachable fallback: supplies the else result type
                new End(),
                new End());
        }

        module.Codes.Add(Body(1)); // func0 tail-calls func1
        module.Codes.Add(Body(0)); // func1 tail-calls func0
        return module;
    }

    /// <summary>A <see cref="ReturnCallRef"/> nested in a try_table returns the correct result through the region.</summary>
    [TestMethod]
    public void ReturnCallRef_InTryTable_Compiled()
    {
        var exports = CreateMutualRecursionInTryTableModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(0));
        Assert.AreEqual(42, exports.Test(1));
        Assert.AreEqual(42, exports.Test(11));
    }

    /// <summary>
    /// The tail call inside the protected region still replaces the frame: five million mutual-recursion frames
    /// through the try_table trampoline must not overflow the stack.
    /// </summary>
    [TestMethod]
    public void ReturnCallRef_InTryTable_DeepRecursion_DoesNotOverflow()
    {
        var exports = CreateMutualRecursionInTryTableModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(42, exports.Test(5_000_000));
    }

    /// <summary>
    /// Tests the <see cref="ReturnCallRef.Equals(Instruction)"/> and <see cref="ReturnCallRef.GetHashCode()"/> methods.
    /// </summary>
    [TestMethod]
    public void ReturnCallRef_Equals()
    {
        TestUtility.CreateInstances<ReturnCallRef>(out var a, out var b);

        a.Type = 1;
        TestUtility.AreNotEqual(a, b);
        b.Type = 1;
        TestUtility.AreEqual(a, b);
    }
}
