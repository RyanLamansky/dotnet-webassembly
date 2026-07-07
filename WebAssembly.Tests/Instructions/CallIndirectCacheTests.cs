using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the monomorphic inline cache that <see cref="CallIndirect"/> emits around the canonical function-type
/// guard (<see cref="WebAssembly.Runtime.WebAssemblyGC.CheckIndirectCallTypeCached"/>). The cache must accelerate
/// the repeated-target case without ever masking a canonical type mismatch, and must stay correct when the target
/// slot is mutated or when the shared static cache field is exercised across instances of one compilation.
/// </summary>
[TestClass]
public class CallIndirectCacheTests
{
    /// <summary>Abstract exports for the table-mutation test: <c>Call</c> dispatches slot 0, <c>Swap</c> rewrites it.</summary>
    public abstract class TableSwapExports
    {
        /// <summary>Creates a new <see cref="TableSwapExports"/> instance.</summary>
        protected TableSwapExports()
        {
        }

        /// <summary>Indirectly calls the function currently in table slot 0.</summary>
        /// <returns>The selected function's result.</returns>
        public abstract int Call();

        /// <summary>Rewrites table slot 0 to a different function of the same type.</summary>
        public abstract void Swap();
    }

    /// <summary>
    /// Repeated indirect calls through the same, unchanging slot return the correct result every time — the
    /// cache-hit path (second and later calls reuse the cached delegate without re-running the ancestry lookup).
    /// </summary>
    [TestMethod]
    public void CallIndirect_RepeatedSameSlot_CacheHit()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(0)); // func0: the table target.
        module.Functions.Add(new Function(0)); // func1: exported "Test", calls slot 0 indirectly.
        module.Codes.Add(new FunctionBody(new Int32Constant(42), new End()));
        module.Codes.Add(new FunctionBody(new Int32Constant(0), new CallIndirect(0), new End()));
        module.Tables.Add(new Table(1));
        module.Elements.Add(new Element(0, 0)); // slot 0 = func0.
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 1));

        var exports = module.ToInstance<CompilerTestBase0<int>>().Exports;
        Assert.AreEqual(42, exports.Test()); // Cache miss: validates and caches.
        Assert.AreEqual(42, exports.Test()); // Cache hit.
        Assert.AreEqual(42, exports.Test()); // Cache hit.
    }

    /// <summary>
    /// After <c>table.set</c> swaps slot 0 to a different (but same-type) function, subsequent calls dispatch to the
    /// new target: the cache misses on the changed delegate, re-validates, and re-caches.
    /// </summary>
    [TestMethod]
    public void CallIndirect_TableSetSwap_Revalidates()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32] }); // 0: []->[i32]
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [] });                          // 1: []->[]

        module.Functions.Add(new Function(0)); // func0: returns 10, initial slot 0 occupant.
        module.Functions.Add(new Function(0)); // func1: returns 20, swapped in.
        module.Functions.Add(new Function(0)); // func2: exported "Call".
        module.Functions.Add(new Function(1)); // func3: exported "Swap".

        module.Codes.Add(new FunctionBody(new Int32Constant(10), new End()));
        module.Codes.Add(new FunctionBody(new Int32Constant(20), new End()));
        module.Codes.Add(new FunctionBody(new Int32Constant(0), new CallIndirect(0), new End()));
        module.Codes.Add(new FunctionBody(new Int32Constant(0), new RefFunc(1), new TableSet { TableIndex = 0 }, new End()));

        module.Tables.Add(new Table(1));
        module.Elements.Add(new Element(0, 0)); // slot 0 = func0.

        // ref.func 1 requires func1 to be declared referenceable.
        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(1);
        module.Elements.Add(declared);

        module.Exports.Add(new Export("Call", 2));
        module.Exports.Add(new Export("Swap", 3));

        var exports = module.ToInstance<TableSwapExports>().Exports;
        Assert.AreEqual(10, exports.Call()); // Cache miss: caches func0.
        Assert.AreEqual(10, exports.Call()); // Cache hit.
        exports.Swap();                       // slot 0 = func1.
        Assert.AreEqual(20, exports.Call()); // Cache miss on the new delegate: re-validates and dispatches func1.
        Assert.AreEqual(20, exports.Call()); // Cache hit on func1.
    }

    // Two func types, both []->[i32] (so both lower to the same CLR delegate shape), made canonically distinct only
    // by finality — one final, one not — so the arity castclass alone would accept either but the canonical guard
    // must reject the wrong one. Declared call type is index 0 (the final type); slot 0 holds a type-0 function
    // (matches) and slot 1 holds a type-1 function (mismatch).
    private static Module BuildFinalityMismatchModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32] });                 // 0: final []->[i32]
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32], IsFinal = false }); // 1: non-final []->[i32]
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] }); // 2: Test

        module.Functions.Add(new Function(0)); // func0: type 0, returns 10 — matches the declared call type.
        module.Functions.Add(new Function(1)); // func1: type 1, returns 20 — canonically distinct, same CLR shape.
        module.Functions.Add(new Function(2)); // func2: exported "Test".

        module.Codes.Add(new FunctionBody(new Int32Constant(10), new End()));
        module.Codes.Add(new FunctionBody(new Int32Constant(20), new End()));
        module.Codes.Add(new FunctionBody(new LocalGet(0), new CallIndirect { Type = 0, Table = 0 }, new End()));

        module.Tables.Add(new Table(2, 2));
        module.Elements.Add(new Element(0, 0, 1)); // slot 0 = func0 (type 0), slot 1 = func1 (type 1).
        module.Exports.Add(new Export(nameof(CompilerTestBase<int>.Test), 2));
        return module;
    }

    /// <summary>
    /// A canonically-mismatched target still traps "indirect call type mismatch" even after a successful call through
    /// the same remapper cached a different (matching) delegate: the cache must never mask a mismatch.
    /// </summary>
    [TestMethod]
    public void CallIndirect_CanonicalMismatch_NotMaskedByCache()
    {
        var exports = BuildFinalityMismatchModule().ToInstance<CompilerTestBase<int>>().Exports;

        // Slot 0 holds a type-0 function: matches the declared type, caches that delegate.
        Assert.AreEqual(10, exports.Test(0));
        Assert.AreEqual(10, exports.Test(0)); // Cache hit, still fine.

        // Slot 1 holds a canonically-distinct type-1 function through the *same* remapper (same type+table); the
        // cache holds the type-0 delegate, so this is a miss that must re-validate and trap rather than be accepted.
        var ex = Assert.ThrowsExactly<InvalidCastException>(() => exports.Test(1));
        Assert.AreEqual("indirect call type mismatch", ex.Message);

        // And the cache state after a trap must not corrupt a subsequent valid call.
        Assert.AreEqual(10, exports.Test(0));
    }

    /// <summary>
    /// Two instances created from the same compilation both dispatch correctly. The cache slot is a private static
    /// field shared across instances, and each instance owns distinct delegate objects for the same function, so the
    /// second instance necessarily misses the cache warmed by the first, re-validates, and still dispatches correctly.
    /// </summary>
    [TestMethod]
    public void CallIndirect_SharedCacheAcrossInstances()
    {
        byte[] bytes;
        using (var memory = new MemoryStream())
        {
            BuildFinalityMismatchModule().WriteToBinary(memory);
            bytes = memory.ToArray();
        }

        InstanceCreator<CompilerTestBase<int>> maker;
        using (var input = new MemoryStream(bytes, writable: false))
            maker = Compile.FromBinary<CompilerTestBase<int>>(input);

        var first = maker(new ImportDictionary()).Exports;
        var second = maker(new ImportDictionary()).Exports;

        // First instance warms the shared static cache with its own delegate for slot 0.
        Assert.AreEqual(10, first.Test(0));

        // Second instance's slot-0 delegate is a different object, so it misses the warmed cache, re-validates, and
        // still returns the right result — the shared cache neither wrongly accepts nor wrongly rejects it.
        Assert.AreEqual(10, second.Test(0));

        // Both instances still enforce the canonical mismatch on slot 1.
        Assert.AreEqual("indirect call type mismatch",
            Assert.ThrowsExactly<InvalidCastException>(() => first.Test(1)).Message);
        Assert.AreEqual("indirect call type mismatch",
            Assert.ThrowsExactly<InvalidCastException>(() => second.Test(1)).Message);

        // Interleave to confirm neither instance's cache state disturbs the other.
        Assert.AreEqual(10, first.Test(0));
        Assert.AreEqual(10, second.Test(0));
    }
}
