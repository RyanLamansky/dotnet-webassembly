# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build WebAssembly.sln

# Test (all frameworks, run sequentially to avoid timeout flakiness)
dotnet test --framework net8.0 && dotnet test --framework net9.0 && dotnet test --framework net10.0

# Run all frameworks at once (may see flaky Loop_Compiled / Branch_LoopValue timeouts under load)
dotnet test

# Run a specific test class
dotnet test --filter "ClassName=WebAssembly.Instructions.Int32AddTests"

# Run a specific spec test
dotnet test --filter "FullyQualifiedName~SpecTest_globals"

# Build/test specific configuration (both Debug and Release are tested in CI — conditional compilation differs)
dotnet build --configuration Release
dotnet test --configuration Release
```

## Architecture

This library reads, writes, modifies, and executes WebAssembly (WASM 2.0) files entirely in C#, using `System.Reflection.Emit` to JIT-compile WASM to .NET IL — no external interpreter.

### Three layers

**1. Module layer** (`WebAssembly` namespace)  
`Module` is the root object representing a WASM binary. It holds typed collections: `Types`, `Imports`, `Functions`, `Tables`, `Memories`, `Globals`, `Exports`, `Codes`, `Data`, `Elements`, `CustomSections`. `Module.ReadFromBinary()` / `WriteToBinary()` handle serialization.

**2. Instructions layer** (`WebAssembly.Instructions` namespace)  
200+ classes, one per WASM opcode, each inheriting from `Instruction` (or `BlockTypeInstruction`/`OperandInstruction`). A `FunctionBody.Code` is a `List<Instruction>`. You build or inspect WASM logic by constructing these objects directly.  
Prefixed opcode families use separate enums: `MiscellaneousOpCode` (0xFC prefix: non-trapping conversions, bulk memory), `SimdOpCode` (0xFD prefix: SIMD).

**3. Runtime/Compilation layer** (`WebAssembly.Runtime` namespace)  
`Compile.FromBinary<T>()` and `Compile.FromModule<T>()` are the main entry points. They take a generic abstract class `T` (whose abstract methods map to WASM exports) and an `ImportDictionary`, and return a factory that produces instances of `T`. Internally, `Runtime/Compilation/CompilationContext.cs` drives IL emission. An experimental `Compile.ToAssembly()` path (requires .NET 9+) uses `PersistedAssemblyBuilder` to emit a .NET DLL instead.

### Import system

`ImportDictionary` maps `"module"/"field"` names to:
- `FunctionImport` — wraps a delegate
- `MemoryImport` — provides linear memory
- `GlobalImport` — provides a mutable or immutable global
- `TableImport` — provides a function table

### Test project

Uses **MSTest**. Base classes (`CompilerTestBase<T>`, `ComparisonTestBase`, `ConversionTestBase`, etc.) reduce boilerplate for instruction tests. Each instruction class in `WebAssembly.Instructions/` has a corresponding `*Tests.cs` in `WebAssembly.Tests/Instructions/`. WASM spec test data lives in `WebAssembly.Tests/Runtime/SpecTestData/` — all 62 WASM spec test suites pass, including 45 SIMD suites. The only skipped tests are permanent CLR limitations (see below).

## Code style

Enforced via `.editorconfig` and treated as build errors:
- File-scoped namespaces (`namespace Foo;`)
- Expression-bodied members preferred
- `var` for apparent and built-in types
- Nullable reference types enabled
- All warnings are errors

## Important constraints

- **WASM 2.0.** All WASM 2.0 opcodes are implemented: non-trapping conversions (0xFC), bulk memory (0xFC), reference types (`ref.null`, `ref.is_null`, `ref.func`, `table.get/set`), typed select, and SIMD (0xFD, 200+ sub-opcodes).
- **Strong-named assembly.** The SNK file (`Properties/WebAssembly.snk`) must remain in place; do not remove it.
- **Multi-framework targets.** The library targets `netstandard2.0`, `net8.0`, and `net9.0`. Tests target `net8.0`, `net9.0`, and `net10.0`. CI tests both Debug and Release.
- **Flaky timeout tests:** `Loop_Compiled` and `Branch_LoopValue` occasionally time out when all three framework test runs execute concurrently (resource contention). Run frameworks sequentially to avoid this.
- **Tail-call optimization and stack exhaustion:** The CLR JIT tail-call-optimizes simple self-recursion into a true loop, so `EnsureSufficientExecutionStack()` (emitted at the start of every compiled function) never fires. The `assert_exhaustion` tests for `runaway`/`mutual-runaway` in `call` and `call_indirect` remain permanently skipped for this reason.

## CLR workarounds already in place

These issues were fixed and should not be regressed:

- **NaN payload preservation in constants:** `Float32Constant`/`Float64Constant` emit `ldc.i4`/`ldc.i8` + `FloatHelper.UInt32BitsToFloat`/`UInt64BitsToDouble` for NaN values instead of `ldc.r4`/`ldc.r8`, which would let the JIT canonicalize the payload.
- **NaN payload preservation in memory:** `MemoryReadInstruction` loads float data as integer bits (`Ldind_I4`/`Ldind_I8`) then reinterprets; `MemoryWriteInstruction` reinterprets float to integer bits (`FloatHelper.FloatToUInt32Bits`/`DoubleToUInt64Bits`) before storing with `Stind_I4`/`Stind_I8`.
- **Canonical NaN from arithmetic:** `ValueTwoToOneInstruction.Compile` calls `FloatHelper.CanonicalizeFloat32`/`CanonicalizeFloat64` after float32/float64 binary ops (add/sub/mul/div) to replace non-canonical NaN payloads from sNaN inputs with the WASM canonical qNaN. `Float32DemoteFloat64` and `Float64PromoteFloat32` do the same after conversion.
- **`rem_s` INT_MIN % -1:** `Int32RemainderSigned`/`Int64RemainderSigned` emit a helper that returns 0 when divisor is −1 (CLR `Rem` would throw `OverflowException`; WASM spec requires 0).
- **SIMD f32x4/f64x2 min/max on .NET 8:** `Vector128.Min`/`Max` maps to `MINPS`/`MAXPS` on .NET 8, which has wrong NaN-propagation and ±0 semantics. `V128Helper.Float32x4Min/Max` and `Float64x2Min/Max` use a scalar per-lane fallback on .NET < 9 that implements WASM spec precisely.

## Permanently skipped spec tests

| Test | Lines | Reason |
|------|-------|--------|
| `call` | 272, 273 | `assert_exhaustion`: CLR JIT tail-call-optimizes `runaway`/`mutual-runaway` into infinite loops; `EnsureSufficientExecutionStack` never fires |
| `call_indirect` | 556, 557 | Same as above |
| `skip-stack-guard-page` | entire suite | Invoking this WASM causes an uncatchable `StackOverflowException` that crashes the CLR test host |
