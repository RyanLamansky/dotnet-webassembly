# PR 1 — Multi-Value Returns

Goal: implement the WASM 2.0 multi-value proposal so functions and blocks can return more than one value.

## Background

The WASM binary format encodes function types as `(params*) -> (returns*)`. WASM 1.0 capped returns at 1. Multi-value removes that cap. Block/loop/if instructions also use type signatures ("block types"), which in 1.0 were a single value type or empty, but with multi-value become full type indices into the module's type section.

## Data model (already fine)

- `WebAssemblyType.Returns` is already `IList<WebAssemblyValueType>` ✓
- `Signature.ReturnTypes` / `RawReturnTypes` are already arrays ✓
- `Call.cs` already loops through all return types when pushing to the value stack ✓
- `CallIndirect.cs` stack push already loops through all return types ✓

## Blocking issues to fix (in dependency order)

### 1. `Signature.cs` — reading is gated to 0 or 1

```csharp
// line ~44
var returns = this.ReturnTypes = new Type[reader.ReadVarUInt1()];  // ← ReadVarUInt1 hard-limits to 1
// ...
if (returns.Length > 1)
    throw new ModuleLoadException("Multiple returns are not supported.", ...);  // ← remove
```

Fix: change `ReadVarUInt1()` → `ReadVarUInt32()`, remove the throw.

---

### 2. `BlockType.cs` + `BlockTypeInstruction.cs` — block types can't represent multi-value

In WASM 2.0, a block type is one of:
- `0x40` — empty (void)
- A value type byte (`-1` .. `-4`)
- A **type index** (non-negative s33 integer) — references a full function type in `module.Types`

The current `BlockType` enum only covers the first two cases. `BlockTypeInstruction.Type` is a single `BlockType`.

Fix:
- Keep the `BlockType` enum for the inline cases.
- Add `BlockTypeInstruction.TypeIndex` (nullable `int`) for the third case.
- When reading a block instruction, decode the s33: if negative, it's an inline value type; if non-negative, it's a type index.
- Compiler must look up the full `WebAssemblyType` from the module when a type index is used.

---

### 3. `CompilationContext.cs` — maps function signature to a single `BlockType` for the outermost block

```csharp
// lines ~36-50
returnType = signature.RawReturnTypes[0] switch { ... };  // ← only looks at [0]
```

The outermost block of a function needs to carry the full return signature, not a single `BlockType`. Fix: store the full `Signature` (or `WebAssemblyValueType[]`) alongside the block stack so `End`/`Return` can emit multiple values.

---

### 4. `Return.cs` — assertion + single-value emit

```csharp
Assert(returnsLength is 0 or 1);  // ← remove
// ...
var value = context.DeclareLocal(returns[0].ToSystemType());  // ← single value only
```

With multiple returns the .NET method must return a value type that packs all of them (a `ValueTuple<...>`). Fix:
- Remove assertion.
- If `returnsLength == 1`, keep existing path.
- If `returnsLength > 1`, emit code to store each return value into a `ValueTuple` and return that.

---

### 5. `End.cs` — same assertion + single-value pop

```csharp
Assert(returnsLength is 0 or 1);  // ← remove
// ...
context.PopStack(OpCode.End, returns[0]);  // ← single value only
```

Same fix as Return.cs.

---

### 6. `Compile.cs` — method definitions use `.FirstOrDefault()` / `[0]`

Several places define .NET `MethodBuilder` return types using only the first WASM return:

| ~Line | Site | Fix |
|-------|------|-----|
| 431 | Internal function `MethodBuilder` | Use `ValueTuple` type when `returns.Length > 1` |
| 621 | Exported function `MethodBuilder` | Same |
| 755 | Function import invoker | Same |
| 1318 | Function table element wrapper | Same |

---

### 7. `CallIndirect.cs` — delegate creation uses `returns[0]`

```csharp
returns.Length == 0 ? typeof(void) : returns[0],  // ← only first return
```

Fix: same `ValueTuple` approach.

---

