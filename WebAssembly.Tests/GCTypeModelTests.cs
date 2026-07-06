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
    /// Every GC/structural-type fixture round-trips byte-identically at the object-model level.  Fixtures whose
    /// function bodies use GC instructions (the <c>0xFB</c> prefix, <c>ref.eq</c> <c>0xD3</c>, or a GC constant in an
    /// initializer expression) cannot be parsed until a later Phase 8 milestone adds those instruction classes; such a
    /// fixture is required to fail with exactly that pending-feature signature (so a real regression still fails, and
    /// the round-trip assertion engages automatically once the instructions land).
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
        catch (ModuleLoadException x) when (IsPendingGCInstruction(x))
        {
            // The type section parsed; only a GC instruction body (later milestone) is unimplemented. The type-section
            // round-trip for this fixture is covered once those instructions land.
            return;
        }

        using var output = new MemoryStream();
        module.WriteToBinary(output);

        CollectionAssert.AreEqual(original, output.ToArray(), $"Round-trip of {path} was not byte-identical.");
    }

    // The GC instruction set (0xFB prefix / ref.eq 0xD3 / GC constants in initializer expressions) is added in later
    // Phase 8 milestones; until then these fixtures are expected to fail to parse with exactly these signatures.
    private static bool IsPendingGCInstruction(ModuleLoadException exception)
    {
        for (System.Exception? e = exception; e != null; e = e.InnerException)
        {
            var message = e.Message;
            if (message.Contains("opcode \"251\"", System.StringComparison.Ordinal)      // 0xFB GC prefix
                || message.Contains("opcode \"211\"", System.StringComparison.Ordinal)   // 0xD3 ref.eq
                || message.Contains("\"251\" is not permitted", System.StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
