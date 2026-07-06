using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;
using WebAssembly.Runtime.Compilation;

namespace WebAssembly.Runtime;

/// <summary>
/// Runtime tests for exception-tag identity, import, export, and cross-module linking.
/// </summary>
[TestClass]
public class ExceptionTagTests
{
    /// <summary>Exposes a single exported tag named "e".</summary>
    public abstract class TagExports
    {
        /// <summary>The exported exception tag.</summary>
        public abstract ExceptionTag e { get; }
    }

    private static Module DefiningModule()
    {
        // A module that defines one () -> () tag and exports it as "e".
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        module.Tags.Add(new Tag(0));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        return module;
    }

    /// <summary>A module-defined tag is exported as an <see cref="ExceptionTag"/> with fresh per-instance identity.</summary>
    [TestMethod]
    public void ExceptionTag_DefinedAndExported_FreshIdentityPerInstance()
    {
        var creator = DefiningModule().Compile<TagExports>();

        var a = creator(new ImportDictionary()).Exports;
        var b = creator(new ImportDictionary()).Exports;

        Assert.IsNotNull(a.e);
        Assert.IsNotNull(b.e);
        Assert.AreSame(a.e, a.e, "The same instance's tag must be reference-stable.");
        Assert.AreNotSame(a.e, b.e, "Each instantiation must produce a distinct tag identity.");
    }

