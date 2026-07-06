using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using WebAssembly.Instructions;

namespace WebAssembly;

/// <summary>
/// Object-model and byte-identical round-trip tests for WASM 3.0 exception handling
/// (Tag section, tag import/export, throw/throw_ref/try_table with all catch kinds).
/// </summary>
[TestClass]
public class ExceptionModelTests
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

    /// <summary>
    /// Each fixture exercises one or more new WASM 3.0 exception-handling encodings; all must re-emit byte-for-byte.
    /// (tag.4-7 reference tags through recursion groups, which round-trip like any other rec-group type.)
    /// </summary>
    [TestMethod]
    [DataRow("exceptions/tag/tag.0")]           // tag section + type with params
    [DataRow("exceptions/tag/tag.1")]
    [DataRow("exceptions/tag/tag.2")]           // assert_invalid non-empty tag result (parses, fails at validation)
    [DataRow("exceptions/tag/tag.3")]
    [DataRow("exceptions/tag/tag.4")]
    [DataRow("exceptions/tag/tag.5")]
    [DataRow("exceptions/tag/tag.6")]
    [DataRow("exceptions/tag/tag.7")]
    [DataRow("exceptions/throw/throw.0")]       // throw 0x08 + tagidx
    [DataRow("exceptions/throw/throw.1")]
    [DataRow("exceptions/throw/throw.2")]
    [DataRow("exceptions/throw/throw.3")]
    [DataRow("exceptions/throw_ref/throw_ref.0")] // throw_ref 0x0a
    [DataRow("exceptions/throw_ref/throw_ref.1")]
    [DataRow("exceptions/throw_ref/throw_ref.2")]
    [DataRow("exceptions/try_table/try_table.0")]  // try_table 0x1f, catch/catch_all
    [DataRow("exceptions/try_table/try_table.1")]  // return_call inside try_table, aliased tag imports
    [DataRow("exceptions/try_table/try_table.2")]
    [DataRow("exceptions/try_table/try_table.5")]  // catch at function boundary
    [DataRow("exceptions/try_table/try_table.6")]
    [DataRow("exceptions/try_table/try_table.7")]
    [DataRow("exceptions/try_table/try_table.8")]
    [DataRow("exceptions/try_table/try_table.9")]
    [DataRow("exceptions/try_table/try_table.10")]
    [DataRow("exceptions/try_table/try_table.11")]
    [DataRow("exceptions/try_table/try_table.12")]
    [DataRow("exceptions/try_table/try_table.13")] // typed-ref tag params, catch_ref
    [DataRow("exceptions/try_table/try_table.14")] // assert_invalid type mismatch (parses)
    [DataRow("exceptions/try_table/try_table.15")]
    [DataRow("imports/imports.0")]                 // tag definitions + tag exports
    [DataRow("imports/imports.5")]                 // tag imports/exports
    public void ExceptionModel_Fixture_RoundTripsByteIdentical(string relative)
        => AssertFixtureRoundTripsByteIdentical(relative);

    /// <summary>
    /// A module hand-built with a tag section, tag import, tag export, and a try_table carrying all four catch
    /// kinds plus throw/throw_ref round-trips through binary and reloads to an equal structure.
    /// </summary>
    [TestMethod]
    public void ExceptionModel_HandBuilt_RoundTrips()
    {
        var module = new Module();
        // Type 0: () -> () ; Type 1: (i32) -> ()
        module.Types.Add(new WebAssemblyType());
        var withParam = new WebAssemblyType();
        withParam.Parameters.Add(WebAssemblyValueType.Int32);
        module.Types.Add(withParam);

        module.Imports.Add(new Import.Tag("m", "imported", 0));
        module.Tags.Add(new Tag(1));
        module.Exports.Add(new Export("exported_tag", 1, ExternalKind.Tag));

        var tryTable = new TryTable(BlockType.Empty);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0));
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchRef, 0, tag: 1));
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAll, 0));
        tryTable.Catches.Add(new CatchClause(CatchKind.CatchAllRef, 0));

        module.Functions.Add(new Function(0));
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                tryTable,
                new Throw(0),
                new End(),
                new ThrowRef(),
                new End(),
            ],
        });

        using var output = new MemoryStream();
        module.WriteToBinary(output);
        var bytes = output.ToArray();

        using var reread = new MemoryStream(bytes);
        var back = Module.ReadFromBinary(reread);

        Assert.AreEqual(1, back.Tags.Count);
        Assert.AreEqual(1u, back.Tags[0].TypeIndex);
        Assert.AreEqual(1, back.Imports.Count);
        Assert.IsInstanceOfType(back.Imports[0], typeof(Import.Tag));
        var importedTag = (Import.Tag)back.Imports[0];
        Assert.AreEqual(0u, importedTag.TypeIndex);
        var export = back.Exports.Single(e => e.Kind == ExternalKind.Tag);
        Assert.AreEqual("exported_tag", export.Name);
        Assert.AreEqual(1u, export.Index);

        var reloadedTry = back.Codes[0].Code.OfType<TryTable>().Single();
        Assert.AreEqual(4, reloadedTry.Catches.Count);
        Assert.AreEqual(CatchKind.Catch, reloadedTry.Catches[0].Kind);
        Assert.AreEqual(CatchKind.CatchRef, reloadedTry.Catches[1].Kind);
        Assert.AreEqual(CatchKind.CatchAll, reloadedTry.Catches[2].Kind);
        Assert.AreEqual(CatchKind.CatchAllRef, reloadedTry.Catches[3].Kind);
        Assert.IsTrue(reloadedTry.Equals(tryTable));

        // Byte-identical second round-trip.
        var again = Roundtrip(bytes);
        CollectionAssert.AreEqual(bytes, again);
    }
}
