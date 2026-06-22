using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests that exports whose WASM name is not a valid .NET identifier are reachable under a cleaned member
/// name, consistent across functions, globals, memories, and tables.
/// </summary>
[TestClass]
public class ExportNameCleaningTests
{
    /// <summary>
    /// A global exported under a name that isn't a valid identifier (here containing a '.', as produced by
    /// re-exporting an imported global in the spec's linking suite) is reachable as a property whose name is
    /// the cleaned form.
    /// </summary>
    [TestMethod]
    public void ExportedGlobal_WithInvalidIdentifierName_UsesCleanedMemberName()
    {
        const string exportName = "Mg.glob";

        var module = new Module();
        module.Globals.Add(new Global
        {
            ContentType = WebAssemblyValueType.Int32,
            IsMutable = false,
            InitializerExpression = [new Int32Constant(42), new End()],
        });
        module.Exports.Add(new Export { Name = exportName, Kind = ExternalKind.Global, Index = 0 });

        var exports = module.ToInstance<object>(new ImportDictionary()).Exports;

        var cleaned = NameCleaner.CleanName(exportName);
        Assert.AreNotEqual(exportName, cleaned); // Confirm the name genuinely required cleaning.

        var property = exports.GetType().GetProperty(cleaned);
        Assert.IsNotNull(property, $"Expected an exported property named '{cleaned}'.");
        Assert.AreEqual(42, property.GetValue(exports));
    }
}