    /// <summary>A host-supplied <see cref="ExceptionTag"/> import is stored and re-exported as the same instance.</summary>
    [TestMethod]
    public void ExceptionTag_HostImport_PreservedByReference()
    {
        // Imports tag "host"::"e" and re-exports it as "e".
        var module = new Module();
        module.Types.Add(new WebAssemblyType());
        module.Imports.Add(new Import.Tag("host", "e", 0));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));

        var hostTag = new ExceptionTag();
        var exports = module.Compile<TagExports>()(new ImportDictionary
        {
            { "host", "e", hostTag },
        }).Exports;

        Assert.AreSame(hostTag, exports.e, "The imported tag must be stored and re-exported by reference.");
    }

    /// <summary>A tag exported from one compiled module links by reference into another that imports it.</summary>
    [TestMethod]
    public void ExceptionTag_CrossModule_LinksByReference()
    {
        var provider = DefiningModule().Compile<TagExports>()(new ImportDictionary()).Exports;

        var imports = new ImportDictionary();
        imports.AddFromExports("provider", provider);

        // A module that imports provider::e and re-exports it.
        var importer = new Module();
        importer.Types.Add(new WebAssemblyType());
        importer.Imports.Add(new Import.Tag("provider", "e", 0));
        importer.Exports.Add(new Export("e", 0, ExternalKind.Tag));

        var importerExports = importer.Compile<TagExports>()(imports).Exports;

        Assert.AreSame(provider.e, importerExports.e, "The tag must link across modules by reference identity.");
    }

    // A compiled instance exporting a tag "e" of the given signature.
    private static TagExports ProvidedTag(WebAssemblyType tagType)
    {
        var module = new Module();
        module.Types.Add(tagType);
        module.Tags.Add(new Tag(0));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        return module.Compile<TagExports>()(new ImportDictionary()).Exports;
    }

    /// <summary>
    /// A parameterized tag exported from one compiled module links into another that imports the structurally-equal
    /// signature — the closed descriptors (compiler-rendered on both sides) match.
    /// </summary>
    [TestMethod]
    public void ExceptionTag_CrossModuleParameterized_Matches()
    {
        var provider = ProvidedTag(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] });

        var imports = new ImportDictionary();
        imports.AddFromExports("provider", provider);

        var importer = new Module();
        importer.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] });
        importer.Imports.Add(new Import.Tag("provider", "e", 0));
        importer.Exports.Add(new Export("e", 0, ExternalKind.Tag));

        Assert.AreSame(provider.e, importer.Compile<TagExports>()(imports).Exports.e,
            "Structurally-equal tag signatures must link across modules by reference.");
    }

    /// <summary>
    /// A tag exported with one signature does not satisfy an import declaring a structurally-different signature: tag
    /// matching is invariant, so the descriptor difference is rejected even though both are <see cref="ExceptionTag"/>s.
    /// </summary>
    [TestMethod]
    public void ExceptionTag_CrossModuleIncompatible_Throws()
    {
        var provider = ProvidedTag(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] });

        var imports = new ImportDictionary();
        imports.AddFromExports("provider", provider);

        var importer = new Module();
        importer.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32] }); // (i32) -> (), differs
        importer.Imports.Add(new Import.Tag("provider", "e", 0));

        Assert.ThrowsExactly<ImportException>(() => importer.ToInstance<object>(imports));
    }

    // A module that imports tag "host"::"e" of the given signature and re-exports it as "e".
    private static Module ImportReExportTagModule(WebAssemblyType tagType)
    {
        var module = new Module();
        module.Types.Add(tagType);
        module.Imports.Add(new Import.Tag("host", "e", 0));
        module.Exports.Add(new Export("e", 0, ExternalKind.Tag));
        return module;
    }

    /// <summary>
    /// A host <see cref="ExceptionTag(WebAssemblyValueType[])"/> whose rendered descriptor equals the module's
    /// declared tag signature links successfully — proving the host-rendered descriptor matches the compiler-rendered
    /// one for a multi-operand signature.
    /// </summary>
    [TestMethod]
    public void ExceptionTag_HostParameterizedImport_Matches()
    {
        var module = ImportReExportTagModule(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] });

        var hostTag = new ExceptionTag(WebAssemblyValueType.Int32, WebAssemblyValueType.Int64);
        var exports = module.Compile<TagExports>()(new ImportDictionary
        {
            { "host", "e", hostTag },
        }).Exports;

        Assert.AreSame(hostTag, exports.e, "A matching host-rendered tag descriptor must link by reference.");
    }

    /// <summary>A host tag whose rendered descriptor differs from the declared tag signature is rejected.</summary>
    [TestMethod]
    public void ExceptionTag_HostParameterizedImport_Mismatch_Throws()
    {
        var module = ImportReExportTagModule(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] });

        // Declared (i32, i64) -> () but supplied (i32) -> (): the descriptors differ.
        var hostTag = new ExceptionTag(WebAssemblyValueType.Int32);
        Assert.ThrowsExactly<ImportException>(() => module.Compile<TagExports>()(new ImportDictionary
        {
            { "host", "e", hostTag },
        }));
    }

    /// <summary>
    /// The general recursion-group renderer and the standalone-tag fast path never drift: the descriptor
    /// <see cref="CanonicalTypeMap.Build"/> produces for a single-member final group holding one
    /// <c>(param i32 i64)</c> function equals <see cref="CanonicalTypeMap.RenderStandaloneTagDescriptor"/> for the same
    /// operand types — the one rendering that both a compiled tag definition and a host <see cref="ExceptionTag"/> go
    /// through, so a silent divergence (which once caused a real linking bug) is caught here.
    /// </summary>
    [TestMethod]
    public void ExceptionTag_StandaloneDescriptor_MatchesGeneralRenderer()
    {
        var map = CanonicalTypeMap.Build(
            [new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int64] }],
            [(0, 1)]);

        Assert.AreEqual(
            map.Descriptor[0],
            CanonicalTypeMap.RenderStandaloneTagDescriptor([WebAssemblyValueType.Int32, WebAssemblyValueType.Int64]));
    }

    /// <summary>Exposes an i32-returning function named "Test".</summary>
    public abstract class ToInt32
    {
        /// <summary>Invokes the compiled function.</summary>
        public abstract int Test();
    }

    /// <summary>
    /// A host parameterized tag is usable at run time: a module that imports an <c>(i32) -&gt; ()</c> tag, throws it
    /// with an operand, and catches it recovers the operand — the host descriptor matched the compiler's.
    /// </summary>
    [TestMethod]
    public void ExceptionTag_HostParameterizedImport_ThrowCatchRoundTrips()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32] }); // 0: (i32) -> () tag
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] }); // 1: () -> (i32)
        module.Imports.Add(new Import.Tag("host", "e", 0));
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("Test", 0, ExternalKind.Function));

        var tryTable = new TryTable(BlockType.Int32);
        tryTable.Catches.Add(new CatchClause(CatchKind.Catch, 0, tag: 0)); // catch $e -> $h (label 0)

        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Block(BlockType.Int32), // $h (result i32)
                    tryTable,               // try_table (result i32)
                        new Int32Constant(77),
                        new Throw(0),       // throw imported tag 0, carrying 77
                    new End(),              // end try_table (normal path unreachable)
                new End(),                  // end block $h (yields the caught operand)
                new End(),                  // end function
            ],
        });

        var exports = module.Compile<ToInt32>()(new ImportDictionary
        {
            { "host", "e", new ExceptionTag(WebAssemblyValueType.Int32) },
        }).Exports;

        Assert.AreEqual(77, exports.Test(), "The thrown operand must be recovered by the catch.");
    }
}
