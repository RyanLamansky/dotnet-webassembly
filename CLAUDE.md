# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build WebAssembly.sln

# Test (all frameworks)
dotnet test

# Run a specific test class
dotnet test --filter "ClassName=WebAssembly.Instructions.Int32AddTests"

# Build/test specific configuration (both Debug and Release are tested in CI — conditional compilation differs)
dotnet build --configuration Release
dotnet test --configuration Release
```

## Architecture

This library reads, writes, modifies, and executes WebAssembly (WASM 1.0) files entirely in C#, using `System.Reflection.Emit` to JIT-compile WASM to .NET IL — no external interpreter.

### Three layers

**1. Module layer** (`WebAssembly` namespace)  
`Module` is the root object representing a WASM binary. It holds typed collections: `Types`, `Imports`, `Functions`, `Tables`, `Memories`, `Globals`, `Exports`, `Codes`, `Data`, `Elements`, `CustomSections`. `Module.ReadFromBinary()` / `WriteToBinary()` handle serialization.

**2. Instructions layer** (`WebAssembly.Instructions` namespace)  
200+ classes, one per WASM opcode, each inheriting from `Instruction` (or `BlockTypeInstruction`/`OperandInstruction`). A `FunctionBody.Code` is a `List<Instruction>`. You build or inspect WASM logic by constructing these objects directly.

**3. Runtime/Compilation layer** (`WebAssembly.Runtime` namespace)  
`Compile.FromBinary<T>()` and `Compile.FromModule<T>()` are the main entry points. They take a generic abstract class `T` (whose abstract methods map to WASM exports) and an `ImportDictionary`, and return a factory that produces instances of `T`. Internally, `Runtime/Compilation/CompilationContext.cs` drives IL emission. An experimental `Compile.ToAssembly()` path (requires .NET 9+) uses `PersistedAssemblyBuilder` to emit a .NET DLL instead.

### Import system

`ImportDictionary` maps `"module"/"field"` names to:
- `FunctionImport` — wraps a delegate
- `MemoryImport` — provides linear memory
- `GlobalImport` — provides a mutable or immutable global
- `TableImport` — provides a function table

### Test project

Uses **MSTest**. Base classes (`CompilerTestBase<T>`, `ComparisonTestBase`, `ConversionTestBase`, etc.) reduce boilerplate for instruction tests. Each instruction class in `WebAssembly.Instructions/` has a corresponding `*Tests.cs` in `WebAssembly.Tests/Instructions/`. WASM spec test data lives in `WebAssembly.Tests/Runtime/SpecTestData/`.

## Code style

Enforced via `.editorconfig` and treated as build errors:
- File-scoped namespaces (`namespace Foo;`)
- Expression-bodied members preferred
- `var` for apparent and built-in types
- Nullable reference types enabled
- All warnings are errors

## Important constraints

- **WASM 1.0 only.** Post-1.0 features (SIMD, threads, multi-memory, etc.) are not supported and will fail at read time.
- **Strong-named assembly.** The SNK file (`Properties/WebAssembly.snk`) must remain in place; do not remove it.
- **Multi-framework targets.** The library targets `netstandard2.0`, `net8.0`, and `net9.0`. Tests target `net8.0`, `net9.0`, and `net10.0`. CI tests both Debug and Release.