### 8. `CompilerConfiguration.cs` — `GetStandardDelegateForType` only handles 0 or 1 returns

```csharp
public static Type? GetStandardDelegateForType(int parameters, int returns)
{
    switch (returns) { ... }  // ← no case for returns > 1
}
```

Fix: for `returns > 1`, return a `Func<..., ValueTuple<...>>` delegate type, or document that callers must supply a custom `GetDelegateForTypeCallback`.

---

## ValueTuple strategy for multiple returns

.NET methods can only return one type. The cleanest mapping for N>1 returns is `System.ValueTuple<T1, T2, ...>` (up to 7 items; nest for more). This is:
- Blittable / zero-allocation
- Easily deconstructed by callers
- Supported by `System.Reflection.Emit`

Helpers needed:
- `ToTupleType(WebAssemblyValueType[])` — builds the CLR `Type` for the tuple
- `EmitStoreTuple(ILGenerator, int count)` — emits IL to pack the top N stack values into a tuple
- `EmitLoadTuple(ILGenerator, int count)` — emits IL to unpack a tuple back onto the stack (used after `Call` returns a multi-value)

---

## Test plan

- Add tests for functions that return 2 and 3 values of mixed types.
- Test multi-value block (`block (result i32 i64) ... end`).
- Test `call` to a multi-value function — results pushed onto caller's stack.
- Existing tests must still pass (0-return and 1-return paths unchanged).

---

## Files to change (summary)

| File | Change |
|------|--------|
| `WebAssembly/Runtime/Compilation/Signature.cs` | `ReadVarUInt1` → `ReadVarUInt32`, remove multi-return throw |
| `WebAssembly/BlockType.cs` | Document type-index encoding; no enum changes needed |
| `WebAssembly/Instructions/BlockTypeInstruction.cs` | Add `TypeIndex` property; update binary read/write |
| `WebAssembly/Runtime/Compilation/CompilationContext.cs` | Carry full return signature on outermost block |
| `WebAssembly/Instructions/Return.cs` | Remove assertion; emit ValueTuple for N>1 returns |
| `WebAssembly/Instructions/End.cs` | Remove assertion; pop/push N values for N>1 returns |
| `WebAssembly/Runtime/Compile.cs` | Use tuple return type wherever `returns[0]` / `FirstOrDefault` used |
| `WebAssembly/Instructions/CallIndirect.cs` | Use tuple return type for delegate creation |
| `WebAssembly/Runtime/CompilerConfiguration.cs` | Handle N>1 returns in `GetStandardDelegateForType` |
| `WebAssembly.Tests/Instructions/*` | New tests for multi-value functions and blocks |

---

## Status

- [x] Signature.cs — unlock reading multiple returns
- [x] BlockTypeInstruction.cs — add type-index block types
- [x] CompilationContext.cs — carry full return signature
- [x] Return.cs — emit tuple for N>1
- [x] End.cs — pop/push N values
- [x] Compile.cs — use tuple return type throughout
- [x] CallIndirect.cs — use tuple return type
- [x] MultiValueHelper.cs — new helper for ClrReturnType, DelegateTypeArgs, EmitTupleUnpack
- [x] SpecTestRunner.cs — "invalid result arity" now skipped (valid in WASM 2.0)
- [x] Tests — MultiValueTests.cs (5 tests covering End, Return, mixed types, params, Call)

## Implementation notes

- Multi-value CLR encoding: N>1 returns → `ValueTuple<T1,…,TN>` (max 7; nesting not yet implemented)
- Callers of multi-value functions (`Call`, `CallIndirect`) unpack the tuple back onto the WASM stack via `EmitTupleUnpack`
- Delegate types for N>1 returns use the packed tuple as a single CLR return: `Func<…, ValueTuple<…>>`
- Block type-index encoding (non-negative s33) is decoded in `BlockTypeInstruction` but full type-index resolution in block validation (`Branch`, `BranchIf`, `End`) still uses the single-value `BlockType` path; blocks with type indices are treated as empty for branching purposes until that is extended
