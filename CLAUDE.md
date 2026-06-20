# CLAUDE.md

Guidance for working in this repository.
Keep it accurate — update it when the layout, commands, or conventions change.

Personal, machine-specific preferences can go in `CLAUDE.local.md` (gitignored, optional) — it's imported here:

@CLAUDE.local.md

## What this project is

**WebAssembly for .NET** — a pure-.NET library (NuGet package `WebAssembly`) to create, read, modify, write, and execute WebAssembly (`.wasm`) binaries, plus convert them to .NET DLLs.
Execution is not an interpreter: WASM instructions are emitted as .NET IL via `System.Reflection.Emit` and JIT-compiled to native code by the CLR.

- Execution currently targets WASM **1.0** (spec compliance is good but imperfect).
  The spec **test data** covers ratified WASM 2.0; post-2.0 proposals aren't covered yet.
- `WebAssembly.Module` is the object-model root for reading, writing, and modifying.
  It exposes typed section collections — `Types`, `Imports`, `Functions`, `Tables`, `Memories`, `Globals`, `Exports`, `Elements`, `Codes`, `Data`, `CustomSections` — with `ReadFromBinary()` / `WriteToBinary()` for serialization.
- `WebAssembly.Runtime.Compile` drives execution and the WASM→DLL path.
  `Compile.FromBinary<TExports>(...)` takes an abstract class whose members map to WASM exports plus an `ImportDictionary`, and returns an `InstanceCreator<TExports>` factory.
  `Compile.CreatePersistedAssembly(...)` is the experimental .NET 9+ path that emits a DLL instead (via `PersistedAssemblyBuilder`).
- Imports are supplied through `ImportDictionary`, keyed by module/field name: `FunctionImport` (wraps a delegate), `MemoryImport`, `GlobalImport`, `TableImport`.

## Layout

| Path | What it holds |
|------|---------------|
| `WebAssembly/` | The library. Top-level types are the WASM object model (`Module.cs`, `Function.cs`, `Export.cs`, `OpCode.cs`, …). |
| `WebAssembly/Instructions/` | One file per instruction, each a subclass of `Instruction`. |
| `WebAssembly/Runtime/` | Execution: `Compile.cs`, configuration, runtime exceptions, import types, `UnmanagedMemory`. |
| `WebAssembly/Runtime/Compilation/` | The IL-emission engine: `CompilationContext`, `BlockStack`, `Signature`, `HelperMethod`, IL extensions. |
| `WebAssembly.Tests/` | MSTest test project. Mirrors the library: `Instructions/`, `Runtime/`. |
| `WebAssembly.Tests/Runtime/SpecTestData/` | **Generated** spec-suite fixtures (`.wasm` + `.json`). Do not hand-edit. |
| `Tools/RefreshSpecTests/` | Tool that regenerates `SpecTestData/` from the upstream WebAssembly spec suite. |
| `Examples/` | Standalone runnable samples (`RunExisting`, `GenerateClassFromWasm`, `ReadMeSample`). |
| `docs/BreakingChanges.md` | Breaking-change log. |

## Build & test

The solution `WebAssembly.sln` is the build root.
Requires the .NET 8, 9, and 10 SDKs.

```bash
dotnet build                       # build everything (Debug)
dotnet build -c Release            # Release can differ — see note below
dotnet test                        # run the full MSTest suite
dotnet test -c Release
dotnet test --filter "FullyQualifiedName~SpecTest_address"   # one test / class
```

CI (`.github/workflows/dotnetcore.yml`) builds **and** tests in **both Debug and Release**, because conditional compilation makes them genuinely different.
If you change anything platform- or `#if`-conditional, validate both configurations locally before assuming green.

Target frameworks:
- Library `WebAssembly.csproj`: `net8.0;net9.0` (net8.0 is the LTS baseline; .NET Standard 2.0 was dropped in 2.0.0).
- Tests: `net8.0;net9.0;net10.0`.

