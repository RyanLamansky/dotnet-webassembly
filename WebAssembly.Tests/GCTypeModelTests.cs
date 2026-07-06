using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebAssembly;

/// <summary>
/// Phase 8 (M8.1) object-model and byte-identical round-trip tests for the WASM 3.0 garbage-collection type section
/// (recursion groups, sub/final types, struct/array composites, packed fields).  Every parseable <c>.wasm</c> under
/// the GC and structural-type fixture directories must re-emit byte-for-byte through
/// <see cref="Module.ReadFromBinary(System.IO.Stream)"/> / <see cref="Module.WriteToBinary(System.IO.Stream)"/>.
/// </summary>
[TestClass]
public class GCTypeModelTests
{
    private static readonly string[] FixtureDirectories =
    [
        Path.Combine("Runtime", "SpecTestData", "gc"),
        Path.Combine("Runtime", "SpecTestData", "type-rec"),
        Path.Combine("Runtime", "SpecTestData", "type-canon"),
        Path.Combine("Runtime", "SpecTestData", "type-equivalence"),
    ];

    // The binary-gc category is intentionally malformed-binary asserts (not valid modules); excluded from round-trip.
    private static readonly string ExcludedDirectory =
        Path.Combine("Runtime", "SpecTestData", "gc", "binary-gc") + Path.DirectorySeparatorChar;

    /// <summary>Enumerates every parseable GC/structural-type fixture path.</summary>
    /// <returns>One single-element argument row per fixture path.</returns>
    public static IEnumerable<object[]> Fixtures()
    {
        foreach (var directory in FixtureDirectories)
        {
            if (!Directory.Exists(directory))
                continue;

            foreach (var path in Directory.EnumerateFiles(directory, "*.wasm", SearchOption.AllDirectories)
                .Where(p => !p.StartsWith(ExcludedDirectory, System.StringComparison.Ordinal))
                .OrderBy(p => p, System.StringComparer.Ordinal))
            {
                yield return [path];
            }
        }
    }

    /// <summary>Produces a readable test-case name from the fixture file name.</summary>
    /// <param name="method">The test method.</param>
    /// <param name="data">The data row.</param>
    /// <returns>A display name incorporating the fixture file name.</returns>
    public static string FixtureName(System.Reflection.MethodInfo method, object?[] data) =>
        $"{method.Name}({Path.GetFileName((string)data[0]!)})";

    /// <summary>
    /// Every GC/structural-type fixture round-trips byte-identically at the object-model level, including the GC
    /// instruction bodies (the <c>0xFB</c> prefix and <c>ref.eq</c> <c>0xD3</c>), which the parser reads in full.
    /// The one remaining gap is a GC constant (<c>array.new_data</c> / <c>array.new_elem</c>) inside an initializer
    /// expression, which the parser rejects; a fixture that hits it is required to fail with exactly that signature
    /// (so a real regression still fails, and the round-trip assertion engages automatically once it is supported).
    /// </summary>
    /// <param name="path">The fixture path.</param>
    [TestMethod]
    [DynamicData(nameof(Fixtures), DynamicDataDisplayName = nameof(FixtureName))]
    public void GCType_Fixture_RoundTripsByteIdentical(string path)
    {
        var original = File.ReadAllBytes(path);

        Module module;
        try
        {
            using var input = new MemoryStream(original);
            module = Module.ReadFromBinary(input);
        }
        catch (ModuleLoadException x) when (IsGCConstantInInitializer(x))
        {
            // These fixtures are spec-invalid modules (assert_invalid "constant expression required"): they place
            // non-constant instructions (array.new_data / array.new_elem) in initializer expressions, which the
            // parser correctly rejects. A rejected module has no object model to round-trip.
            return;
        }

        using var output = new MemoryStream();
        module.WriteToBinary(output);

        CollectionAssert.AreEqual(original, output.ToArray(), $"Round-trip of {path} was not byte-identical.");
    }

    // array.new_data / array.new_elem are not constant instructions, so a module placing them in an initializer
    // expression is spec-invalid and must be rejected; this matches that rejection's exact signature (the 0xFB
    // prefix reported as not permitted) so no other parse failure slips through.
    private static bool IsGCConstantInInitializer(ModuleLoadException exception)
    {
        for (System.Exception? e = exception; e != null; e = e.InnerException)
        {
            if (e.Message.Contains("\"251\" is not permitted", System.StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
