using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Phase 6 (M6.2) runtime tests for exception-tag identity, import, export, and cross-module linking.
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
}
