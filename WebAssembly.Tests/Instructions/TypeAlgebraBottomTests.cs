using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Exercises the abstract/concrete arms of the WebAssembly 3.0 subtyping lattice
/// (<see cref="WebAssembly.Runtime.Compilation.TypeAlgebra"/>) with the internal-hierarchy bottom <c>(ref none)</c>
/// against concrete struct and array targets: the bottom is a subtype of every concrete struct/array (accepted), while
/// a concrete struct/array is not a subtype of the bottom (rejected).
/// </summary>
[TestClass]
public class TypeAlgebraBottomTests
{
    /// <summary>Exposes a niladic reference-returning function named "Test".</summary>
    public abstract class RefResult
    {
        /// <summary>Invokes the compiled function.</summary>
        public abstract object Test();
    }

    // A module whose type 0 is a struct/array of the given form and type 1 is () -> (ref null $0); the exported
    // function returns a (ref null none) value, checking that none <: the concrete struct/array result.
    private static Module BottomReturnedAsConcrete(FunctionType form)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = form, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] }); // 0
        var sig = new WebAssemblyType();
        sig.AddReturn(new ReferenceType(0u, isNullable: true)); // () -> (ref null $0)
        module.Types.Add(sig); // 1
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("Test", 0));
        module.Codes.Add(new FunctionBody(new RefNull(WebAssemblyHeapType.None), new End()));
        return module;
    }

    /// <summary><c>(ref none)</c> is a subtype of a concrete struct result: the function validates and returns null.</summary>
    [TestMethod]
    public void BottomNone_IsSubtypeOfConcreteStruct_Accepted()
    {
        var exports = BottomReturnedAsConcrete(FunctionType.Struct).ToInstance<RefResult>().Exports;
        Assert.IsNull(exports.Test());
    }

    /// <summary><c>(ref none)</c> is a subtype of a concrete array result: the function validates and returns null.</summary>
    [TestMethod]
    public void BottomNone_IsSubtypeOfConcreteArray_Accepted()
    {
        var exports = BottomReturnedAsConcrete(FunctionType.Array).ToInstance<RefResult>().Exports;
        Assert.IsNull(exports.Test());
    }

    // A module with a single struct/array type 0 and a () -> () function whose body is a br_on_cast whose target
    // reference type is (ref null $0) but whose source is (ref null none) — the target must be a subtype of the
    // source (rt2 <: rt1), which a concrete struct/array is not of the bottom, so validation must reject it.
    private static Module ConcreteNotSubtypeOfBottom(FunctionType form)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = form, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] }); // 0
        module.Types.Add(new WebAssemblyType()); // 1: () -> ()
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("Test", 0));
        module.Codes.Add(new FunctionBody(
            new BranchOnCast { CastFlags = 0b11, Index = 0, FromHeapType = WebAssemblyHeapType.None, ToTypeIndex = 0 },
            new End()));
        return module;
    }

    /// <summary>A concrete struct is not a subtype of <c>(ref none)</c>: the br_on_cast target check rejects it.</summary>
    [TestMethod]
    public void ConcreteStruct_NotSubtypeOfBottomNone_Rejected()
    {
        Assert.ThrowsExactly<StackTypeInvalidException>(() => ConcreteNotSubtypeOfBottom(FunctionType.Struct).ToInstance<object>());
    }

    /// <summary>A concrete array is not a subtype of <c>(ref none)</c>: the br_on_cast target check rejects it.</summary>
    [TestMethod]
    public void ConcreteArray_NotSubtypeOfBottomNone_Rejected()
    {
        Assert.ThrowsExactly<StackTypeInvalidException>(() => ConcreteNotSubtypeOfBottom(FunctionType.Array).ToInstance<object>());
    }
}
