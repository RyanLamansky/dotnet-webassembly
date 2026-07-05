# Phase 5 — Typed Function References: Design

Design record for Phase 5 of [the WASM 3.0 upgrade plan](Wasm3Plan.md), produced 2026-07-05 from codebase exploration and empirical inspection of the checked-in spec fixtures.
The representation decisions here are deliberately Phase 8-complete: GC builds on this design without a second redesign.

## Verified ground truth (what the fixtures actually contain)

Binary encodings confirmed empirically with `wasm-tools dump` against the checked-in fixtures:

- **Ref-type value encodings**: `0x63` = `(ref null ht)`, `0x64` = `(ref ht)`, each followed by an **s33 heap type**: non-negative = concrete type index (e.g. `63 00` in `table/table.10.wasm`), negative single byte = abstract heap type (`0x70` func, `0x6F` extern, `0x6E` any, `0x71` none, `0x73` nofunc, `0x72` noextern, `0x69` exn, `0x74` noexn; `0x6D` eq / `0x6C` i31 / `0x6B` struct / `0x6A` array occur only in `gc/*`).
- **Table-with-initializer**: leading `0x40 0x00`, then tabletype, then a const expr (`table/table.14.wasm`: `40 00 70 00 01 d0 70 0b`). Typed tables also appear *without* the prefix (`63 00 00 01`).
- **Instructions**: `call_ref` = `0x14 typeidx`, `return_call_ref` = `0x15 typeidx`, `ref.as_non_null` = `0xD4`, `br_on_null` = `0xD5 labelidx`, `br_on_non_null` = `0xD6 labelidx`; `ref.null` (`0xD0`) now takes a full s33 heap type (concrete indices appear throughout: `ref.null $ii` in call_ref.0).
- **Where refs appear in Phase 5 fixtures**: function params/results (`ref.0.wasm` has all forms including `(ref 1)` by raw index), locals, globals (`(ref $ll) ref.func $fac` in call_ref.0; the full mutable/immutable export matrix in linking.9), tables + element segments (`br_table.0.wasm`: `(table 1 1 (ref null $t))`, `(elem (table $t) (i32.const 0) (ref null $t) (ref.func $tf))`), **block result types** (`block $l2 (result (ref null $t))` in br_table), and elem types `(ref func)` in `elem`.
- **Abstract heap types beyond func/extern in Phase 5 categories**: only in `ref_null` (`any`, `none`, `nofunc`, `noextern`, **`exn`, `noexn`**) — as *null values only*. Everything non-null with any/eq/i31/struct/array lives in `gc/*` (Phase 8); non-null exn values live in `exceptions/*` (Phase 6).
- **Trap texts**: `"null function reference"` (call_ref/return_call_ref on null), `"null reference"` (ref.as_non_null). **Invalid texts**: `"type mismatch"`, `"uninitialized local"` (new to the runner), `"unknown type"` (exists), `"unknown label"` (exists).
- **`local_init` semantics from its own asserts**: set-before-block persists *into* inner blocks (valid, local_init.0#2); set *inside* a block does **not** persist past `end` — even when *both* if-arms set it (invalid, local_init.2 and local_init.4); `else` resets to the state at `if` entry (invalid, local_init.3); `local.tee` initializes (valid, local_init.5).
- **`return_call_ref.wast` runs 1,000,000-deep recursion** (`"count"` tests) — the tail call must be real, not best-effort.

## 1. Representation — the central decision

### Decision

Three additive public pieces, one internal workhorse:

**(a) `public enum WebAssemblyHeapType : sbyte`** — new file `WebAssembly/WebAssemblyHeapType.cs`.
Members are the s7 values of the binary bytes: `NoExn = -0x0C (0x74)`, `NoFunc = -0x0D`, `NoExtern = -0x0E`, `None = -0x0F`, `Func = -0x10`, `Extern = -0x11`, `Any = -0x12`, `Eq = -0x13`, `I31 = -0x14`, `Struct = -0x15`, `Array = -0x16`, `Exn = -0x17`.
All twelve are defined in Phase 5 (they are pure encoding facts; Phase 8/6 add *semantics*, not enum members).

**(b) `public readonly struct ReferenceType : IEquatable<ReferenceType>`** — new file `WebAssembly/ReferenceType.cs`:

- `public bool IsNullable { get; }`
- `public WebAssemblyHeapType? HeapType { get; }` — null when the target is a concrete index
- `public uint? TypeIndex { get; }` — null when the target is abstract (exactly one of the two is set, enforced by the two constructors `ReferenceType(WebAssemblyHeapType, bool isNullable)` / `ReferenceType(uint typeIndex, bool isNullable)`)
- internal `static ReferenceType Parse(Reader, sbyte leadByte)` and `void WriteTo(Writer)` (emits `0x63`/`0x64` + s33 heap type).

This shape is **Phase 8-complete**: GC adds struct/array/i31 refs as either abstract heap types (already enum members) or concrete indices into composite types (already `TypeIndex`).
Rec groups and `sub` live in the *type section* object model (Phase 8's `WebAssemblyType` overhaul), not here.
Nothing in this struct needs a second redesign.

**(c) Additive enum members** on the three existing narrow enums, mirroring the raw encoding bytes so the narrow view is context-free and honest:

- `WebAssemblyValueType`: `Reference = -0x1C` (0x64), `NullableReference = -0x1D` (0x63), plus the ten one-byte shorthands `AnyRef = -0x12`, `EqRef = -0x13`, `I31Ref = -0x14`, `StructRef = -0x15`, `ArrayRef = -0x16`, `NoneRef = -0x0F`, `NoFuncRef = -0x0D`, `NoExternRef = -0x0E`, `ExnRef = -0x17`, `NoExnRef = -0x0C`.
- `BlockType`: the same additions.
- `ElementType`: `Reference = -0x1C`, `NullableReference = -0x1D` (element types are reftypes in 3.0).

**(d) `internal readonly struct ValType`** — new file `WebAssembly/Runtime/Compilation/ValType.cs`, the *validator's canonical currency* (see §2).
Public model preserves encoding form for byte-identical round-trip; the validator canonicalizes (`FuncRef` ≡ `(ref null func)`).

### Attachment pattern (the Phase 4 narrow/wide sibling pattern, extended)

Every *scalar* public site keeps its existing property as a **computed narrow view** over internal storage, exactly like `ResizableLimits.Minimum` over `ulong`:

| Site | Existing narrow property (unchanged shape) | New wide sibling |
|---|---|---|
| `Local` | `Type` | `ReferenceType? RefType` |
| `Global` | `ContentType` | `ReferenceType? ContentRefType` |
| `Import.Global` | `ContentType` | `ReferenceType? ContentRefType` |
| `Table` | `ElementType` | `ReferenceType? ElementRefType` **plus** `IList<Instruction>? InitializerExpression` (non-null ⇔ the `0x40 0x00` encoding) |
| `Element` | `ElemType` | `ReferenceType? ElemRefType` |
| `RefNull` | `Type` | `WebAssemblyHeapType? HeapType` + `uint? TypeIndex` (its immediate is a *heap* type, not a reftype) |
| `SelectWithType` | `Type` | `ReferenceType? RefType` |
| `BlockTypeInstruction` | `Type` / `TypeIndex` | `ReferenceType? RefType` (inline reftype block results) |

Semantics: internal storage holds the wide value; the narrow getter returns the stored enum for all pre-3.0/shorthand forms (bit-for-bit unchanged behavior) and returns `Reference`/`NullableReference` when the wide sibling is set; the narrow setter clears the wide sibling.
Writers emit the long form iff the wide sibling is set — so **every existing encoding round-trips byte-identically** (short-form `funcref` stays one byte `0x70`; a long-form `(ref null func)` read from a 3.0 module is preserved as `0x63 0x70`).

The one *list* site, `WebAssemblyType.Parameters`/`Returns` (`IList<WebAssemblyValueType>`), cannot change element type and must keep returning the user's own list instance (reference-equality is observable behavior).
So it keeps the narrow list as **primary storage** — ref entries appear as `Reference`/`NullableReference` — with a positional sidecar: accessors `GetParameterRefType(int index)` / `SetParameterRefType(int, ReferenceType)` (and the `Returns` pair), plus a convenience `AddParameter(ReferenceType)` that appends to both.
Documented: positional edits of a signature containing refs must maintain the sidecar.
`Equals`/`GetHashCode`/`ToString` extended to include the sidecars.

### Defense against the strongest alternative

The strongest alternative is the **write-through wide-primary projection**: store `ValType`-like wide lists and make `Parameters` a projecting `IList` wrapper.
It is more elegant (no misalignment footgun) but it changes observable behavior — `type.Parameters` would no longer be reference-equal to the list a user assigned, and mutation aliasing semantics shift.
That violates the hard additive rule for the sake of an edge case (hand-editing ref-typed signatures positionally) that essentially only new code performs, and new code gets the safe `AddParameter` path.
For scalars there is no such tension, so they get the cleaner computed-view design.
The other alternative — a single new "wide value type" struct replacing `WebAssemblyValueType` everywhere via new parallel properties of a new type — doubles the public surface for *every* site including purely numeric ones, for zero gain: numerics are never lossy.

### Parse/write path

- `Reader` gains `ReadHeapTypeS33()` (via the existing `ReadVarInt64`, range-checked); `Writer` gains `WriteHeapType(...)`.
  Both parse paths must change — **`Compile.cs` re-parses the binary independently of the object model** (its private `Local` struct at `Compile.cs:173`, `Signature(reader,…)`, and the inline Table/Global/Element section readers).
  Every encoding change lands twice; this is the established Phase 3/4 reality.
- `BlockTypeInstruction(Reader)`: today reads one s33 via `ReadVarInt32`.
  A single byte `0x63`/`0x64` decodes to s33 `-0x1D`/`-0x1C` (a genuine type *index* ≥ 0x40 encodes multi-byte, so this is unambiguous); on seeing those, read the following heap type into `RefType`.
- `Table(Reader)` / `Compile.cs` `Section.Table`: first byte `0x40` (s7 `-0x40`, impossible as a reftype) selects the with-initializer form: read `0x00`, tabletype, then `Instruction.ParseInitializerExpression`.
- `Element`: expression-form segments read a full reftype (e.g. `(ref func)` = `0x64 0x70`), not a single `varint7`.
- **Parse-but-defer boundary**: all twelve heap types and all value-type shorthands parse and round-trip.
  What still *rejects* is the **type section**: composite forms `0x5F`/`0x5E` (struct/array), `0x50` (sub), `0x4E` (rec) throw `ModuleLoadException` with an explicit "GC composite types are not yet supported (Phase 8)" — note that `Signature`'s ctor currently *ignores* the form byte (`Signature.cs:36`), which would mis-parse a GC module catastrophically; add the explicit check in both parse paths.
  At *validation* time, non-null use of any/eq/i31/struct/array-hierarchy values is impossible in Phase 5 because the producing opcodes don't exist; null values of those types are fully supported (this is exactly what `ref_null` needs, including `exnref` nulls with zero Phase 6 machinery).

## 2. Subtyping — the validator core

### `ValType` (internal) and the canonicalization split

`ValType` canonically represents: numeric/vector types, or a reference `(nullable, heapKind-or-typeIndex)`.
Key properties/factories: `FromValueType(WebAssemblyValueType)` (canonicalizes `FuncRef` → ref-null-func etc.), `Ref(bool, WebAssemblyHeapType)`, `RefIndexed(bool, uint)`, `IsReference`, `IsNullable`, `ToSystemType()`, `ToNarrow()` (for the public exception surfaces, mapping refs to `Reference`/`NullableReference`/shorthands), `ToString()` for diagnostics.

**The ergonomic linchpin: an implicit conversion `WebAssemblyValueType → ValType`.**
`CompilationContext.Stack` becomes `Stack<ValType?>` (null stays the spec's "unknown" bottom from unreachable code), and the `PopStack`/`PopStackNoReturn`/`ValidateStack` family switches its parameters to `ValType`/`ValType?` — the hundreds of existing call sites like `PopStackNoReturn(opcode, WebAssemblyValueType.Int32)` compile unchanged.
`Signature.RawParameterTypes`/`RawReturnTypes` become `ValType[]` (internal; `Signature` is not source-linked into the test project, so this is free).
Same for `GlobalInfo.Type`, `CompilationContext.Locals`, `TableElementTypes`, `ElementSegmentTypes`, `GetOrCreateResultLocal`, `DeclareResultLocals`, `TailCallHelper.SpillOperands`.

### The Phase 5 lattice

New internal static class `TypeAlgebra` (in `Runtime/Compilation/`), with `bool IsSubtype(ValType sub, ValType super, Signature[] types)`:

- `unknown` (null entry) <: everything (existing behavior, kept).
- Numeric/vector: equality only.
- `(ref null₁ ht₁) <: (ref null₂ ht₂)` iff `ht₁ <:ₕ ht₂` and (`null₁` ⇒ `null₂`).
- Heap lattice (fixed table + one structural rule):
  - func hierarchy: `nofunc <:ₕ $t <:ₕ func`; `$t <:ₕ $u` iff their function signatures are **structurally equal** (recursive over ref-typed params/results with a bisimulation assumption set for cycles — Phase 5 fixtures don't exercise recursion, but building it now is cheap and Phase 8 needs it).
  - extern hierarchy: `noextern <:ₕ extern`.
  - exn hierarchy: `noexn <:ₕ exn`.
  - any hierarchy (null-only in Phase 5, but the full static table goes in now): `none <:ₕ i31, struct, array <:ₕ eq <:ₕ any`.
  - No cross-hierarchy relations.
    No `sub`/`rec` declared subtyping (Phase 8).

### Where equality becomes is-subtype-of

Every expected-vs-actual comparison inside the pop/validate family (`CompilationContext.cs:412-527`) becomes `IsSubtype(actual, expected)`.
Beyond that, the explicit `SequenceEqual`/`!=` sites:

- Block results and `if`-pass-through checks in `End.Compile` / `Else`, and the label-type checks in `BranchHelper.GetBranchTypes` consumers.
- **`BranchTable`**: replace the default-vs-label `SequenceEqual` (`BranchTable.cs:177`) with independent per-label subtype validation of the operand types against *each* label (3.0 no longer requires labels to agree with each other — this is what `br_table`'s "meet" tests exercise).
- `Return`, `ReturnCall`, `ReturnCallIndirect` result matching: callee results must be *subtypes* of the enclosing function's results (relaxed from `SequenceEqual`).
- Call argument pops (already via PopStack), `local.set`/`global.set`/`table.set`/`table.fill`/`table.grow` value operands.
- Element segments: per-element init-expr type <: segment elem type (`ValidateElementInitExpr` in `Compile.cs:1475` becomes a real const-expr type check accepting typed `ref.func`, bottom-typed `ref.null`, and `global.get`); active-segment/`table.init` compatibility: segment type <: table element type (clears `bulk-memory/table-sub:1`).
- Global initializers: init result <: declared type (`(global funcref (ref.null nofunc))`, `(global anyref (ref.null none))`, `(global (ref $ll) (ref.func $fac))`).
- Import/export linking (§3g).
- Type-index bounds everywhere a heap type is read: out-of-range ⇒ `ModuleLoadException("unknown type …")` (matches the runner's existing case; clears `ref` 65–78).

## 3. Instructions & runtime mapping

**Runtime representation stays exactly as today**: every func-hierarchy reference is a `System.Delegate` on the CLR stack; extern/any/exn hierarchies are `object`.
`ValType.ToSystemType()` encodes this.
The concrete delegate type (`Func<…>` from `DelegateInvokersByTypeIndex`) is applied by `castclass` **at use sites only** (the call_indirect precedent), never in locals/fields — this keeps spills, ferry locals, and multi-value plumbing type-uniform.

New opcodes in `OpCode.cs`: `CallRef = 0x14`, `ReturnCallRef = 0x15`, `RefAsNonNull = 0xD4`, `BranchOnNull = 0xD5`, `BranchOnNonNull = 0xD6`; dispatch in `Instruction.cs`'s general parser (none are initializer-expression opcodes; `RefNull`'s reader ctor gains heap-type parsing, which covers both parsers).

**(a) `CallRef`** (`Instructions/CallRef.cs`): validate typeidx; pop `(ref null $t)`-compatible (subtype incl. unknown/nofunc), pop args (subtypes of params), push results.
IL: a per-type-index **static remapper** exactly like `CallIndirect`'s minus the table: signature `(params…, Delegate target) → results`; body: `ldarg target; castclass concreteDelegateType; ldarg params…; call Invoke; ret`.
A null target throws `NullReferenceException` from the delegate invoke — the runner maps trap text `"null function reference"` to that (new runner case).
Multi-value returns unpack via the existing `MultiValueHelper` path.
Cache in a new `Dictionary<uint, MethodBuilder> DelegateCallersByTypeIndex` on `CompilationContext`.

**(b) `ReturnCallRef`** (`Instructions/ReturnCallRef.cs`): result types subtype-match the enclosing function; spill `(params…, funcref)` with `TailCallHelper.SpillOperands` (the ref spills as a `Delegate` local); reload; `castclass`; `Tailcall` + `Call` on the concrete delegate's `Invoke`; `ret`; `MarkUnreachable(functionWide: true)`.
**This is byte-for-byte the Phase 2 `ReturnCallIndirect` shape** (`ReturnCallIndirect.cs:129-148`) with the table lookup replaced by the spilled operand — the open verifiability question resolves by construction: same operand kinds (value types + one Delegate), no byrefs, no protected regions, direct `tail. call Invoke` so no intermediate frame survives.
Proof obligation: the in-suite 1,000,000-deep `"count"` tests plus a dedicated 5,000,000-deep mutual-recursion unit test mirroring `ReturnCallIndirectTests`, run in Debug and Release on net8/9/10.

**(c) `RefAsNonNull`** (`Instructions/RefAsNonNull.cs`): pop any ref (or unknown), push its non-null version.
IL: `dup; brtrue.s CONT; call ThrowNullReference; CONT:` where `ThrowNullReference` is a tiny `HelperMethod` that throws `NullReferenceException("null reference")`.
**Emit nothing when `context.IsUnreachable`** (validation only) — `unreached-valid.2` puts this after `unreachable`, and a dead `dup` on a provably-empty IL stack is the one shape worth not risking.

**(d) `BranchOnNull` / `BranchOnNonNull`** (`Instructions/BranchOnNull.cs`, `BranchOnNonNull.cs`): validation per spec — `br_on_null l`: pop `(ref null ht)`, check label `l` against the remaining stack, push `(ref ht)` on fall-through; `br_on_non_null l`: pop `(ref null ht)`, label's types must end in a non-null ref supertype `(ref ht')` with `ht <:ₕ ht'`; the ref travels *with* the branch; fall-through continues without it.
IL composes the existing `BranchHelper` ferrying:

- on_null: `dup; brtrue FALL; pop; <stash label results, br label>; FALL:`
- on_non_null: `dup; brfalse NULL; <stash label results incl. the ref, br label>; NULL: pop`

Same unreachable-code emit-nothing rule as (c).
This is the trickiest IL in the phase (§7 risk 3).

**(e) Upgrades to existing instructions**: `RefNull` pushes exactly `(ref null ht-as-written)` including concrete indices (bounds-checked); `RefFunc` pushes the **typed** `(ref $sig(f))` using `context.CheckedFunctionSignatures[Index].TypeIndex` (required for `(global (ref $ll) (ref.func $fac))` to validate); `RefIsNull` accepts any reference `ValType`; `SelectWithType` accepts a reftype annotation and subtype-matches operands (clears `select:279`); `table.get/set/grow/fill/copy/init` and `call_indirect` use the table's precise element `ValType` instead of the funcref/externref pair; typed tables still back onto `FunctionTable`/`ExternRefTable` unchanged.

**(f) Typed tables/globals/elements in `Compile.cs`**: Table section parses reftypes + the `0x40 0x00` initializer form; the instance constructor fills every slot of an initializer-carrying table with the evaluated const expr (`ref.null` / `ref.func` / `global.get`); a **non-nullable element type without an initializer** is a "type mismatch" `ModuleLoadException`.
Globals: `GlobalInfo` carries `ValType`; getter/setter CLR types come from `ToSystemType`.
Element section parses reftype elem types; declarative/passive segments carry `ValType`.

**(g) Import/export linking fidelity**: exported globals/tables whose WASM type is a reftype get a **closed structural descriptor** (a canonical string rendering, e.g. `ref null func(i64)->(i64)`, concrete indices expanded recursively with a cycle guard) carried on an extended `NativeExportAttribute` (new optional string member — additive); `RuntimeImport.FromCompiledExports` populates a matching internal field on `GlobalImport`/`TableImport`; `ImportException.ValidateGlobal`/`ValidateTable` gain overloads that compare descriptors — **immutable global: supplied <: expected; mutable global and table element type: invariant** — falling back to today's CLR-type check when no descriptor exists (host-supplied numeric imports unchanged).
New file `Runtime/Compilation/WasmTypeDescriptor.cs`.
This is what flips the `linking` Mref_ex/Mtable_ex blocks and `table` 86/93/112–116.

## 4. Non-defaultable locals — definite-assignment validation

Derived from the `local_init` asserts (§0), which match the spec-appendix algorithm:

- State on `CompilationContext`: `bool[] LocalIsInitialized` + `List<int> InitLog`; `BlockContext` gains `int InitLogHeight`.
- `Reset(...)`: parameters and every *defaultable* local (numerics, v128, nullable refs) start initialized; non-nullable-ref locals start uninitialized.
  Zero behavior change for all existing modules.
- `local.get`: if uninitialized ⇒ `ModuleLoadException("uninitialized local {index}")` (new runner-accepted invalid text).
- `local.set` / `local.tee`: if uninitialized, mark + append to `InitLog`.
- Block/loop/if entry (`MakeBlockContext` and the plain-block path): record `InitLog.Count`.
- `Else.Compile`: roll `InitLog` back to the if-frame's height (un-initialize everything set in the then-arm), keep the height recorded.
- `End.Compile` (frame pop): roll back to the frame's height — **initialization never survives the enclosing block's `end`**, even when both if-arms set it (local_init.4).
- Unreachability does not alter init state.

Hook points are exactly the existing block bookkeeping in `LocalGet`/`LocalSet`/`LocalTee`, `BlockTypeInstruction.MakeBlockContext`, `Else`, `End`, and `CompilationContext.Reset` — no new pass over the code.

## 5. Scope boundary — the checkable target

**Flips fully green in Phase 5** (remove skip / `RunAllSkipped`):
`call_ref`, `return_call_ref` (its 40 skip lines), `br_on_null`, `br_on_non_null`, `ref_as_non_null`, `local_init`, `br_table`, `ref_is_null`, `ref_null` (null-only support for any/exn hierarchies makes the whole category reachable), `ref` (lines 3, 65, 69, 73, 78), `unreached-valid`, and the labeled per-line skips: `func:660`, `select:279`, `table:9,14,19,20,21,86,93,112–116`, `elem:87,315,448–578` (16 lines), `global:634,659–664`, `bulk-memory/table-sub:1`.
Re-check `call_indirect:995` ("type mismatch" validation edge) during curation — subtype-aware arg matching may clear it for free.

**Stays skipped after Phase 5**:

- `imports`: every skipped line traces to **tags** (imports.0 defines tags; 240–256 are tag imports; 3–293 cascade from it) — Phase 6.
  Expect zero imports lines to flip; verify at curation.
- `linking`: the typed-ref blocks (96–245 Mref_*/…, 426–455 Mtable_*) flip; any remaining tag/exception lines stay Phase 6.
- All `gc/*` (incl. `type-rec`/`type-canon`/`type-equivalence`), `exceptions/*`, `relaxed-simd/*` — untouched.
- `type` and `exports` are already green; no Phase 5 work.

Per CLAUDE.md, re-derive the whole skip set from a fresh `SpecDiscovery` run at the end — one fix often clears stale skips elsewhere.

## 6. Milestones

- **M5.1 — Representation & byte-identical parsing** (no behavior change): heap-type enum, `ReferenceType`, additive enum members, reader/writer s33 support, wide siblings + computed narrow views on all scalar sites, `WebAssemblyType` sidecar, composite-form rejection in both parse paths, round-trip test suite.
- **M5.2 — Validator currency swap** (pure refactor, full suite stays green): `ValType`, `TypeAlgebra`, `Stack<ValType?>`, `Signature`/`GlobalInfo`/locals/tables → `ValType`, subtype-relax the equality sites (incl. `BranchTable` per-label, `Return*` results), fix the `FunctionOuterBlock` ref-to-Int32 default while there.
- **M5.3 — Non-defaultable locals**: init tracking per §4, unit tests replicating all seven `local_init` shapes.
- **M5.4 — Instructions & typed tables/globals/elements**: the five new instruction classes + upgrades + `Compile.cs` section work, per-instruction test files incl. the 5M-deep `ReturnCallRefTests` proof.
- **M5.5 — Linking, runner, curation**: descriptor-based import/export validation, runner trap/invalid cases, full skip-set re-derivation, Debug+Release on net8/9/10.

## 7. Risks, ranked

1. **Validator currency swap ripple (M5.2)** — touches nearly every instruction.
   *Mitigation*: the implicit `WebAssemblyValueType → ValType` conversion keeps most call sites textually unchanged; land M5.2 as a standalone no-behavior-change step with the full suite green before any new semantics.
2. **`return_call_ref` tail-call reality** — the suite itself recurses 1,000,000 deep.
   *Mitigation*: identical IL shape to the proven Phase 2 `return_call_indirect`.
   *Stop-and-report trigger*: if the 5M-deep proof test stack-overflows in any (framework × config) cell, stop — a non-tail fallback cannot pass the fixtures, so the shape needs rework, not a silent degrade.
3. **`br_on_null`/`br_on_non_null` vs. the result-ferry machinery** — the branch path must stash label results (possibly multi-value, possibly including the ref itself) exactly like `Branch`/`BranchIf`.
   *Mitigation*: compose `BranchHelper` rather than re-derive; dedicated tests for every block-result arity; emit nothing in unreachable code.
4. **Cross-module type fidelity for linking** (descriptor design).
   *Mitigation*: contained in one new file + attribute field; degenerate fallback is today's CLR-type check.
   *Honest fallback*: if a specific assert needs more than closed structural descriptors, leave that line skipped with a precise note rather than approximating.
5. **Runtime type identity is CLR-shape-coarse** — `(func (param funcref))` vs `(func (param (ref $t)))` share a CLR delegate type, so a `call_indirect`/`call_ref` cross-type castclass can't distinguish them at runtime.
   Pre-existing coarseness; no Phase 5 fixture found that requires finer runtime identity.
   *Stop-and-report trigger*: a ratified fixture failing on this — it would need per-entry type tags in `FunctionTable`, a design decision the maintainer should weigh (it foreshadows Phase 8 canonicalization).
6. **Recursive structural equality** of function types (self-referential single-type rec groups are legal 3.0).
   *Mitigation*: bisimulation-with-assumption from day one (small); Phase 5 fixtures don't exercise it, Phase 8 will.
7. **`table.6` (min 2³²−1 funcref `module_definition`)** — reason for its current failure unverified; likely clears with the varuint/limits work already done, or exposes an allocation-at-compile-time issue.
   *Mitigation*: investigate first during M5.4; it's a single line.

## Implementation notes (deviations, appended during build)

Only places where the build departed from the design above; the design's constraints (additive API, byte-identical round-trips) held throughout.

- **M5.1 — `ReferenceType.WriteTo` lead byte.** The `0x63`/`0x64` lead byte must be emitted via `Writer.WriteVar` (its LEB s7 form), not `Write((byte)(sbyte)…)`. The enum values are `-0x1C`/`-0x1D`, whose raw bytes are `0xE4`/`0xE3`; only the LEB encoding produces the wire bytes `0x64`/`0x63`. Same care applies to element/value-type shorthands, whose single wire byte (e.g. funcref `0x70`) is the LEB s7 of `-0x10`, not the raw byte `0xF0` — so the `Table` reader/writer route through `ReadVarInt7`/`WriteVar` rather than raw bytes. This is an encoding detail the design left implicit; getting it wrong is caught immediately by the round-trip suite.
- **M5.1 — `Table` `0x40` detection.** The with-initializer prefix is detected by reading the lead as a `ReadVarInt7` and testing for s7 `-0x40` (rather than a raw-byte `0x40` peek), which avoids needing a seekable stream and keeps the element-type LEB decode correct.
- **M5.1 — `Compile.cs` shares the object-model `Element`/`Table`/`Global` readers.** The design (§1) says "Compile.cs re-parses the binary independently"; that is true for the type section (`Signature`) and function locals, but the Element/Table/Global sections are read through the same object-model classes. Consequently M5.1's permissive element-reftype parsing reached `Compile` before any validator existed, so `ref` line 47 (`elem (ref 1)`, an out-of-range-index `assert_invalid "unknown type"`) stopped throwing at parse. It is temporarily added to the `ref` skip list with a note; the out-of-range "unknown type" rejection is validator work (M5.2/M5.4) and the skip is expected to be removed in M5.5. No other category regressed (verified: only `ref` moved, and it stays Inconclusive).
- **M5.1 — narrow view for `RefNull`.** Because each value-type shorthand shares the exact s7 value of its heap type (`FuncRef == Func == -0x10`, etc.), `RefNull.Type` maps abstract heap types to the matching shorthand by a direct `(WebAssemblyValueType)(sbyte)heapType` cast, and a concrete index reports `NullableReference`. No separate lookup table was needed.
- **M5.2 — `ValType` currency scope.** The stack-facing currency was swapped to `ValType` exactly as designed: `Stack<ValType?>`, the whole `PopStack`/`PopStackNoReturn`/`ValidateStack` family, `Signature.Raw{Parameter,Return}Types`, `GlobalInfo.Type`, `CompilationContext.Locals`, `GetOrCreateResultLocal`/`DeclareResultLocals`, `TailCallHelper.SpillOperands`, and `BranchHelper.BranchTypes`. The implicit `WebAssemblyValueType → ValType` conversion kept the numeric call sites textually unchanged; the sites that break under it (a `switch` over a popped type, or passing `ValType.Value` to a `WebAssemblyValueType` API) were converted explicitly via `ToNarrow()` — `Select`, `RefIsNull`, `Return`/`ReturnCall`/`ReturnCallIndirect`/`Call`/`CallIndirect` (the `.Cast<WebAssemblyValueType?>()` immediates became `.Select(t => (ValType?)t)`). **Deferred to M5.4:** the *table/element* currency (`TableElementTypes`, `ElementSegmentTypes`, and the `Compile.cs` global-content parse) stayed on the `ElementType`/`WebAssemblyValueType` enums — converting them requires the typed-table work and would risk the zero-behavior gate, so they were left for the milestone that actually adds typed tables. `Signature` and the private `Compile.Local` **do** now parse full reftypes into `ValType` (needed so the currency swap doesn't throw on the `Reference`/`NullableReference` narrow forms).
- **M5.2 — "unknown type" bounds check landed here, not M5.4.** M5.1's permissive reftype parsing reaches `Compile` (per the note above), so once `Signature`/`Compile.Local` parsed concrete-index references cleanly, several `assert_invalid "unknown type"` lines (`ref` 56/60/64 in locals, `ref.1` in a type-section param) stopped throwing. The design (§2) lists the type-index bounds check as M5.2/M5.4 work; it was implemented now for the type section and function-body locals (`Compile.ValidateTypeIndex`, out-of-range concrete index ⇒ `ModuleLoadException("unknown type …")`). Globals/tables/elements/instruction-immediate index bounds remain for M5.4 (no current fixture needs them to preserve the gate). This did **not** let `ref` 47 be removed yet (that path is the element section, still M5.4/M5.5).
- **M5.2 — `type-rec`/`type-equivalence` forward-reference skips.** `type-rec.1` (line 22) and `type-equivalence.5` (line 77) reference a *later* type index across singleton rec groups — an `assert_invalid "unknown type"` that only the Phase 8 rec-group scoping rules reject (the referenced index is in range, so the M5.2 bounds check correctly does *not* fire). They threw at the M5.1 baseline only by accident of the reftype mis-parse; with clean parsing they now load, so both lines were added to their categories' skip lists as Phase 8 deferrals. The categories were already Inconclusive, so the skip *count* is unchanged.
- **M5.2 — `BranchTable` per-label validation.** The pre-3.0 "all labels agree exactly" `SequenceEqual` was replaced by independent per-label subtype validation of the operand types against each label (equal arity still required; `BranchHelper.ValidateBranchTypes` used non-mutatingly per label, reachable code only). The single-value default keeps its unconditional `context.ValidateStack` call — it must run **even in unreachable code**, because concrete values pushed after `unreachable` are still type-checked by the reference algorithm (this is exactly what `unreached-invalid.85` = `br_table` num-vs-num-after-unreachable exercises; guarding it behind reachability was a regression caught by the gate).
- **M5.2 — `FunctionOuterBlock` default.** `Reset` now derives the outer block's single-result `BlockType` from the return type's `ValType.ToBlockType()` (narrow s7 cast) instead of the old `_ => BlockType.Int32` fallback that silently mistyped v128 and every reference return.
- **M5.4 (instruction half) — five instructions + upgrades done; typed sections deferred.** All five new instruction classes are implemented with the five-touch pattern and per-instruction tests: `CallRef` (0x14, a per-type-index static caller `(params…, Delegate) → results` cached in `DelegateCallersByTypeIndex`, mirroring `CallIndirect`'s remapper minus the table), `ReturnCallRef` (0x15, `ReturnCallIndirect`'s shape with the table lookup replaced by a spilled `Delegate` operand — the 5,000,000-deep mutual-recursion `ReturnCallRefTests` proof passes on net8/9/10 × Debug/Release, so the risk-2 trigger is cleared and it did **not** degrade to a non-tail fallback), `RefAsNonNull` (0xD4, `dup; brtrue CONT; throw NullReferenceException; CONT:`, emitting nothing in unreachable code), and `BranchOnNull`/`BranchOnNonNull` (0xD5/0xD6). The branch pair composes a new `BranchHelper.EmitReachableBranch` (extracted verbatim from `Branch`'s reachable value-ferrying so `Branch` is unchanged) — `br_on_null` = `dup; brtrue FALL; pop; <branch below-values>; FALL:` keeping the non-null ref; `br_on_non_null` = `dup; brfalse NULL; <branch below-values + ref>; NULL: pop` dropping it on fall-through. Upgrades landed: `RefNull` pushes the exact `(ref null ht)`/concrete-index type (bounds-checked), `RefFunc` pushes the typed `(ref $sig(f))`, `RefIsNull` accepts any reference. **Deferred to the rest of M5.4/M5.5** (spec categories stay skipped): inline **long-form** reference block results (`block (result (ref $t))` — `BlockType.TryToValueType` still only covers funcref/externref, so the branch-to-single-inline-ref-result case and `local_init.0` are not yet handled; multi-value/type-index blocks with ref results already work), typed `SelectWithType`, and the `Compile.cs` typed tables/globals/elements + `0x40 0x00` table-with-initializer + `table.6`. The branch instructions have basic null/non-null path tests but still need the full zero/single/multi-value + branch-in-unreachable arity matrix before their categories flip (risk 3).
- **M5.3 — definite assignment implemented and unit-tested; `local_init` category deferred to M5.4.** The tracking (`CompilationContext.LocalIsInitialized`/`InitLog`, `BlockContext.InitLogHeight`, `MarkLocalInitialized`/`IsLocalInitialized`/`RollbackInit`, hooks in `LocalGet`/`LocalSet`/`LocalTee`/`MakeBlockContext`/`Else`/`End`, `Reset` seeding) is complete and covered by `LocalInitTests` (all seven fixture shapes, using `(ref func)` param/local so nothing depends on M5.4). The runner learned the `"uninitialized local"` invalid text. The `local_init` **spec category itself** could not flip yet: its valid functions use inline **reference block results** (`block (result (ref extern))`), which the block-result machinery (`BlockType.TryToValueType`, `End`/`Branch` single-value paths) does not yet handle — that is the `BlockTypeInstruction` upgrade in §3, deferred to M5.4. So `local_init` stays `RunAllSkipped` with a note pointing at M5.4, and flips in M5.5.
- **M5.4 (remainder) — inline reference block results via a synthetic signature, not a plumbed `TryToValueType`.** Rather than teach every single-value block path (`End`/`Else`/`Branch`/`BranchIf`/`BranchTable`) to understand `BlockTypeInstruction.RefType`, `MakeBlockContext` synthesizes a zero-parameter, single-result `Signature` for an inline reference result and routes it through the **existing multi-value `BlockSignature` machinery**. This is a much smaller change and reuses proven ferrying. A new `Signature(ValType, CompilerConfiguration)` ctor backs it. The block-type type-index bounds check was extracted to `BlockTypeInstruction.ValidateBlockTypeIndex` and called at the **start** of `If.Compile` (before the condition pop) so an out-of-range `if (result (ref N))` reports "unknown type" rather than a stack-size error (ref.11); `SelectWithType` and `RefNull` got the same bounds check inline (ref.12/ref.9).
- **M5.4 (remainder) — `br_on_non_null` fall-through re-types to the label prefix.** The rule is `[t* (ref null ht)] → [t*]` where the *label's* `t*` is imposed, not the operand-below's actual (possibly more specific) types. `BranchOnNonNull` therefore pops the prefix values and re-pushes the label's `t*` on the fall-through, which is what makes `br_on_non_null.3` invalid (a `(ref null $t)` below the operand becomes `funcref`, so a subsequent `call $f` expecting `(ref null $t)` is a type mismatch). Validation also requires the label's *last* type to be a reference; a single-value shorthand-ref label (`block (result funcref)`) has no multi-value branch types, so the inline block type supplies that last type. The label's last type may be **nullable** (`br_on_non_null.0` `$n2` branches to a `(ref null $t)`-result block) — the earlier "must be non-null" reading was wrong.
- **M5.4 (remainder) — `BranchTable` "meet" fix + current-frame availability.** Two 3.0 br_table bugs surfaced when the category flipped: (1) the single-value default label was validated with `ValidateStack`, which *replaced* the actual (more specific) stack type with the default label's type, breaking the per-label subtype "meet" checks (`meet-funcref-*`) — now validated non-destructively in reachable code, keeping the `ValidateStack` behavior only for the dead-code path (`unreached-invalid.85`); (2) `BranchHelper.ValidateBranchTypes` measured available carried values from the *target* block's baseline, over-counting values hidden beneath the innermost frame — now measured from the current control frame's baseline (`br_table.10` `type-arg-void-vs-num-nested`). This shared helper change affects all branch instructions and left every prior category green.
- **M5.4 (remainder) — typed tables/globals/elements: `ValType` currency + func-index elem type.** `CompilationContext.TableElementTypes`/`ElementSegmentTypes` and `GlobalInfo.Type` became `ValType`; `Compile.cs`'s defined/imported Table, defined/imported Global, and Element sections all read full reftypes (`ReadTableElementTypeFromLead`, `ReferenceType.ReadValueType`), bounds-checked via `ValidateTypeIndex`. The `0x40 0x00` table-with-initializer form is handled by a new `EmitTableDefinition` that also validates the initializer (`ValidateElementInitExpr`, which became a real subtype check accepting typed `ref.func`, bottom `ref.null`, and `global.get`) and fills every slot `[0, min)` in a constructor loop. **Deviation from the design's implicit assumption:** the func-index (kind 0/1/3) element form produces **non-null** function references, so its segment element type is `(ref func)` (not the nullable `funcref` shorthand) — that is what lets `elem.2` initialize a `(ref func)` table from a `func $g` segment while still satisfying a plain `funcref` table. `call_indirect` now explicitly rejects a non-func-hierarchy table as a "type mismatch". `table.6` (design risk 7) needed no work.
- **M5.5 (mostly) — runner + curation done; linking descriptors deferred (risk 4/5).** The runner maps `"null function reference"`/`"null reference"` → `NullReferenceException`, and `CompareReturnValue` accepts an unspecified `(ref.func)` expectation as any non-null `Delegate` (the spec pins only the funcref *type*, not identity — the CLR-shape-coarse reality of risk 5, but here it only affects result comparison, so no fixture is *stopped*). Every Phase 5 category listed in §5 flipped green; the skip set was re-derived from a fresh `SpecDiscovery` run (52 → 36), which also cleared many stale `linking` and `type-equivalence` skips that the typed-ref parsing + bounds checks fixed as a side effect. **The one piece left is the import/export linking descriptors (§3g):** `table:93,112–116` and `linking:112,182,186,190,442,451` remain skipped because a structurally-identical `(ref $f)`/`(ref $dummy)` (or an incompatible pair) share one CLR `Delegate`/`object` shape, so the coarse `GlobalImport.GetterType`/table-cast check can neither confirm a valid supply nor reject an invalid one. This needs `WasmTypeDescriptor` + an additive `NativeExportAttribute` descriptor member + `GlobalImport`/`TableImport` descriptor fields + `ImportException.ValidateGlobal`/`ValidateTable` descriptor overloads (immutable global supplied <: expected; mutable + table elem invariant) with the CLR-type fallback. It was left as a clean, well-scoped next step rather than risk the green gate near the budget limit.
- **M5.5 (final) — linking descriptors landed; all twelve lines flipped green, none left skipped.** Phase 5 is complete. The descriptor is a closed, canonical string (`WasmTypeDescriptor`): `ref [null ]heap`, where `heap` is an abstract name (`func`, `extern`, `any`, `eq`, `i31`, `struct`, `array`, `none`, `nofunc`, `noextern`, `exn`, `noexn`) or a concrete function type `func(v,…)->(v,…)` whose elements are value descriptors (`i32`/`i64`/`f32`/`f64`/`v128` or a nested `ref …`) — every concrete type index is expanded recursively into its structural signature with a depth-based back-reference cycle guard, so the string is self-contained and two independently-compiled modules render structurally-equal types identically. Only reference types get a descriptor; numerics render `null`. **Why the format suffices for the exact relations the fixtures need:** because Phase 5 has no declared (`sub`) subtyping, two *distinct* concrete function types are subtypes only when structurally equal, which under canonicalization is byte-identical strings. That collapses all the required comparisons to (a) exact string equality — the invariant case for mutable globals (`linking:182/186/190`) and table element types (`linking:442/451`) — and (b) a shallow subtype over the *outer* reference layer only: nullable-relaxation plus a heap lattice where a concrete-func string (`func(`…) `<:` abstract `func`, and `nofunc <:` anything func-shaped (`linking:112`, `table:93/112–116`). No recursion into params/results is ever needed at *compare* time (only at *render* time), so `IsSubtype` is a small string-parse of `ref [null ]…` plus a `switch` mirroring `TypeAlgebra`'s abstract lattice. The descriptor rides an additive `NativeExportAttribute.Descriptor` (init-only string, emitted via a named-property `CustomAttributeBuilder`; the persisted-assembly path already skips the attribute, so it is unaffected); `RuntimeImport.FromCompiledExports` copies it onto `GlobalImport.Descriptor`/`TableImport.Descriptor`; `Compile` emits the *expected* descriptor as a constant into new `ImportException.ValidateGlobal`/`ValidateTable` overloads (the pre-descriptor overloads are kept for public-API compatibility, selected apart by explicit parameter-type `GetMethod`). Both sides absent ⇒ fall back to the coarse CLR value-type/cast check, so every host-supplied and numeric import is byte-for-byte unchanged (the existing `ImportValidationTests` stay green). **Nothing was left skipped under risk 4** — the closed structural descriptor expressed every relation the twelve asserts required. Risk 5 (CLR-shape-coarse runtime identity) was never triggered: all twelve are *link-time* type checks, which the descriptor resolves exactly; it does not attempt finer *runtime* `call_indirect`/`call_ref` identity, and no ratified fixture demands it. Counts after the fix: net8.0 928 passed, net9.0/net10.0 931 passed, 34 skipped, 0 failed, Debug and Release identical.
