using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// =========================================================================================
// Spec test refresher
//
// Regenerates WebAssembly.Tests/Runtime/SpecTestData by running `wasm-tools json-from-wast`
// (from the Bytecode Alliance) over a pinned snapshot of the upstream WebAssembly spec test
// suite. The resulting JSON+wasm files drive WebAssembly.Tests/Runtime/SpecTests.cs.
//
// Scope: ratified WASM 3.0 (released 2025-09-17). WASM 3.0 folds a set of previously-separate
// proposals — extended const, tail calls, multiple memories, memory64/table64, typed function
// references, exception handling (try_table), relaxed SIMD, and GC — into one ratified version,
// so their former proposal subdirectories are now in scope. We still exclude anything that is
// NOT part of ratified 3.0 (custom page sizes, wide arithmetic, JS string builtins, …); those
// live in their own proposal repos rather than under this spec commit's test/core/, so nothing
// needs excluding at the current pin. "Proposal" here means: in the WebAssembly proposals
// pipeline (https://github.com/WebAssembly/proposals) but not yet in the ratified spec.
//
// Why wasm-tools instead of wabt
// ------------------------------
// wabt (even 1.0.41, 2026-05) cannot parse the 3.0 suite: GC composite types (struct/array,
// rec/sub groups), the new abstract heap types (anyref, exnref, …), the exn heap type used by
// try_table.wast, and the `(module definition …)` script syntax used by instance.wast and much
// of memory64/ all error out — wabt's README doesn't list GC among supported proposals at all.
// `wasm-tools json-from-wast` converts the whole suite (it is what wasmtime uses to run it) and
// has NO per-feature enable flags — it parses the full spec text by default — so the old
// wast2json --enable-* block (and its --enable-compact-imports hazard) is gone entirely.
//
// SpecCommit / WasmToolsVersion compatibility window
// --------------------------------------------------
// These two pins are NOT independent. wasm-tools has to be new enough to parse every .wast
// syntax the spec commit uses; the spec commit's scope determines which features end up in the
// regenerated test data. SpecCommit was pinned to the `wg-3.0` tag — the Working Group's
// ratified-3.0 snapshot (2025-09-26), which bounds scope to exactly-ratified 3.0 — and has since
// moved just past it to pick up upstream test-coverage improvements. Every test-suite commit between
// the tag and the current pin was verified to be a pure ratified-3.0 coverage addition (e.g.
// {data,elem}.drop) with zero post-3.0 feature leakage, so scope is unchanged. `main` has kept
// moving beyond this (post-3.0 drift), so stay at or below this pin unless a specific later upstream
// test fix is verified 3.0-clean. Verified 2026-07-06: wasm-tools v1.252.0 cleanly converts the
// suite at this pin.
//
// Two reasons to bump SpecCommit:
//   (a) Pick up 3.0-era refinements upstream has made since this pin. Prefer a later tag (or the
//       parent of the next post-3.0 proposal merge) to stay ratified-3.0-clean.
//   (b) Intentionally adopt a newly-ratified WASM version (e.g., a future 4.0). Pick the matching
//       tag/commit, bump WasmToolsVersion to a release that parses the new syntax, and update
//       proposalSubdirs / proposalFileStems below to reflect whatever proposals remain unratified.
// Either case ends with editing WebAssembly.Tests/Runtime/SpecTests.cs by hand to match
// added/removed/renamed categories — that file is curated, not generated.
//
// Note: the bulk-memory tests, once at top level, now live in a bulk-memory/ subdir upstream;
// they are ratified (2.0) and stay in scope — they're simply recursed into like simd/.
//
// Maintenance procedure
// ---------------------
// 1. Update SpecCommit and WasmToolsVersion per the guidance above.
// 2. Run: dotnet run --project Tools/RefreshSpecTests
// 3. Review the SpecTestData/ diff. Sync SpecTests.cs to match.
// 4. Re-run the test project; expect to re-curate per-line skip predicates if upstream
//    edits shifted line numbers.
// =========================================================================================

