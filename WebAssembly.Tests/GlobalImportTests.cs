using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Intrinsics;
using WebAssembly.Instructions;
using WebAssembly.Runtime;

namespace WebAssembly;

/// <summary>
/// Tests basic functionality of <see cref="GlobalImport"/> when used with <see cref="Compile"/>.
/// </summary>
[TestClass]
public class GlobalImportTests
{
    /// <summary>
    /// Always returns 3, used by <see cref="Compile_GlobalImmutableImportExport"/>.
    /// </summary>
    public static int ImportedImmutableGlobalReturns3 => 3;

    /// <summary>
    /// Verifies that imported globals can be exported.
    /// </summary>
    [TestMethod]
    [Timeout(1000)]
    public void Compile_GlobalImmutableImportExport()
    {
        var module = new Module();
        module.Imports.Add(new Import.Global
        {
            Module = "Imported",
            Field = "Global",
            ContentType = WebAssemblyValueType.Int32,
        });
        module.Exports.Add(new Export
        {
            Name = "Test",
            Kind = ExternalKind.Global,
        });

        var compiled = module.ToInstance<CompilerTestBaseExportedImmutableGlobal<int>>(
            new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => ImportedImmutableGlobalReturns3) },
            });

        Assert.IsNotNull(compiled);
        Assert.IsNotNull(compiled.Exports);

        var instance = compiled.Exports;

        Assert.AreEqual(ImportedImmutableGlobalReturns3, instance.Test);
    }

    /// <summary>
    /// Used by <see cref="Compile_GlobalMutableImportExport"/>.
    /// </summary>
    public static int MutableGlobal { get; set; }

    /// <summary>
    /// Verifies that imported globals can be exported.
    /// </summary>
    [TestMethod]
    [Timeout(1000)]
    public void Compile_GlobalMutableImportExport()
    {
        var module = new Module();
        module.Imports.Add(new Import.Global
        {
            Module = "Imported",
            Field = "Global",
            ContentType = WebAssemblyValueType.Int32,
            IsMutable = true,
        });
        module.Exports.Add(new Export
        {
            Name = "Test",
            Kind = ExternalKind.Global,
        });

        var compiled = module.ToInstance<CompilerTestBaseExportedMutableGlobal<int>>(
            new ImportDictionary {
                    { "Imported", "Global", new GlobalImport(() => MutableGlobal, value => MutableGlobal = value) },
            });

        Assert.IsNotNull(compiled);
        Assert.IsNotNull(compiled.Exports);

        var instance = compiled.Exports;

        Assert.AreEqual(0, instance.Test);

        const int passedThroughImport = 5;
        MutableGlobal = passedThroughImport;
        Assert.AreEqual(passedThroughImport, instance.Test);

        const int passedThroughExport = 7;
        instance.Test = passedThroughExport;
        Assert.AreEqual(passedThroughExport, MutableGlobal);
    }

    /// <summary>
    /// Verifies that exported SIMD globals can be re-imported through <see cref="ImportDictionaryExtensions.AddFromExports"/>.
    /// </summary>
    [TestMethod]
    public void Compile_GlobalImmutableImportExport_V128()
    {
        var exporter = new Module();
        exporter.Globals.Add(new Global
        {
            ContentType = WebAssemblyValueType.V128,
            IsMutable = false,
            InitializerExpression =
            [
                new V128Const { Value = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15] },
                new End(),
            ],
        });
        exporter.Exports.Add(new Export
        {
            Name = "Test",
            Kind = ExternalKind.Global,
        });

        var exported = exporter.ToInstance<CompilerTestBaseExportedImmutableGlobal<Vector128<byte>>>();

        var imports = new ImportDictionary();
        imports.AddFromExports("Imported", exported.Exports);

        var importer = new Module();
        importer.Imports.Add(new Import.Global
        {
            Module = "Imported",
            Field = "Test",
            ContentType = WebAssemblyValueType.V128,
        });
        importer.Exports.Add(new Export
        {
            Name = "Test",
            Kind = ExternalKind.Global,
        });

        var roundTripped = importer.ToInstance<CompilerTestBaseExportedImmutableGlobal<Vector128<byte>>>(imports);

        Assert.AreEqual(exported.Exports.Test, roundTripped.Exports.Test);
    }
}