## Conventions

- `TreatWarningsAsErrors=True` on the library and tests.
  A warning fails the build.
  `AnalysisMode=Recommended` with `EnforceCodeStyleInBuild=true`.
- `Nullable` is enabled everywhere.
  Honor nullability; don't paper over it with `!` unless there's a real invariant.
- **Public API requires XML doc comments** (`GenerateDocumentationFile`).
  Every public type/member needs `///` docs, matching the existing terse style (see any file in `Instructions/`).
- Code style (`.editorconfig`): file-scoped namespaces, `var` preferred, expression-bodied members preferred, explicit accessibility modifiers, pattern matching preferred.
  C# 14 / `LangVersion=14.0`.
- The assembly is strong-name signed (`WebAssembly.snk`); `InternalsVisibleTo` exposes internals to the test project, so tests can and do reach `internal` members.
- Multi-targeting is now just net8 vs net9: guard net9-only APIs with `#if NET9_0_OR_GREATER` and provide a net8 fallback.
  The only remaining conditional symbols are `NET9_0_OR_GREATER` and `DEBUG`; the old netstandard2.0/netcoreapp3.0 fallbacks were removed when .NET Standard 2.0 was dropped.

## How instructions work (the core pattern)

Each opcode is a class in `WebAssembly/Instructions/` deriving from `Instruction` (often via `SimpleInstruction` / `BlockTypeInstruction`).
A new or modified instruction touches several coordinated places:

1. The instruction class itself: exposes `OpCode`, implements `WriteTo(Writer)` (binary emission), `Compile(CompilationContext)` (IL emission, including `context.ValidateStack(...)`), `Equals`, and a `reader`-based constructor if it has immediates.
2. `WebAssembly/OpCode.cs` (and `MiscellaneousOpCode.cs` for `0xFC`-prefixed ops) — the opcode enum value.
3. `WebAssembly/Instruction.cs` — the binary **parse dispatch** `switch (opCode)` that maps a byte to `new SomeInstruction(reader)`.
   Both the initializer-expression parser and the general parser live here.
4. Tests in `WebAssembly.Tests/Instructions/` — there's a test file per instruction; follow the neighbors.
   Shared base classes cut the boilerplate: `CompilerTestBase` (and its arity variants), `ComparisonTestBase`, `ConversionTestBase`, `MemoryReadTestBase` / `MemoryWriteTestBase`.

When emitting IL by hand, study `CompilationContext` and `ILGeneratorExtensions` first.
Stack validation is mandatory and the spec tests will catch mismatches.

## Spec tests

`WebAssembly.Tests/Runtime/SpecTests.cs` is **hand-curated**, one `[TestMethod]` per spec category, each calling `SpecTestRunner.Run(...)` with a set of per-line `skips`.
Each skip has a comment explaining *why* a line is skipped (a known limitation or an unimplemented assert harness).
When you fix a limitation, remove the now-passing skips.

`SpecTestData/` is **generated** by `Tools/RefreshSpecTests`.
To refresh (rarely needed):

```bash
dotnet run --project Tools/RefreshSpecTests
```

It downloads a pinned `wabt` release, checks out a pinned spec commit, runs `wast2json`, and rewrites `SpecTestData/`.
The pins (`SpecCommit`, `WabtVersion`) and the rationale for the current ratified-2.0 scope are documented at length in `Tools/RefreshSpecTests/Program.cs` — read that header before bumping anything.
After a refresh you must re-curate `SpecTests.cs` by hand (line numbers and categories shift).

## Gotchas

- Don't edit anything under `bin/`, `obj/`, `TestResults/`, or `SpecTestData/` — all generated.
- The library has no runtime third-party dependencies at all (only SourceLink, for packaging). The former netstandard2.0-only `System.Reflection.Emit` package reference was removed with the .NET Standard 2.0 target.