const string SpecCommit = "a05043b5afe432cac0f635bd7c5f20b6c9b325a5"; // 2026-05-26, just past wg-3.0 (ratified-3.0-clean test-coverage improvements)
const string SpecRepoUrl = "https://github.com/WebAssembly/spec.git";
const string WasmToolsVersion = "1.252.0";

// Subdirectories under test/core/ that contain only unratified-proposal tests. Empty at the
// current pin: every subdir present upstream at wg-3.0 (exceptions/, gc/, memory64/,
// multi-memory/, relaxed-simd/, bulk-memory/, simd/) is part of ratified 3.0 (or earlier) and in
// scope. Custom-page-sizes and wide-arithmetic — the remaining non-3.0 proposals — live in their
// own repos, not here. Re-add an entry if a future pin surfaces a genuinely-unratified subdir.
string[] proposalSubdirs = [];

// Individual file stems to exclude (unratified-proposal tests that share a directory with
// ratified content). Empty at the current pin — see proposalSubdirs. Re-add a targeted entry,
// with a comment, if regeneration surfaces a file that tests an unratified proposal.
string[] proposalFileStems = [];

var repoRoot = FindRepoRoot();
var destRoot = Path.Combine(repoRoot, "WebAssembly.Tests", "Runtime", "SpecTestData");

EnsureOnPath("git");

var workDir = Path.Combine(Path.GetTempPath(), "wasm-spec-refresh");
Directory.CreateDirectory(workDir);

var wasmTools = await EnsureWasmTools(workDir, WasmToolsVersion);
Console.WriteLine($"wasm-tools: {wasmTools}");

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
Console.WriteLine($"Converting {wastFiles.Count} .wast files");

var failures = new List<(string Path, string Error)>();
foreach (var wast in wastFiles)
{
    var rel = Path.GetRelativePath(sourceRoot, wast);
    var stem = Path.GetFileNameWithoutExtension(rel);
    var relDir = Path.GetDirectoryName(rel) ?? "";
    var outDir = Path.Combine(destRoot, relDir, stem);
    Directory.CreateDirectory(outDir);
    var outJson = Path.Combine(outDir, $"{stem}.json");

    // wasm-tools json-from-wast parses the full 3.0 spec text by default (no per-feature enable flags),
    // emits the JSON command list to --output, and writes the referenced .wasm/.wat modules into --wasm-dir.
    var (exit, _, stderr) = TryRun(wasmTools,
        ["json-from-wast", wast, "-o", outJson, "--wasm-dir", outDir]);
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

static async Task<string> EnsureWasmTools(string workDir, string version)
{
    // Bytecode Alliance release assets are named wasm-tools-<ver>-<arch>-<os>.tar.gz and extract to a
    // directory of the same stem containing the single `wasm-tools` binary (`.exe` on Windows).
    var (osPart, archPart, exeExt) = (
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : throw new PlatformNotSupportedException("Unsupported OS for wasm-tools prebuilt binaries."),
        RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "aarch64",
            var a => throw new PlatformNotSupportedException($"Unsupported architecture for wasm-tools prebuilt binaries: {a}"),
        },
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ""
    );

    var stem = $"wasm-tools-{version}-{archPart}-{osPart}";
    var assetName = $"{stem}.tar.gz";
    var extractDir = Path.Combine(workDir, stem);
    var binary = Path.Combine(extractDir, stem, $"wasm-tools{exeExt}");
    if (File.Exists(binary)) return binary;

    Directory.CreateDirectory(extractDir);
    var tarball = Path.Combine(workDir, assetName);
    if (!File.Exists(tarball))
    {
        var url = $"https://github.com/bytecodealliance/wasm-tools/releases/download/v{version}/{assetName}";
        Console.WriteLine($"Downloading {url}");
        using var http = new HttpClient();
        await using var src = await http.GetStreamAsync(url);
        await using var dst = File.Create(tarball);
        await src.CopyToAsync(dst);
    }

    Console.WriteLine($"Extracting {assetName}");
    Run("tar", ["-xzf", tarball, "-C", extractDir]);
    if (!File.Exists(binary))
        throw new InvalidOperationException($"Expected wasm-tools at {binary} after extraction.");
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
