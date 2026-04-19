# PR 3 — Bulk Memory & Table Operations

Goal: implement the WASM 2.0 bulk memory proposal — new instructions under the 0xFC prefix.

## Opcodes (all under MiscellaneousOperationPrefix 0xFC)

| Code | Instruction | Stack signature | Notes |
|------|-------------|-----------------|-------|
| 0x08 | memory.init seg, mem | i32 i32 i32 → | Copy from passive data segment into memory |
| 0x09 | data.drop seg | → | Drop passive data segment |
| 0x0A | memory.copy dst, src | i32 i32 i32 → | Copy within memory (may overlap) |
| 0x0B | memory.fill mem | i32 i32 i32 → | Fill memory region with byte value |
| 0x0C | table.init seg, tbl | i32 i32 i32 → | Copy from passive element segment into table |
| 0x0D | elem.drop seg | → | Drop passive element segment |
| 0x0E | table.copy dst, src | i32 i32 i32 → | Copy within / between tables |
| 0x0F | table.grow tbl | ref i32 → i32 | Grow table, return old size or -1 |
| 0x10 | table.size tbl | → i32 | Current table size |
| 0x11 | table.fill tbl | i32 ref i32 → | Fill table region with ref value |

## CLR implementation strategy

- `memory.copy` / `memory.fill` → emit a call to a helper that uses `Buffer.MemoryCopy` / `Unsafe.InitBlock` on the `UnmanagedMemory.Start` pointer
- `memory.init` / `data.drop` → needs access to the data segment bytes at runtime; store segments as `byte[]` fields on the compiled type
- `table.*` → most operate on `FunctionTable`; size/grow/fill need new methods on `FunctionTable`
- Instructions that reference segments or tables carry immediate operands (LEB128 indices)

For this PR, implement the memory operations fully and stub the table/segment operations (parse correctly, throw `NotSupportedException` at runtime for now) — this gets most real-world WASM files loading.

## Files to change

| File | Change |
|------|--------|
| `MiscellaneousOpCode.cs` | Add 0x08–0x11 |
| `Runtime/UnmanagedMemory.cs` | Add `Copy()` and `Fill()` methods |
| `Instructions/MemoryInit.cs` | New — 0x08 |
| `Instructions/DataDrop.cs` | New — 0x09 |
| `Instructions/MemoryCopy.cs` | New — 0x0A |
| `Instructions/MemoryFill.cs` | New — 0x0B |
| `Instructions/TableInit.cs` | New — 0x0C (stub) |
| `Instructions/ElemDrop.cs` | New — 0x0D (stub) |
| `Instructions/TableCopy.cs` | New — 0x0E (stub) |
| `Instructions/TableGrow.cs` | New — 0x0F (stub) |
| `Instructions/TableSize.cs` | New — 0x10 |
| `Instructions/TableFill.cs` | New — 0x11 (stub) |
| `Instruction.cs` | Wire 0x08–0x11 into misc opcode parser |
| `WebAssembly.Tests/Instructions/*` | Test classes for each |

## Status

- [x] MiscellaneousOpCode.cs
- [x] UnmanagedMemory.cs — Copy, Fill methods
- [x] MemoryInit.cs (stub — throws NotSupportedException at compile time)
- [x] DataDrop.cs (stub — throws NotSupportedException at compile time)
- [x] MemoryCopy.cs
- [x] MemoryFill.cs
- [x] TableInit.cs (stub — throws NotSupportedException at compile time)
- [x] ElemDrop.cs (stub — throws NotSupportedException at compile time)
- [x] TableCopy.cs (stub — throws NotSupportedException at compile time)
- [x] TableGrow.cs
- [x] TableSize.cs
- [x] TableFill.cs (stub — throws NotSupportedException at compile time)
- [x] Instruction.cs parser wiring
- [x] Tests
