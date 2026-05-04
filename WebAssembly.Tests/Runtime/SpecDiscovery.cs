using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebAssembly.Runtime;

/// <summary>
/// Maintenance helper that re-derives per-line skip predicates for <see cref="SpecTests"/>
/// after a spec test refresh. <see cref="DiscoverAllFailures"/> runs every category through
/// <see cref="SpecTestRunner.Discover"/> and writes the results to a file in the OS temp dir.
/// </summary>
/// <remarks>
/// Workflow when refreshing the spec test suite:
/// <list type="number">
/// <item>Run <c>Tools/RefreshSpecTests</c> to regenerate <c>SpecTestData/</c>.</item>
/// <item>Add or remove <see cref="IgnoreAttribute"/> on whole categories that gained or lost
/// .json files in the refresh.</item>
/// <item>Remove <see cref="IgnoreAttribute"/> from <see cref="DiscoverAllFailures"/> and run
/// only this test:
/// <c>dotnet test ... --filter FullyQualifiedName~SpecDiscovery</c>.
/// It walks <c>SpecTestData/</c>, runs every category through <see cref="SpecTestRunner.Discover"/>
/// (which collects all failures rather than throwing on the first), and writes a (line, message)
/// report to <c>{tempPath}/spec-discovery.txt</c>.</item>
/// <item>Use that report to author per-test skip predicates: one <c>HashSet&lt;uint&gt;</c> per
/// method body in <see cref="SpecTests"/>, with a comment summarizing the failure categories.</item>
/// <item>Restore the <see cref="IgnoreAttribute"/> here. The full SpecTests suite should be
/// green: every previously-failing category Inconclusive (some-skipped) or Passed.</item>
/// </list>
/// </remarks>
[TestClass]
public class SpecDiscovery
{
    // Categories whose tests crash the runner process (StackOverflowException, CLR malfunction,
    // JIT-internal-limitation faults). These would abort the entire test run if Discover invoked
    // them; SpecTests.cs marks them [Ignore] for the same reason. Re-evaluate after library work
    // that addresses the underlying crashes.
    private static readonly HashSet<string> categoryExcludes =
    [
        "labels", "skip-stack-guard-page", "unwind",
    ];

    /// <summary>Walks SpecTestData/ and writes a failure report to the OS temp dir.</summary>
    [TestMethod]
    //[Ignore("Maintenance helper. Remove [Ignore] manually when refreshing skip predicates.")]
    public void DiscoverAllFailures()
    {
        var dataRoot = Path.Combine("Runtime", "SpecTestData");
        // RefreshSpecTests lays out each category as <category>/<category>.json, so the JSON
        // stem matches the leaf directory name. Walking recursively picks up nested categories
        // like simd/* without requiring a hardcoded list.
        var jobs = Directory.EnumerateFiles(dataRoot, "*.json", SearchOption.AllDirectories)
            .Select(jsonPath => new
            {
                Category = Path.GetRelativePath(dataRoot, Path.GetDirectoryName(jsonPath)!),
                Dir = Path.GetDirectoryName(jsonPath)!,
                JsonName = Path.GetFileName(jsonPath),
            })
            .Where(j => !categoryExcludes.Contains(j.Category) && j.Category is "func" or "block" or "br" or "br_if" or "br_table" or "loop" or "if" or "call" or "global")
            .OrderBy(j => j.Category, StringComparer.Ordinal)
            .ToList();

        var outputPath = Path.Combine(Path.GetTempPath(), "spec-discovery.txt");
        using var sw = new StreamWriter(outputPath);
        sw.WriteLine($"# Spec discovery report ({jobs.Count} categories)");
        sw.WriteLine();

        foreach (var job in jobs)
        {
            List<(uint Line, string Message)> failures;
            try
            {
                failures = SpecTestRunner.Discover(job.Dir, job.JsonName);
            }
            catch (Exception ex)
            {
                sw.WriteLine($"## {job.Category} -- DISCOVER THREW: {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
                sw.WriteLine();
                continue;
            }

            sw.WriteLine($"## {job.Category}  ({failures.Count} failure(s))");
            foreach (var (line, message) in failures)
                sw.WriteLine($"{line}\t{message}");
            sw.WriteLine();
        }
    }
}
