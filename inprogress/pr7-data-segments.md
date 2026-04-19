# PR 7 — Passive Data Segments & memory.init / data.drop

Goal: parse WASM 2.0 extended data segment encodings and implement `memory.init` and `data.drop` properly.

## Background

WASM 1.0 data segments were always active (kind 0): they specified a memory index and an i32 offset expression, and the runtime copied their bytes into memory at instantiation.

WASM 2.0 adds:
- Kind 0: active, memory index 0 (legacy encoding — `u32 flags=0`, offset expr, byte vec)
- Kind 1: passive — no memory, no offset; bytes are available at runtime via `memory.init`; must be explicitly dropped with `data.drop`
- Kind 2: active, explicit memory index

Additionally the `DataCount` section (0x0C) appears before the `Code` section when passive segments are present; it gives the total count of data segments upfront (already partially handled — `DataCount` section parsing was added in a prior PR).

## CLR implementation strategy

- Extend `Data.cs` to hold a `Kind` (0/1/2) and optional explicit `MemoryIndex`.
  - Kind 1 (passive): no `InitializerExpression`, no `MemoryIndex`; bytes stored as field on compiled type.
- In `Compile.cs`: for passive data segments, emit a `static byte[]` field on the compiled type initialized with the segment bytes. Use `DataSegments` dictionary in `CompilationContext` mapping segment index → FieldBuilder.
- `memory.init` Compile: load `this.memory`, load offset/src/len from stack, load segment field, call a new `UnmanagedMemory.InitFromSegment(uint dst, byte[] src, uint srcOffset, uint length)` helper.
- `data.drop` Compile: store null to the segment field (marks it as dropped).

## Files to change

| File | Change |
|------|--------|
| `Data.cs` | Add Kind (0/1/2), optional MemoryIndex; update reader/writer |
| `Runtime/UnmanagedMemory.cs` | Add `InitFromSegment(uint dst, byte[] src, uint srcOffset, uint length)` |
| `Runtime/Compile.cs` | Emit `byte[]` fields for passive segments; populate `CompilationContext.DataSegments` |
| `Runtime/Compilation/CompilationContext.cs` | Add `DataSegments` dict (index → FieldBuilder) |
| `Instructions/MemoryInit.cs` | Implement Compile (was stub) |
| `Instructions/DataDrop.cs` | Implement Compile (was stub) |
| `WebAssembly.Tests/Instructions/MemoryInitTests.cs` | Update to test actual behavior |
| `WebAssembly.Tests/Instructions/DataDropTests.cs` | Update to test actual behavior |

## Status

- [ ] Data.cs — parse kinds 0/1/2
- [ ] UnmanagedMemory.cs — InitFromSegment helper
- [ ] Compile.cs — emit passive segment fields
- [ ] CompilationContext.cs — DataSegments field
- [ ] MemoryInit.cs — implement Compile
- [ ] DataDrop.cs — implement Compile
- [ ] Tests updated
