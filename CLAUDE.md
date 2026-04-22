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

Uses **MSTest**. Base classes (`CompilerTestBase<T>`, `ComparisonTestBase`, `ConversionTestBase`, etc.) reduce boilerplate for instruction tests. Each instruction class in `WebAssembly.Instructions/` has a corresponding `*Tests.cs` in `WebAssembly.Tests/Instructions/`. WASM spec test data lives in `WebAssembly.Tests/Runtime/SpecTestData/` — 62 test suites covering WASM 1.0, WASM 2.0, bulk memory, and 45 SIMD suites.

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
- **CLR NaN canonicalization:** The CLR replaces arbitrary NaN bit payloads with the platform's canonical quiet NaN when values pass through floating-point registers. This affects `Float32Constant`/`Float64Constant` (`ldc.r4`/`ldc.r8`) and float store instructions (`Stind_R4`/`Stind_R8`). A small number of `float_literals`, `float_memory`, and `float_exprs` spec tests are currently skipped as a result. The fix is to emit integer-reinterpret IL for NaN values (e.g., `ldc.i4 <bits>` + `BitConverter.Int32BitsToSingle`) to bypass FP register canonicalization.
- **Flaky timeout tests:** `Loop_Compiled` and `Branch_LoopValue` occasionally time out when all three framework test runs execute concurrently (resource contention). Run frameworks sequentially to avoid this.
- **SIMD NaN/−0 on .NET 8:** `simd_f32x4` and `simd_f64x2` min/max semantics for NaN inputs and −0 differ from spec on .NET 8. These tests are marked `Assert.Inconclusive` on .NET < 9.

## Known spec test gaps (work in progress)

| Gap | Spec tests affected | Issue |
|-----|---------------------|-------|
| NaN constant canonicalization | `float_literals` (lines 109-113), `float_exprs` (lines 2349-2361) | `ldc.r4`/`ldc.r8` clobbers NaN payload |
| NaN store canonicalization | `float_memory` (lines 21, 73) | `Stind_R4` clobbers NaN payload |
| `float_exprs` overflow | lines 511, 519 | Arithmetic overflow on CLR |
| Stack exhaustion / `assert_exhaustion` | `call` (272-273), `call_indirect` (556) | StackOverflowException not catchable |
| `bulk` spec test not wired up | `bulk.json` | Test method missing in SpecTests.cs |
| `call_indirect` harness gaps | lines 557-589, 940 | No method source / unknown function |
| SIMD min/max on .NET 8 | `simd_f32x4`, `simd_f64x2` | Platform NaN/−0 semantics differ |
