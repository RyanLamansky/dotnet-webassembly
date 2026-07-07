# CLAUDE.md

Guidance for working in this repository.
Keep it accurate — update it when the layout, commands, or conventions change.

Personal, machine-specific preferences can go in `CLAUDE.local.md` (gitignored, optional) — it's imported here:

@CLAUDE.local.md

## What this project is

**WebAssembly for .NET** — a pure-.NET library (NuGet package `WebAssembly`) to create, read, modify, write, and execute WebAssembly (`.wasm`) binaries, plus convert them to .NET DLLs.
Execution is not an interpreter: WASM instructions are emitted as .NET IL via `System.Reflection.Emit` and JIT-compiled to native code by the CLR.

- Execution targets ratified WASM **3.0** (released 2025-09-17) — extended constant expressions, tail calls, multiple memories, 64-bit address space (memory64 / table64), typed function references, exception handling (the `try_table` model), relaxed SIMD (deterministic profile), and garbage collection (struct / array / i31) — on top of the earlier 2.0 feature set (bulk memory, reference types, fixed-width SIMD, multi-value, typed `select`).
  Spec compliance is strong: every ratified-3.0 spec-suite category runs green.
  The only intentional exclusions are the seven `float_exprs` NaN-folding lines marked `unsupported` in `SpecTests.cs` (the JIT folds `x±0` / `x*1` so a signaling NaN is never quieted), and they keep the category green rather than deferring it.
  This library targets the **deterministic profile**: float operations follow IEEE-754 via the CLR, relaxed-SIMD instructions implement the spec's deterministic defaults, and NaN payload bit patterns are the only known divergence.
  Post-3.0 proposals (custom page sizes, wide arithmetic, JS string builtins, …) aren't covered.
- `WebAssembly.Module` is the object-model root for reading, writing, and modifying.
  It exposes typed section collections — `Types`, `Imports`, `Functions`, `Tables`, `Memories`, `Globals`, `Exports`, `Elements`, `Codes`, `Data`, `CustomSections` — with `ReadFromBinary()` / `WriteToBinary()` for serialization.
- `WebAssembly.Runtime.Compile` drives execution and the WASM→DLL path.
  `Compile.FromBinary<TExports>(...)` takes an abstract class whose members map to WASM exports plus an `ImportDictionary`, and returns an `InstanceCreator<TExports>` factory.
  `Compile.CreatePersistedAssembly(...)` is the .NET 9+ path that emits a DLL instead (via `PersistedAssemblyBuilder`); it shares the compiler with in-memory execution, and the full spec suite runs green through it (`PersistedSpecTests.cs`).
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
| `docs/` | `BreakingChanges.md`, the breaking-change log. |

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
- Library `WebAssembly.csproj`: `net8.0;net9.0` (net8.0 is the oldest supported baseline; .NET Standard 2.0 was dropped in 2.0.0; net8/net9 both leave support around Nov 2026).
  A net10.0 target is added only when the code actually uses a .NET 10 API behind `#if NET10_0_OR_GREATER` — an extra target with no conditional code is identical IL and pure package weight (net10 runtimes get the net9 asset and all the JIT uplift regardless).
- Tests: `net8.0;net9.0;net10.0` (the net10 cell exercises the newest-asset-on-newest-runtime pairing users actually get).

## Conventions

- `TreatWarningsAsErrors=True` on the library and tests.
  A warning fails the build.
  `AnalysisMode=Recommended` with `EnforceCodeStyleInBuild=true`.
- `Nullable` is enabled everywhere.
  Honor nullability; don't paper over it with `!` unless there's a real invariant.
- **Public API requires XML doc comments** (`GenerateDocumentationFile`).
  Every public type/member needs `///` docs, matching the existing terse style (see any file in `Instructions/`).
  Completeness is enforced, if some params are documented, all must be.
- Code style (`.editorconfig`): file-scoped namespaces, `var` preferred, expression-bodied members preferred, explicit accessibility modifiers, pattern matching preferred.
  C# 14 / `LangVersion=14.0`.
- The assembly is strong-name signed (`WebAssembly.snk`), but there is **no** `InternalsVisibleTo`, so the test project sees only the public API.
  When a test genuinely needs an `internal` helper, the csproj links that source file directly (e.g. `<Compile Include="..\WebAssembly\RegeneratingWeakReference.cs">`); internal types like `Signature`, `Reader`, and `Writer` are otherwise unreachable from tests.
- Multi-targeting has one conditional split, net8 vs net9+: guard net9-plus APIs with `#if NET9_0_OR_GREATER` (which is also true on net10) and provide a net8 fallback.
  The only remaining conditional symbols are `NET9_0_OR_GREATER` and `DEBUG`; the old netstandard2.0/netcoreapp3.0 fallbacks were removed when .NET Standard 2.0 was dropped.
