using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefAsNonNull"/> instruction.
/// </summary>
[TestClass]
public class RefAsNonNullTests
{
    /// <summary>
    /// A <c>ref.as_non_null</c> applied to a null reference traps (surfacing as <see cref="System.NullReferenceException"/>).
    /// </summary>
    [TestMethod]
    public void RefAsNonNull_Null_Traps()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        // ref.null func; ref.as_non_null; drop; i32.const 3
        module.Codes.Add(new FunctionBody(
            new RefNull(WebAssemblyHeapType.Func), new RefAsNonNull(), new Drop(),
            new Int32Constant(3), new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.ThrowsExactly<System.NullReferenceException>(() => exports.Test());
    }

    /// <summary>
    /// A <c>ref.as_non_null</c> applied to a non-null reference passes it through unchanged.
    /// </summary>
    [TestMethod]
    public void RefAsNonNull_NonNull_PassesThrough()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        // ref.func 0; ref.as_non_null; ref.is_null; (0 because non-null)
        module.Codes.Add(new FunctionBody(
            new RefFunc(0), new RefAsNonNull(), new RefIsNull(), new End()));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(0, exports.Test());
    }

    /// <summary>Tests the <see cref="RefAsNonNull.OpCode"/> and basic equality.</summary>
    [TestMethod]
    public void RefAsNonNull_OpCode()
    {
        Assert.AreEqual(OpCode.RefAsNonNull, new RefAsNonNull().OpCode);
        Assert.AreEqual(new RefAsNonNull(), new RefAsNonNull());
    }
}
