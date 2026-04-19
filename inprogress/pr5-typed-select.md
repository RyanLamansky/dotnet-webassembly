# PR 5 — Typed Select (select t*)

Goal: implement opcode 0x1C (`select t*`) — the typed variant of `select` that carries an explicit vector of value types.

## Background

WASM 2.0 added `select t*` (0x1C) alongside the untyped `select` (0x1B). The typed form carries a byte-count-prefixed vector of value types as immediates. In practice the vector always has exactly one element (the type of the two operands). The untyped form is still valid for numeric types; the typed form is required for reference types (funcref/externref) to disambiguate.

## Binary encoding

```
0x1C  vec(valtype)
       ^--- u32 count (always 1 in practice), followed by count valtype bytes
```

## CLR implementation strategy

- New class `SelectWithType` (or reuse by adding a `Types` property to `Select`) — separate class is cleaner.
- `Compile`: identical to `Select.Compile` but the type is known from the immediate rather than inferred from the stack. Use the same `SelectInt32`/`SelectObject`/etc. helper dispatch.
- Stack effect: pop i32 condition + two values of the declared type, push one value of that type.

## Files to change

| File | Change |
|------|--------|
| `OpCode.cs` | Add `SelectWithType = 0x1C` |
| `Instructions/SelectWithType.cs` | New — reads `vec(valtype)` immediate; compiles like `Select` |
| `Instruction.cs` | Wire 0x1C into `Parse` switch |
| `WebAssembly.Tests/Instructions/SelectWithTypeTests.cs` | New |

## Status

- [x] OpCode.cs — add SelectWithType (0x1C)
- [x] SelectWithType.cs
- [x] Instruction.cs parser wiring
- [x] Tests
- [x] OpCodeTests.cs — added "with"/"type" to replacements dict
