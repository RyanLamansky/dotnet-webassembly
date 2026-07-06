using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefCast"/> instruction.
/// </summary>
[TestClass]
public class RefCastTests
{
    /// <summary>Tests <see cref="RefCast"/> opcode selection, immediates, and equality.</summary>
    [TestMethod]
    public void RefCast_OpCodeAndEquality()
    {
        var nonNull = new RefCast { HeapType = WebAssemblyHeapType.Array };
        Assert.AreEqual(OpCode.GCPrefix, nonNull.OpCode);
        Assert.AreEqual(GCOpCode.RefCast, nonNull.GCOpCode);
        Assert.IsFalse(nonNull.Nullable);

        var nullable = new RefCast(true) { HeapType = WebAssemblyHeapType.Any };
        Assert.AreEqual(GCOpCode.RefCastNull, nullable.GCOpCode);
        Assert.IsTrue(nullable.Nullable);

        Assert.AreEqual(new RefCast { HeapType = WebAssemblyHeapType.Array }, nonNull);
        Assert.AreNotEqual(new RefCast { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreEqual(new RefCast { HeapType = WebAssemblyHeapType.Array }.GetHashCode(), nonNull.GetHashCode());
    }

    /// <summary>Niladic i32-returning functions exercising the <c>ref.cast</c> concrete-func and bottom arms.</summary>
    public abstract class CastFuncTests
    {
        /// <summary><c>ref.cast (ref null $t)</c> of null, then <c>ref.is_null</c> (expects 1).</summary>
        public abstract int NullNullable();
        /// <summary><c>ref.cast (ref null $t)</c> of a matching funcref, then <c>ref.is_null</c> (expects 0).</summary>
        public abstract int NonNullNullable();
        /// <summary><c>ref.cast (ref $t)</c> of a matching funcref, then <c>ref.is_null</c> (expects 0).</summary>
        public abstract int NonNullNonNull();
        /// <summary><c>ref.cast (ref $t)</c> of null (a cast failure, traps).</summary>
        public abstract int NullNonNullTraps();
        /// <summary><c>ref.cast (ref nofunc)</c> of a non-null funcref (a bottom cast, always traps).</summary>
        public abstract int NonNullBottomTraps();
    }

    // Type 0 is the target func type () -> (); type 1 is the () -> (i32) test-function signature. Function 0 is the
    // (declaratively referenceable) target; functions 1..5 cast a funcref to (ref null? $0) or the func bottom.
    private static Module CastFuncTestModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] }); // 1: () -> (i32)
        module.Functions.Add(new Function(0)); // 0: target
        for (var i = 0; i < 5; i++)
            module.Functions.Add(new Function(1)); // 1..5: test functions
        module.Exports.Add(new Export(nameof(CastFuncTests.NullNullable), 1));
        module.Exports.Add(new Export(nameof(CastFuncTests.NonNullNullable), 2));
        module.Exports.Add(new Export(nameof(CastFuncTests.NonNullNonNull), 3));
        module.Exports.Add(new Export(nameof(CastFuncTests.NullNonNullTraps), 4));
        module.Exports.Add(new Export(nameof(CastFuncTests.NonNullBottomTraps), 5));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        module.Codes.Add(new FunctionBody(new End())); // func0
        module.Codes.Add(new FunctionBody(new RefNull { TypeIndex = 0 }, new RefCast(true) { TypeIndex = 0 }, new RefIsNull(), new End()));
        module.Codes.Add(new FunctionBody(new RefFunc(0), new RefCast(true) { TypeIndex = 0 }, new RefIsNull(), new End()));
        module.Codes.Add(new FunctionBody(new RefFunc(0), new RefCast { TypeIndex = 0 }, new RefIsNull(), new End()));
        module.Codes.Add(new FunctionBody(new RefNull { TypeIndex = 0 }, new RefCast { TypeIndex = 0 }, new RefIsNull(), new End()));
        module.Codes.Add(new FunctionBody(new RefFunc(0), new RefCast { HeapType = WebAssemblyHeapType.NoFunc }, new RefIsNull(), new End()));
        return module;
    }

    /// <summary><c>ref.cast</c> against a concrete function type passes for matching (and null, when nullable) values.</summary>
    [TestMethod]
    public void RefCast_ConcreteFuncType_NullableAndNonNull()
    {
        var exports = CastFuncTestModule().ToInstance<CastFuncTests>().Exports;
        Assert.AreEqual(1, exports.NullNullable(), "A null value passes a nullable cast and remains null.");
        Assert.AreEqual(0, exports.NonNullNullable(), "A matching funcref passes and is non-null.");
        Assert.AreEqual(0, exports.NonNullNonNull(), "A matching funcref passes the non-null cast.");
    }

    /// <summary>A non-null cast of null, and any cast to the func bottom, trap with <see cref="System.InvalidCastException"/>.</summary>
    [TestMethod]
    public void RefCast_Failures_Trap()
    {
        var exports = CastFuncTestModule().ToInstance<CastFuncTests>().Exports;
        Assert.ThrowsExactly<System.InvalidCastException>(() => exports.NullNonNullTraps());
        Assert.ThrowsExactly<System.InvalidCastException>(() => exports.NonNullBottomTraps());
    }
}
