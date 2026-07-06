using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the closed structural import/export descriptors that link reference-typed globals and tables across
/// compiled-module boundaries, where the CLR representation (every func-hierarchy reference is a
/// <see cref="System.Delegate"/>) is too coarse to tell structurally-distinct reference types apart.
/// </summary>
[TestClass]
public class ReferenceLinkingTests
{
    // An exporting module: a single func type ($t = () -> ()) plus a global of a concrete nullable reference
    // type (ref null $t), initialized with ref.null $t and exported under the name "g".
    static object ExportedGlobalInstance(bool mutable)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // $t = () -> (), index 0
        module.Globals.Add(new Global
        {
            ContentRefType = new ReferenceType(0u, isNullable: true),
            IsMutable = mutable,
            InitializerExpression = new Instruction[]
            {
                new RefNull { TypeIndex = 0 },
                new End(),
            },
        });
        module.Exports.Add(new Export { Name = "g", Kind = ExternalKind.Global, Index = 0 });
        return module.ToInstance<object>().Exports;
    }

    // An importing module that declares (but does not use) a single imported global of the given type/mutability.
    static Module ImportGlobalModule(Import.Global import)
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType()); // $t = () -> (), index 0
        module.Imports.Add(import);
        return module;
    }

    static ImportDictionary FromExports(object exports)
    {
        var imports = new ImportDictionary();
        imports.AddFromExports("M", exports);
        return imports;
    }

    /// <summary>
    /// An immutable <c>(ref null $t)</c> global export satisfies an import of the identical <c>(ref null $t)</c>
    /// type — the descriptor comparison accepts it even though the coarse CLR check reports both as a bare funcref.
    /// </summary>
    [TestMethod]
    public void GlobalImport_ImmutableExactReference_Succeeds()
    {
        var module = ImportGlobalModule(new Import.Global
        {
            Module = "M",
            Field = "g",
            ContentRefType = new ReferenceType(0u, isNullable: true),
            IsMutable = false,
        });

        Assert.IsNotNull(module.ToInstance<object>(FromExports(ExportedGlobalInstance(mutable: false))));
    }

    /// <summary>
    /// An immutable concrete <c>(ref null $t)</c> global export satisfies an abstract <c>funcref</c> import: the
    /// concrete function type is a subtype of the abstract func heap type.
    /// </summary>
    [TestMethod]
    public void GlobalImport_ImmutableConcreteAsFuncref_Succeeds()
    {
        var module = ImportGlobalModule(new Import.Global
        {
            Module = "M",
            Field = "g",
            ContentType = WebAssemblyValueType.FuncRef,
            IsMutable = false,
        });

        Assert.IsNotNull(module.ToInstance<object>(FromExports(ExportedGlobalInstance(mutable: false))));
    }

    /// <summary>
    /// A mutable global's type is invariant: a <c>(mut (ref null $t))</c> export does not satisfy a
    /// <c>(mut funcref)</c> import even though the concrete type is a subtype, because mutability forbids widening.
    /// </summary>
    [TestMethod]
    public void GlobalImport_MutableConcreteAsFuncref_Throws()
    {
        var module = ImportGlobalModule(new Import.Global
        {
            Module = "M",
            Field = "g",
            ContentType = WebAssemblyValueType.FuncRef,
            IsMutable = true,
        });

        Assert.ThrowsExactly<ImportException>(
            () => module.ToInstance<object>(FromExports(ExportedGlobalInstance(mutable: true))));
    }

    /// <summary>
    /// A host-supplied global carries no descriptor, so linking falls back to the coarse CLR value-type check:
    /// a delegate-returning getter satisfies a <c>funcref</c> import exactly as before descriptors existed.
    /// </summary>
    [TestMethod]
    public void GlobalImport_HostReferenceFallback_Succeeds()
    {
        var module = ImportGlobalModule(new Import.Global
        {
            Module = "M",
            Field = "g",
            ContentType = WebAssemblyValueType.FuncRef,
            IsMutable = false,
        });

        System.Action hostFunc = () => { };
        var imports = new ImportDictionary
        {
            { "M", "g", new GlobalImport((System.Func<System.Delegate>)(() => hostFunc)) },
        };

        Assert.IsNotNull(module.ToInstance<object>(imports));
    }

    // An exporting module whose immutable global "g" is a nullable structref (an abstract GC heap type), initialized
    // with ref.null struct.
    static object ExportedStructRefGlobalInstance()
    {
        var module = new Module();
        module.Globals.Add(new Global
        {
            ContentRefType = new ReferenceType(WebAssemblyHeapType.Struct, isNullable: true),
            IsMutable = false,
            InitializerExpression = new Instruction[]
            {
                new RefNull(WebAssemblyHeapType.Struct),
                new End(),
            },
        });
        module.Exports.Add(new Export { Name = "g", Kind = ExternalKind.Global, Index = 0 });
        return module.ToInstance<object>().Exports;
    }

    /// <summary>
    /// An immutable <c>structref</c> global export satisfies an abstract <c>anyref</c> import: <c>struct</c> is a
    /// subtype of <c>any</c> in the GC hierarchy, exercising the abstract-heap arm of the descriptor subtype check.
    /// </summary>
    [TestMethod]
    public void GlobalImport_ImmutableStructAsAnyref_Succeeds()
    {
        var module = new Module();
        module.Imports.Add(new Import.Global
        {
            Module = "M",
            Field = "g",
            ContentRefType = new ReferenceType(WebAssemblyHeapType.Any, isNullable: true),
            IsMutable = false,
        });

        Assert.IsNotNull(module.ToInstance<object>(FromExports(ExportedStructRefGlobalInstance())));
    }

    // An exporting module that defines and exports an (unpopulated) table "t" whose element type is a concrete
    // function reference (ref null $0), where $0 is the given signature.
    static object ExportedTypedFuncTableInstance(WebAssemblyType signature)
    {
        var module = new Module();
        module.Types.Add(signature); // $0
        var table = new Table(1, 1) { ElementRefType = new ReferenceType(0u, isNullable: true) };
        module.Tables.Add(table);
        module.Exports.Add(new Export("t", 0, ExternalKind.Table));
        return module.ToInstance<object>().Exports;
    }

    /// <summary>
    /// A table whose concrete function-reference element type differs structurally from the import's declared element
    /// type is rejected — the closed descriptor comparison (invariant for tables) sees the difference the coarse CLR
    /// <see cref="FunctionTable"/> check cannot.
    /// </summary>
    [TestMethod]
    public void TableImport_ElementDescriptorMismatch_Throws()
    {
        // Provider element (ref null (i32) -> ()) versus importer element (ref null (i64) -> ()).
        var importer = new Module();
        importer.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64] }); // $0 = (i64) -> ()
        var def = new Table(1, 1) { ElementRefType = new ReferenceType(0u, isNullable: true) };
        importer.Imports.Add(new Import.Table("M", "t", def));

        var provider = ExportedTypedFuncTableInstance(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32] });
        Assert.ThrowsExactly<ImportException>(() => importer.ToInstance<object>(FromExports(provider)));
    }
}
