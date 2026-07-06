using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Phase 5 (M5.1) representation and byte-identical round-trip tests for WASM 3.0 typed function references.
/// </summary>
[TestClass]
public class ReferenceTypeTests
{
    private static string FixturePath(string relative) =>
        Path.Combine("Runtime", "SpecTestData", Path.Combine(relative.Split('/'))) + ".wasm";

    private static byte[] Roundtrip(byte[] original)
    {
        Module module;
        using (var input = new MemoryStream(original))
            module = Module.ReadFromBinary(input);

        using var output = new MemoryStream();
        module.WriteToBinary(output);
        return output.ToArray();
    }

    private static void AssertFixtureRoundTripsByteIdentical(string relative)
    {
        var path = FixturePath(relative);
        Assert.IsTrue(File.Exists(path), $"Fixture not found: {path}");
        var original = File.ReadAllBytes(path);
        var roundTripped = Roundtrip(original);
        CollectionAssert.AreEqual(original, roundTripped, $"Round-trip of {relative} was not byte-identical.");
    }

    // Each fixture exercises one or more new WASM 3.0 reference encodings; the WAT of each was confirmed with
    // wasm-tools. All must re-emit byte-for-byte (the additive design's core invariant).
    /// <summary>
    /// </summary>
    [TestMethod]
    [DataRow("ref/ref.0")]                 // reftype params: funcref/externref shorthand, (ref func)/(ref extern), (ref $t)/(ref idx), (ref null $t)
    [DataRow("ref/ref.1")]
    [DataRow("ref/ref.2")]
    [DataRow("ref_null/ref_null.0")]       // abstract heap types: any, func, exn, extern, none, nofunc, noextern; ref.null with heap types
    [DataRow("ref_null/ref_null.1")]
    [DataRow("ref_func/ref_func.0")]       // typed ref.func
    [DataRow("local_init/local_init.0")]   // non-nullable ref locals
    [DataRow("local_init/local_init.5")]
    [DataRow("table/table.10")]            // typed table (ref null $t) -> 0x63 0x00
    [DataRow("table/table.14")]            // table-with-initializer 0x40 0x00 form + ref.null func init expr
    [DataRow("br_table/br_table.0")]       // block-result reftypes + typed element segment
    [DataRow("elem/elem.0")]               // reference-typed element segment
    // Representative pre-3.0 fixtures, proving the additive changes preserve classic encodings byte-for-byte.
    [DataRow("func/func.0")]
    [DataRow("block/block.0")]
    [DataRow("i32/i32.0")]
    [DataRow("fac/fac.0")]
    public void ReferenceType_Fixture_RoundTripsByteIdentical(string relative)
        => AssertFixtureRoundTripsByteIdentical(relative);

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_TypedTable_ExactEncoding()
    {
        // A table of (ref null $0) with min 1 encodes its element type as 0x63 0x00 (NullableReference + heap index 0),
        // matching table.10.wasm's table section body "63 00 00 01".
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        module.Tables.Add(new Table(1) { ElementRefType = new ReferenceType(0u, isNullable: true) });

        using var output = new MemoryStream();
        module.WriteToBinary(output);
        var bytes = output.ToArray();

        // Locate the table section (id 4) and confirm its body.
        var expectedSection = new byte[] { 0x04, 0x05, 0x01, 0x63, 0x00, 0x00, 0x01 };
        var index = IndexOf(bytes, expectedSection);
        Assert.IsTrue(index >= 0, "Expected table section body 04 05 01 63 00 00 01 was not found.");

        var reloaded = Roundtrip(bytes);
        CollectionAssert.AreEqual(bytes, reloaded);

        using var reread = new MemoryStream(bytes);
        var back = Module.ReadFromBinary(reread);
        var table = back.Tables.Single();
        Assert.AreEqual(ElementType.NullableReference, table.ElementType);
        Assert.IsNotNull(table.ElementRefType);
        Assert.AreEqual(0u, table.ElementRefType!.Value.TypeIndex);
        Assert.IsTrue(table.ElementRefType!.Value.IsNullable);
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_TableWithInitializer_RoundTrips()
    {
        var module = new Module();
        var table = new Table(1)
        {
            InitializerExpression = [new RefNull(WebAssemblyHeapType.Func), new End()],
        };
        module.Tables.Add(table);

        var back = module.BinaryRoundTrip();
        var reloaded = back.Tables.Single();
        Assert.IsNotNull(reloaded.InitializerExpression);
        Assert.AreEqual(2, reloaded.InitializerExpression!.Count);
        Assert.IsInstanceOfType(reloaded.InitializerExpression[0], typeof(RefNull));
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_SignatureRefParameter_RoundTrips()
    {
        var module = new Module();
        var type = new WebAssemblyType();
        type.Parameters.Add(WebAssemblyValueType.Int32);
        type.AddParameter(new ReferenceType(WebAssemblyHeapType.Func, isNullable: false)); // (ref func)
        type.AddParameter(new ReferenceType(0u, isNullable: true));                        // (ref null $0)
        module.Types.Add(type);

        var back = module.BinaryRoundTrip();
        var reloaded = back.Types.Single();
        Assert.AreEqual(3, reloaded.Parameters.Count);
        Assert.AreEqual(WebAssemblyValueType.Int32, reloaded.Parameters[0]);
        Assert.IsNull(reloaded.GetParameterRefType(0));

        Assert.AreEqual(WebAssemblyValueType.Reference, reloaded.Parameters[1]);
        var p1 = reloaded.GetParameterRefType(1);
        Assert.IsNotNull(p1);
        Assert.AreEqual(WebAssemblyHeapType.Func, p1!.Value.HeapType);
        Assert.IsFalse(p1.Value.IsNullable);

        Assert.AreEqual(WebAssemblyValueType.NullableReference, reloaded.Parameters[2]);
        var p2 = reloaded.GetParameterRefType(2);
        Assert.IsNotNull(p2);
        Assert.AreEqual(0u, p2!.Value.TypeIndex);
        Assert.AreEqual(type, reloaded);
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_RefNull_ConcreteIndex_RoundTrips()
    {
        var refNull = new RefNull { TypeIndex = 3 };
        Assert.AreEqual(WebAssemblyValueType.NullableReference, refNull.Type);

        using var output = new MemoryStream();
        var writerModule = BuildModuleWithGlobalInit(new ReferenceType(3u, isNullable: true), refNull);
        var back = writerModule.BinaryRoundTrip();
        var init = back.Globals.Single().InitializerExpression;
        var reloaded = (RefNull)init.First();
        Assert.AreEqual(3u, reloaded.TypeIndex);
        Assert.IsNull(reloaded.HeapType);
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_RefNull_AbstractHeapTypes_RoundTrip()
    {
        foreach (var heapType in new[]
        {
            WebAssemblyHeapType.Func, WebAssemblyHeapType.Extern, WebAssemblyHeapType.Any,
            WebAssemblyHeapType.None, WebAssemblyHeapType.NoFunc, WebAssemblyHeapType.NoExtern,
            WebAssemblyHeapType.Exn, WebAssemblyHeapType.NoExn, WebAssemblyHeapType.Eq,
            WebAssemblyHeapType.I31, WebAssemblyHeapType.Struct, WebAssemblyHeapType.Array,
        })
        {
            var contentRefType = new ReferenceType(heapType, isNullable: true);
            var module = BuildModuleWithGlobalInit(contentRefType, new RefNull(heapType));
            var back = module.BinaryRoundTrip();
            var reloaded = (RefNull)back.Globals.Single().InitializerExpression.First();
            Assert.AreEqual(heapType, reloaded.HeapType, $"Heap type {heapType} did not round-trip.");
        }
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    public void ReferenceType_GlobalRefContentType_RoundTrips()
    {
        var module = BuildModuleWithGlobalInit(
            new ReferenceType(WebAssemblyHeapType.Extern, isNullable: true),
            new RefNull(WebAssemblyHeapType.Extern));

        var back = module.BinaryRoundTrip();
        var global = back.Globals.Single();
        Assert.AreEqual(WebAssemblyValueType.NullableReference, global.ContentType);
        Assert.IsNotNull(global.ContentRefType);
        Assert.AreEqual(WebAssemblyHeapType.Extern, global.ContentRefType!.Value.HeapType);
    }

    /// <summary>
    /// A type-section entry whose form byte is <c>0x5F</c> (struct) is a WASM 3.0 GC composite type, parsed since
    /// Phase 8 (M8.1) and surfaced through the object model with a byte-identical round-trip.
    /// </summary>
    [TestMethod]
    public void ReferenceType_CompositeTypeForm_StructParsed()
    {
        var bytes = new byte[]
        {
            0x00, 0x61, 0x73, 0x6d, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x03, 0x01, 0x5f, 0x00, // type section: 1 group, bare struct composite (0x5f), 0 fields
        };
        using var input = new MemoryStream(bytes);
        var module = Module.ReadFromBinary(input);

        var type = module.Types.Single();
        Assert.AreEqual(FunctionType.Struct, type.Form);
        Assert.IsNotNull(type.Fields);
        Assert.AreEqual(0, type.Fields!.Count);
        Assert.IsTrue(type.IsFinal);
        Assert.IsNotNull(module.RecursionGroups);
        Assert.AreEqual(1, module.RecursionGroups!.Count);

        using var output = new MemoryStream();
        module.WriteToBinary(output);
        CollectionAssert.AreEqual(bytes, output.ToArray());
    }

    private static Module BuildModuleWithGlobalInit(ReferenceType contentRefType, Instruction value)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        module.Globals.Add(new Global(contentRefType)
        {
            InitializerExpression = [value, new End()],
        });
        return module;
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return i;
        }

        return -1;
    }
}
