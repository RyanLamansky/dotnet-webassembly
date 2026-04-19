# PR 6 — New Element Segment Formats

Goal: parse the WASM 2.0 extended element segment encodings so that modules using passive/declarative/expression-based element segments load without error. This unblocks `table.init`, `elem.drop`, and `ref.func`.

## Background

WASM 1.0 had a single element segment format (kind 0): active, targeting table 0 via an i32-constant offset expression, containing a list of function indices.

WASM 2.0 defines 8 segment kinds (bits 0-2 of a u32 flags field):

| Kind | Active? | Table expr? | Init form |
|------|---------|-------------|-----------|
| 0 | active, table 0, i32 offset expr | no | func indices |
| 1 | passive | no | func indices (prefixed with elemtype 0x00) |
| 2 | active, explicit table idx + i32 offset | no | func indices |
| 3 | declarative | no | func indices (prefixed with elemtype 0x00) |
| 4 | active, table 0, i32 offset expr | no | init exprs |
| 5 | passive | no | init exprs (prefixed with reftype) |
| 6 | active, explicit table idx + i32 offset | no | init exprs |
| 7 | declarative | no | init exprs (prefixed with reftype) |

Bit 0 = 0 → active, 1 → passive/declarative
Bit 1 = explicit table index present
Bit 2 = init is expressions (not func indices)

## CLR implementation strategy

- Extend `Element.cs` (or create `ElementSegment.cs`) to hold: Kind, TableIndex, OffsetExpression, FuncIndices, InitExprs, ElemType.
- Parser handles all 8 kinds; runtime uses only kind 0 (existing path) — other kinds parse-and-discard for now.
- Once parsed, `table.init` and `elem.drop` can look up passive segment data by index → implement their `Compile` methods.
- `ref.func` can be implemented properly once element segment data is available at compile time (store func index in field, emit delegate construction).

## Files to change

| File | Change |
|------|--------|
| `Element.cs` | Extend to parse all 8 segment kinds; add Kind, TableIndex, InitExprs, ElemType |
| `Runtime/Compile.cs` | Update element section reading to use new parser; pass passive segment data to compilation context |
| `Runtime/Compilation/CompilationContext.cs` | Add `PassiveElements` (or similar) to hold passive segment byte data for `table.init` |
| `Instructions/TableInit.cs` | Implement Compile (was stub) |
| `Instructions/ElemDrop.cs` | Implement Compile (was stub) |
| `Instructions/RefFunc.cs` | Implement Compile properly (emit delegate, not ldnull) |
| `WebAssembly.Tests/Instructions/TableInitTests.cs` | Update to test actual behavior |
| `WebAssembly.Tests/Instructions/ElemDropTests.cs` | Update to test actual behavior |

## Status

- [x] Element.cs — parse/write all 8 kinds; Kind/ElemType/InitExprs properties added
- [x] Compile.cs — SkipElementSegment handles kinds 1–7; kind 0 still active-populates table
- [x] Module.cs — validation only checks active-kind InitializerExpression
- [ ] CompilationContext.cs — PassiveElements field (deferred; needed for table.init)
- [ ] TableInit.cs — implement Compile (deferred; needs passive element data)
- [ ] ElemDrop.cs — implement Compile (deferred; needs passive element data)
- [ ] RefFunc.cs — implement Compile properly (deferred)
- [x] Tests — ElementSegmentTests for kinds 0, 1, 3