- Prefer `Module` when creating unit tests against well-formed WASMs to preserve human readability.
  - No restrictions on using byte arrays, hex, or other WASM representations for throwaway work.

## How instructions work (the core pattern)

Each opcode is a class in `WebAssembly/Instructions/` deriving from `Instruction` (often via `SimpleInstruction` / `BlockTypeInstruction`).
A new or modified instruction touches several coordinated places:

1. The instruction class itself: exposes `OpCode`, implements `WriteTo(Writer)` (binary emission), `Compile(CompilationContext)` (IL emission, including `context.ValidateStack(...)`), `Equals`, and a `reader`-based constructor if it has immediates.
2. The opcode enum value: `WebAssembly/OpCode.cs` for single-byte ops, or the prefixed-op enum matching the prefix byte — `MiscellaneousOpCode.cs` (`0xFC`), `SimdOpCode.cs` (`0xFD`), or `GCOpCode.cs` (`0xFB`).
3. `WebAssembly/Instruction.cs` — the binary **parse dispatch** `switch (opCode)` that maps a byte to `new SomeInstruction(reader)`.
   Both parsers live here: the general parser and the restricted initializer-expression parser (`ParseInitializerExpression`).
   A constant-capable instruction (usable in a global / element / data offset initializer) needs a dispatch entry in **both**.
4. Tests in `WebAssembly.Tests/Instructions/` — there's a test file per instruction; follow the neighbors.
   Shared base classes cut the boilerplate: `CompilerTestBase` (and its arity variants), `ComparisonTestBase`, `ConversionTestBase`, `MemoryReadTestBase` / `MemoryWriteTestBase`.

When emitting IL by hand, study `CompilationContext` and `ILGeneratorExtensions` first.
Stack validation is mandatory and the spec tests will catch mismatches.

## Load-bearing runtime mechanisms (WASM 3.0)

The non-obvious design decisions behind the 3.0 features; each is easy to subtly break if you don't know why it exists.

- **Canonical type identity** — `Runtime/Compilation/CanonicalTypeMap` canonicalizes the type section once per compilation (iso-recursive, rec-group by rec-group, De Bruijn positions for intra-group references).
  Its `Descriptor` strings are module-independent, so string equality doubles as the **cross-module import-matching currency** for tags, globals, tables, and functions.
  `ExceptionTag` renders host-side descriptors through the shared `CanonicalTypeMap` helper — never render that grammar by hand; two independent renderers once drifted and silently broke all parameterized host-tag imports.
- **Funcref identity** — funcrefs are arity-shaped CLR `Delegate`s, so canonical function-type identity lives in a `ConditionalWeakTable` registry in `WebAssemblyGC`, populated wherever WASM-typed delegates are born (ref.func / element segments / globals / imports / export relinking).
  `call_indirect` and concrete-func-index casts consult it via tokens interned once at instantiation into the per-instance `FuncTypeTokens` array — never re-intern per call.
  Unregistered (host-created) delegates deliberately fall back to CLR-shape acceptance so embeddings don't break.
- **GC objects** — one emitted CLR class per *canonical* struct/array id, with declared WASM subtyping as CLR inheritance, so `ref.cast`/`ref.test` lower to `castclass`/`isinst`.
  Hierarchy: `WebAssemblyGCObject` (= the `eq` type) over `WebAssemblyStructObject` / `WebAssemblyArrayObject` / `WebAssemblyI31`.
  GC reference slots (stack, locals, fields) are uniformly `object` (`Delegate` for the func hierarchy); the concrete emitted class is applied only at use sites via `castclass`.
  `ref.eq` lowers to `WebAssemblyGC.RefEqual`, not `ceq`, because equal-payload `WebAssemblyI31` boxes must compare equal.
  Array types are wrapper classes (not bare CLR arrays) because element mutability is part of canonical identity.
- **Exceptions** — WASM exceptions are `WebAssemblyException`, and only compiled throw instructions create that type, so a single `isinst` filter cleanly separates WASM exceptions from CLR-native traps in `try_table` handlers.
  Tag identity is `ExceptionTag` reference identity in-process, plus canonical-descriptor matching for cross-module imports.
- **Tail calls** — the IL `tail.` prefix (guaranteed since .NET Core 3.0).
  ECMA-335 forbids `tail.` inside protected regions, so a `return_call*` inside a `try_table` spills its operands, `leave`s to a per-callsite trampoline emitted after the function's final `ret`, and tail-calls from there — preserving both catchability and constant stack.
