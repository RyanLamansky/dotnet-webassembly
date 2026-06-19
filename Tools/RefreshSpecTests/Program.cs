using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// =========================================================================================
// Spec test refresher
//
// Regenerates WebAssembly.Tests/Runtime/SpecTestData by running wast2json (from wabt) over a
// pinned snapshot of the upstream WebAssembly spec test suite. The resulting JSON+wasm files
// drive WebAssembly.Tests/Runtime/SpecTests.cs.
//
// Scope: ratified WASM 2.0. We deliberately exclude proposals (anything not yet merged into
// a published WebAssembly version), since this library doesn't aim to track in-flight proposals.
// "Proposal" here means: in the WebAssembly proposals pipeline (https://github.com/WebAssembly/proposals)
// but not yet in the ratified spec.
//
// SpecCommit / WabtVersion compatibility window
// ----------------------------------------------
// These two pins are NOT independent. wabt has to be new enough to parse every .wast syntax
// the spec commit uses; the spec commit's scope determines which features end up in the
// regenerated test data.
//
// As of this writing, WASM 3.0 is in development on upstream's wasm-3.0 branch, which is
// periodically merged into main. After such a merge, top-level files like memory.wast /
// table.wast / ref_null.wast contain inline 3.0 syntax (GC, typed-fn-refs, etc.). The
// current SpecCommit is the parent of the most recent wasm-3.0 → main merge (2024-06-03),
// keeping the test data scoped to ratified WASM 2.0.
//
// Two reasons to bump SpecCommit:
//   (a) Pick up 2.0-era refinements upstream has made since this pin. Choose the parent of
//       the next-newer wasm-3.0 → main merge to stay 2.0-clean.
//   (b) Intentionally adopt a newly-ratified WASM version (e.g., when 3.0 ships and this
//       library is ready for it). Pick a recent main-branch commit, bump WabtVersion to a
//       release that parses the new syntax, and update proposalSubdirs / proposalFileStems
//       below to reflect whatever proposals remain unratified at that time.
// Either case ends with editing WebAssembly.Tests/Runtime/SpecTests.cs by hand to match
// added/removed/renamed categories — that file is curated, not generated.
//
// Maintenance procedure
// ---------------------
// 1. Update SpecCommit and WabtVersion per the guidance above.
// 2. Run: dotnet run --project Tools/RefreshSpecTests
// 3. Review the SpecTestData/ diff. Sync SpecTests.cs to match.
// 4. Re-run the test project; expect to re-curate per-line skip predicates if upstream
//    edits shifted line numbers.
// =========================================================================================

const string SpecCommit = "debdda8c1809194fe1e83e5f14f26bed614f8ed6"; // 2024-04-30, parent of wasm-3.0 merge
const string SpecRepoUrl = "https://github.com/WebAssembly/spec.git";
const string WabtVersion = "1.0.40";

// Subdirectories under test/core/ that contain only post-2.0 proposal tests.
// Note: bulk-memory/ and simd/ are deliberately NOT in this list — both are part of ratified WASM 2.0,
// even though upstream files them under proposal-style subdirectories. (At the current SpecCommit,
// bulk-memory tests are still at top level; simd/ is a subdir that we recurse into.)
string[] proposalSubdirs = ["exceptions", "gc", "memory64", "multi-memory", "relaxed-simd"];

// Memory64 tests that lived at top level at the pinned commit (before upstream relocated them
// to a memory64/ subdirectory). Listed explicitly rather than via a "*64" pattern because that
// pattern would also match f64/i64, which are core 1.0 type tests we want to keep.
string[] proposalFileStems = ["address64", "align64", "endianness64", "float_memory64", "load64",
    "memory64", "memory_grow64", "memory_redundancy64", "memory_trap64"];

var repoRoot = FindRepoRoot();
var destRoot = Path.Combine(repoRoot, "WebAssembly.Tests", "Runtime", "SpecTestData");

EnsureOnPath("git");

var workDir = Path.Combine(Path.GetTempPath(), "wasm-spec-refresh");
Directory.CreateDirectory(workDir);

var wast2json = await EnsureWabt(workDir, WabtVersion);
Console.WriteLine($"wast2json: {wast2json}");

var specCheckout = Path.Combine(workDir, "spec");
Console.WriteLine($"spec: {specCheckout}");

if (!Directory.Exists(Path.Combine(specCheckout, ".git")))
{
    Console.WriteLine($"Cloning {SpecRepoUrl}");
    Run("git", ["clone", "--no-checkout", "--filter=blob:none", SpecRepoUrl, specCheckout]);
}

Console.WriteLine($"Fetching {SpecCommit[..12]}");
Run("git", ["-C", specCheckout, "fetch", "--depth", "1", "origin", SpecCommit]);
Run("git", ["-C", specCheckout, "checkout", "--force", SpecCommit]);

var sourceRoot = Path.Combine(specCheckout, "test", "core");
if (!Directory.Exists(sourceRoot))
    throw new InvalidOperationException($"Spec test directory not found: {sourceRoot}");

// Wipe-then-regenerate so files removed/renamed upstream actually disappear locally.
if (Directory.Exists(destRoot))
{
    Console.WriteLine($"Clearing {destRoot}");
    Directory.Delete(destRoot, recursive: true);
}
Directory.CreateDirectory(destRoot);

var wastFiles = Directory.EnumerateFiles(sourceRoot, "*.wast", SearchOption.AllDirectories)
    .Where(f =>
    {
        var top = Path.GetRelativePath(sourceRoot, f).Split(Path.DirectorySeparatorChar)[0];
        if (proposalSubdirs.Contains(top)) return false;
        if (proposalFileStems.Contains(Path.GetFileNameWithoutExtension(f))) return false;
        return true;
    })
    .OrderBy(p => p, StringComparer.Ordinal)
    .ToList();
