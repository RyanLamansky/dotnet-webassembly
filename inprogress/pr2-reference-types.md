# PR 2 — Reference Types

Goal: implement the WASM 2.0 reference types proposal so modules using `funcref`/`externref`, `ref.null`, `ref.is_null`, `ref.func`, and multiple tables parse and execute.

## Background

Reference types adds:
- Two new value types: `funcref` (0x70 / -0x10) and `externref` (0x6F / -0x11)
- Three new instructions: `ref.null` (0xD0), `ref.is_null` (0xD1), `ref.func` (0xD2)
- `select t*` (typed select, 0x1C) — allows select on reference types
- Multiple tables per module
- New element segment forms (expressions, not just function indices)
- `table.get`/`table.set` (under 0xFC prefix)

## CLR type mappings

| WASM type | CLR type |
|---|---|
| funcref | `object` (callable, stored as delegate or null) |
| externref | `object` (opaque host reference) |

Using `object` for both avoids having to differentiate reference types in generic select/store operations at the CLR level.

## Files to change

### 1. `WebAssembly/WebAssemblyValueType.cs`
Add:
- `FuncRef = -0x10`
- `ExternRef = -0x11`

Update `ValueTypeExtensions`:
- `ToSystemType()`: FuncRef → `object`, ExternRef → `object`
- `TryConvertToValueType()`: no change (no round-trip from CLR `object`)

### 2. `WebAssembly/BlockType.cs`
Add:
- `FuncRef = -0x10`
- `ExternRef = -0x11`

Update `BlockTypeExtensions.TryToValueType()` to map these.

### 3. `WebAssembly/ElementType.cs`
Add:
- `ExternRef = -0x11`

(FunctionReference = -0x10 already covers funcref)

### 4. `WebAssembly/OpCode.cs`
Add:
- `RefNull = 0xD0`
- `RefIsNull = 0xD1`
- `RefFunc = 0xD2`

### 5. New instruction files
- `WebAssembly/Instructions/RefNull.cs` — `ref.null t`: pushes null of given ref type; emits `ldnull`
- `WebAssembly/Instructions/RefIsNull.cs` — `ref.is_null`: pops ref, pushes i32; emits `ldnull; ceq`
- `WebAssembly/Instructions/RefFunc.cs` — `ref.func idx`: pushes funcref for function at index

### 6. `WebAssembly/Instructions/Instruction.cs` (parser)
Add cases for 0xD0, 0xD1, 0xD2 in the switch statement.

### 7. `WebAssembly/Instructions/Select.cs`
Add a reference-type path: when the top-of-stack type is `FuncRef` or `ExternRef`, use `object`-typed select helper instead of the numeric helpers.

### 8. `WebAssembly/Runtime/Compilation/HelperMethod.cs`
Add `SelectObject` helper method entry.

### 9. `WebAssembly/Runtime/Compile.cs` — Table section
Remove the throw on multiple tables; allow additional tables to be parsed (extra tables won't be used by `call_indirect` for now but the module will load).

## Deferred to later PR
- `table.get` / `table.set` / `table.grow` / `table.size` / `table.fill` / `table.copy` (these are bulk-memory territory, covered in PR 3)
- Typed `select t*` (0x1C) — needs type annotation parsing
- New element segment forms (active/passive/declarative with expressions)
- Full multi-table `call_indirect` — needs table-index tracking in compiler

## Status

- [x] WebAssemblyValueType.cs — add FuncRef (-0x10), ExternRef (-0x11); map both to `object`
- [x] BlockType.cs — add FuncRef, ExternRef; update TryToValueType and ToTypeString
- [x] ElementType.cs — add ExternRef (-0x11)
- [x] OpCode.cs — add RefNull (0xD0), RefIsNull (0xD1), RefFunc (0xD2)
- [x] RefNull.cs — new instruction; emits ldnull, pushes ref type on stack
- [x] RefIsNull.cs — new instruction; emits ldnull/ceq, pushes i32
- [x] RefFunc.cs — new instruction; emits ldnull placeholder (TODO: real function reference)
- [x] Instruction.cs — wire new opcodes into both Parse and ParseInitializerExpression
- [x] Select.cs + HelperMethod.cs — SelectObject helper for FuncRef/ExternRef
- [x] Compile.cs — allow multiple tables; extra/externref tables are parsed but unused
- [x] SpecTestRunner.cs — "multiple tables" now skipped (valid in WASM 2.0)
- [x] Tests — RefNullTests, RefIsNullTests, RefFuncTests

## Known limitations

- `ref.func` emits `null` as placeholder — full function-reference storage not yet implemented
- Additional tables (index > 0) are parsed but not accessible via `call_indirect`
- Typed `select t*` (0x1C) not yet parsed
- New element segment formats (passive/declarative/expression-based) not yet handled