- **memory64** — `UnmanagedMemory` stays under 4 GiB; 64-bit accesses compute the effective address in overflow-checked u64, so anything ≥ 2³² necessarily trap-checks rather than wrapping.
  Wide/narrow API pattern: `ulong` storage (`Offset64`, `Minimum64`, …) with the pre-existing `uint` members as `checked` narrowing views.

## Spec tests

`WebAssembly.Tests/Runtime/SpecTests.cs` is **hand-curated**, one `[TestMethod]` per spec category, each calling `SpecTestRunner.Run(...)` with optional per-line predicates.
Each carries a comment explaining *why* a line is excluded.
Every category also runs through the WASM→DLL path via `PersistedSpecTests.cs` (net9+ only): compile → save → load into a collectible `AssemblyLoadContext` → instantiate → execute, using `SpecTestRunner`'s `Persisted` execution mode.
Per-line curation is defined **once** and shared by both modes (see `SpecTests.Curation`) — never duplicate a line list into the persisted methods.
A line falls into one of two buckets:

- `skip` — a **deferral**: a fixable limitation or an unimplemented assert harness. A category with any `skip` line ends Inconclusive (shown as "Skipped"), signalling work that remains. When you fix the limitation, remove the now-passing skips — and re-check the whole set, since one fix often clears many stale skips at once.
- `unsupported` — **intentional avoidance**: a scenario that can't reasonably pass under the .NET execution model and isn't expected to (e.g. the JIT folding `x±0` / `x*1` so a signaling NaN is never quieted, in `float_exprs`). These are skipped like `skip` lines but do **not** make the category Inconclusive, because nothing is pending; the category stays green.

Scenarios that can *never* pass under the .NET execution model are also auto-skipped at the top of the `SpecTestRunner` command loop, recognized by command *shape* rather than line number — the shape-based counterpart to `unsupported`, for cases identifiable without a line list, so they stay out of the hand-curated lists and don't trip the Inconclusive signal.
The only such case today is `assert_exhaustion` "call stack exhausted", which requires an uncatchable `StackOverflowException` that would tear down the test host.
There are no whole-category `[Ignore]`s anymore — every category runs.

`SpecTestData/` is **generated** by `Tools/RefreshSpecTests`.
To refresh (rarely needed):

```bash
dotnet run --project Tools/RefreshSpecTests
```

It downloads a pinned `wasm-tools` release (`wabt` was retired — it can't parse the 3.0 GC / exception / script syntax), checks out a pinned spec commit, runs `wasm-tools json-from-wast`, and rewrites `SpecTestData/`.
The pins (`SpecCommit`, `WasmToolsVersion`) and the rationale for the current ratified-3.0 scope are documented at length in `Tools/RefreshSpecTests/Program.cs` — read that header before bumping anything.
After a refresh you must re-curate `SpecTests.cs` by hand (line numbers and categories shift).

## Gotchas

- Don't edit anything under `bin/`, `obj/`, `TestResults/`, or `SpecTestData/` — all generated.
- The library has no runtime third-party dependencies at all (only SourceLink, for packaging).
- **Anything the emitted assembly calls must be `public`**: generated code lives in a separate, non-friend, unsigned assembly (no `InternalsVisibleTo` on the strong-named library), so runtime helpers (`WebAssemblyGC`, the SIMD `Execute` methods, `ImportException.Validate*`, the GC object base classes) are public by necessity, not as invitation.
- **Adding an overload to a method resolved by `GetMethod(nameof(...))`** (e.g. `Compile`'s static `Validate*Import` fields) requires giving that resolution site an explicit parameter-type list, or every compilation dies at static-init with `AmbiguousMatchException` — and a plain library build won't catch it, only the test suite will.
- **Changing the signature of a public member that emitted IL calls** breaks persisted DLLs produced by earlier versions, whose call sites bake in the exact signature; add the new shape as an overload and keep the old one as a forwarder (see `ImportException.ValidateMemory`/`ValidateTable`).
- **Hot paths must not allocate**: `HotPathAllocationGuardTests` scans the IL of every SIMD `Execute` method and the per-call `WebAssemblyGC` helpers and fails on `newarr`/`box`/reference-type `newobj`; use `stackalloc` for temporary buffers.
- **Opcode-enum naming is locked by tests**: `OpCodeTests` derives each member name from its native WASM name for all four opcode enums; `simdReleasedExemptions` there documents the WASM 2.0-era deviations retained for binary compatibility (a candidate list for a future breaking-change release).
- When bumping the spec-test pins, `SpecCommit` and `WasmToolsVersion` move together — mismatches surface as per-file conversion failures in the refresh run.
