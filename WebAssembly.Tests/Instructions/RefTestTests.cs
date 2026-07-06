using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefTest"/> instruction.
/// </summary>
[TestClass]
public class RefTestTests
{
    /// <summary>Tests <see cref="RefTest"/> opcode selection, immediates, and equality.</summary>
    [TestMethod]
    public void RefTest_OpCodeAndEquality()
    {
        var nonNull = new RefTest { HeapType = WebAssemblyHeapType.Struct };
        Assert.AreEqual(OpCode.GCPrefix, nonNull.OpCode);
        Assert.AreEqual(GCOpCode.RefTest, nonNull.GCOpCode);
        Assert.IsFalse(nonNull.Nullable);
        Assert.AreEqual(WebAssemblyHeapType.Struct, nonNull.HeapType);
        Assert.IsNull(nonNull.TypeIndex);

        var nullable = new RefTest(true) { TypeIndex = 3 };
        Assert.AreEqual(GCOpCode.RefTestNull, nullable.GCOpCode);
        Assert.IsTrue(nullable.Nullable);
        Assert.AreEqual(3u, nullable.TypeIndex);
        Assert.IsNull(nullable.HeapType);

        Assert.AreEqual(new RefTest { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreNotEqual(new RefTest(true) { HeapType = WebAssemblyHeapType.Struct }, nonNull);
        Assert.AreEqual(new RefTest { HeapType = WebAssemblyHeapType.Struct }.GetHashCode(), nonNull.GetHashCode());
    }

    /// <summary>Four niladic i32-returning functions, one per <c>ref.test</c> concrete-func-type combination.</summary>
    public abstract class ConcreteFuncTests
    {
        /// <summary><c>ref.test (ref null $t)</c> of a null funcref.</summary>
        public abstract int NullNullable();
        /// <summary><c>ref.test (ref null $t)</c> of a matching non-null funcref.</summary>
        public abstract int NonNullNullable();
        /// <summary><c>ref.test (ref $t)</c> of a null funcref.</summary>
        public abstract int NullNonNull();
        /// <summary><c>ref.test (ref $t)</c> of a matching non-null funcref.</summary>
        public abstract int NonNullNonNull();
    }

    // Type 0 is the target func type () -> (); type 1 is the () -> (i32) test-function signature. Function 0 is the
    // (declaratively referenceable) target; functions 1..4 test a funcref against (ref null? $0) via ref.test.
    private static Module ConcreteFuncTestModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // 0: () -> ()
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] }); // 1: () -> (i32)
        module.Functions.Add(new Function(0)); // 0: target
        module.Functions.Add(new Function(1)); // 1: NullNullable
        module.Functions.Add(new Function(1)); // 2: NonNullNullable
        module.Functions.Add(new Function(1)); // 3: NullNonNull
        module.Functions.Add(new Function(1)); // 4: NonNullNonNull
        module.Exports.Add(new Export(nameof(ConcreteFuncTests.NullNullable), 1));
        module.Exports.Add(new Export(nameof(ConcreteFuncTests.NonNullNullable), 2));
        module.Exports.Add(new Export(nameof(ConcreteFuncTests.NullNonNull), 3));
        module.Exports.Add(new Export(nameof(ConcreteFuncTests.NonNullNonNull), 4));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        module.Codes.Add(new FunctionBody(new End())); // func0
        module.Codes.Add(new FunctionBody(new RefNull { TypeIndex = 0 }, new RefTest(true) { TypeIndex = 0 }, new End()));
        module.Codes.Add(new FunctionBody(new RefFunc(0), new RefTest(true) { TypeIndex = 0 }, new End()));
        module.Codes.Add(new FunctionBody(new RefNull { TypeIndex = 0 }, new RefTest { TypeIndex = 0 }, new End()));
        module.Codes.Add(new FunctionBody(new RefFunc(0), new RefTest { TypeIndex = 0 }, new End()));
        return module;
    }

    /// <summary><c>ref.test</c> against a concrete function type exercises all four null/non-null × nullable-target arms.</summary>
    [TestMethod]
    public void RefTest_ConcreteFuncType_AllCombinations()
    {
        var exports = ConcreteFuncTestModule().ToInstance<ConcreteFuncTests>().Exports;
        Assert.AreEqual(1, exports.NullNullable(), "A null funcref matches (ref null $t).");
        Assert.AreEqual(1, exports.NonNullNullable(), "A matching funcref matches (ref null $t).");
        Assert.AreEqual(0, exports.NullNonNull(), "A null funcref does not match (ref $t).");
        Assert.AreEqual(1, exports.NonNullNonNull(), "A matching funcref matches (ref $t).");
    }
}
