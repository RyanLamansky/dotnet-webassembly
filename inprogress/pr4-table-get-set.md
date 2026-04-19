# PR 4 — table.get / table.set

Goal: implement opcodes 0x25 (`table.get`) and 0x26 (`table.set`) — direct indexed access to table elements.

## Opcodes (top-level, not under 0xFC)

| Code | Instruction | Stack signature | Notes |
|------|-------------|-----------------|-------|
| 0x25 | table.get tbl | [i32] → [ref] | Load ref at index |
| 0x26 | table.set tbl | [i32] [ref] → [] | Store ref at index |

## CLR implementation strategy

- Both take a `uint TableIndex` immediate (LEB128).
- `table.get`: pop i32 index → `FunctionTable.IndexGetter` → push `FuncRef` (object).
- `table.set`: pop ref, pop i32 index → `FunctionTable.IndexSetter`.
- Only table index 0 supported for now (same constraint as `table.grow`/`table.size`).
- `FunctionTable` indexer already exists (`this[int index]`) — just needs `(int)` cast on the index.

## Files to change

| File | Change |
|------|--------|
| `OpCode.cs` | Add `TableGet = 0x25`, `TableSet = 0x26` |
| `Instructions/TableGet.cs` | New — pop i32, push ref via `FunctionTable.IndexGetter` |
| `Instructions/TableSet.cs` | New — pop ref + i32, call `FunctionTable.IndexSetter` |
| `Instruction.cs` | Wire 0x25 and 0x26 into `Parse` switch |
| `WebAssembly.Tests/Instructions/TableGetTests.cs` | New |
| `WebAssembly.Tests/Instructions/TableSetTests.cs` | New |

## Status

- [x] OpCode.cs — add TableGet (0x25), TableSet (0x26)
- [x] TableGet.cs
- [x] TableSet.cs
- [x] Instruction.cs parser wiring
- [x] Tests