Console.WriteLine($"Converting {wastFiles.Count} .wast files (excluding proposal subdirs and memory64 *_64 files)");

var failures = new List<(string Path, string Error)>();
foreach (var wast in wastFiles)
{
    var rel = Path.GetRelativePath(sourceRoot, wast);
    var stem = Path.GetFileNameWithoutExtension(rel);
    var relDir = Path.GetDirectoryName(rel) ?? "";
    var outDir = Path.Combine(destRoot, relDir, stem);
    Directory.CreateDirectory(outDir);
    var outJson = Path.Combine(outDir, $"{stem}.json");

    // We enable wabt's individual proposal features rather than wast2json's defaults: many ratified-feature
    // .wast files (br_if, exports, linking, elem, ...) embed individual proposal-syntax test cases alongside
    // their core ones, and without these the entire file fails to parse over a single proposal assert; we'd
    // lose the 95% that's ratified content. The proposal-syntax cases that come along for the ride are
    // filtered later, per-line, in SpecTests.cs skip predicates.
    //
    // This is the set enabled by --enable-all MINUS --enable-compact-imports: that experimental wabt feature
    // emits a non-standard "compact" import section (a single module name shared across a run of imports)
    // that is not part of any WebAssembly standard and that no conformant runtime reads. --enable-all turns
    // it on, which made table_init/table_copy/bulk fixtures unreadable by the library. Omitting it keeps the
    // import section in the standard, one-name-pair-per-import encoding.
    string[] enableFlags =
    [
        "--enable-exceptions", "--enable-threads", "--enable-function-references", "--enable-tail-call",
        "--enable-annotations", "--enable-code-metadata", "--enable-gc", "--enable-memory64",
        "--enable-multi-memory", "--enable-extended-const", "--enable-relaxed-simd",
        "--enable-custom-page-sizes", "--enable-wide-arithmetic",
    ];
    var (exit, _, stderr) = TryRun(wast2json, [.. enableFlags, wast, "-o", outJson]);
    if (exit == 0)
    {
        Console.WriteLine($"  ok  {rel}");
    }
    else
    {
        Console.WriteLine($"  err {rel}");
        failures.Add((rel, stderr.Trim()));
        Directory.Delete(outDir, recursive: true);
    }
}

// Prune any directories left empty by failed conversions (e.g. all children of gc/ failing).
foreach (var dir in Directory.EnumerateDirectories(destRoot, "*", SearchOption.AllDirectories)
    .OrderByDescending(d => d.Length))
{
    if (!Directory.EnumerateFileSystemEntries(dir).Any())
        Directory.Delete(dir);
}

Console.WriteLine();
Console.WriteLine($"Succeeded: {wastFiles.Count - failures.Count}");
Console.WriteLine($"Failed:    {failures.Count}");
foreach (var (path, error) in failures)
    Console.WriteLine($"  {path}: {error}");

return failures.Count == 0 ? 0 : 1;

// Anchored to this source file via CallerFilePath so the tool resolves the repo root the same way
// regardless of cwd (dotnet run, the IDE, or the published binary).
static string FindRepoRoot([CallerFilePath] string thisFile = "")
    => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));

static async Task<string> EnsureWabt(string workDir, string version)
{
    var (osPart, archPart, exeExt) = (
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : throw new PlatformNotSupportedException("Unsupported OS for wabt prebuilt binaries."),
        RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            var a => throw new PlatformNotSupportedException($"Unsupported architecture for wabt prebuilt binaries: {a}"),
        },
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""
    );

    var assetName = $"wabt-{version}-{osPart}-{archPart}.tar.gz";
    var extractDir = Path.Combine(workDir, $"wabt-{version}");
    var binary = Path.Combine(extractDir, $"wabt-{version}", "bin", $"wast2json{exeExt}");
    if (File.Exists(binary)) return binary;

    Directory.CreateDirectory(extractDir);
    var tarball = Path.Combine(workDir, assetName);
    if (!File.Exists(tarball))
    {
        var url = $"https://github.com/WebAssembly/wabt/releases/download/{version}/{assetName}";
        Console.WriteLine($"Downloading {url}");
        using var http = new HttpClient();
        await using var src = await http.GetStreamAsync(url);
        await using var dst = File.Create(tarball);
        await src.CopyToAsync(dst);
    }

    Console.WriteLine($"Extracting {assetName}");
    Run("tar", ["-xzf", tarball, "-C", extractDir]);
    if (!File.Exists(binary))
        throw new InvalidOperationException($"Expected wast2json at {binary} after extraction.");
    return binary;
}

static void EnsureOnPath(string tool)
{
    try { Run(tool, ["--version"], quiet: true); }
    catch { throw new InvalidOperationException($"Required tool not on PATH: {tool}"); }
}

static void Run(string file, string[] args, bool quiet = false)
{
    var (exit, stdout, stderr) = TryRun(file, args);
    if (exit != 0)
        throw new InvalidOperationException($"{file} {string.Join(' ', args)} failed (exit {exit}): {stderr}");
    if (!quiet && stdout.Length > 0) Console.Write(stdout);
}

static (int Exit, string Stdout, string Stderr) TryRun(string file, string[] args)
{
    var psi = new ProcessStartInfo(file)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var a in args) psi.ArgumentList.Add(a);
    using var p = Process.Start(psi)!;
    var stdoutTask = p.StandardOutput.ReadToEndAsync();
    var stderrTask = p.StandardError.ReadToEndAsync();
    p.WaitForExit();
    return (p.ExitCode, stdoutTask.Result, stderrTask.Result);
}
