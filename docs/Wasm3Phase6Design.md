# Phase 6 — Exception Handling (`try_table` model): Design

Design record for Phase 6 of [the WASM 3.0 upgrade plan](Wasm3Plan.md), produced 2026-07-05 from codebase exploration and empirical inspection of the checked-in fixtures with `wasm-tools dump`/`print`.
Follows the Phase 5 additive-API philosophy ([Wasm3Phase5Design.md](Wasm3Phase5Design.md)): every existing encoding round-trips byte-identically, every existing public member keeps its exact behavior, and new surface is additive.

## Verified ground truth (fixtures + binary encodings)

- **Tag section**: id **13** (`0x0d`), positioned **between Memory (5) and Global (6)** (verified in `imports.0.wasm`: `03` func section → `0d 07` tag section → `06` global section).
  Entry encoding: attribute byte `0x00` (kind Exception; the only defined value) + `varuint32` type index.
  The referenced function type must have **empty results** (`tag.2`/`tag.3` are `assert_invalid "non-empty tag result type"`).
- **ExternalKind.Tag = 4**: import = kind byte `04` + tag-type (attr `00` + typeidx); export = kind byte `04` + tag index (verified in `imports.0`/`imports.5` dumps).
- **Opcodes**: `throw` = `0x08` + tagidx; `throw_ref` = `0x0a` (no immediate); `try_table` = `0x1f` + blocktype (same s33 as `block`) + `vec(catch)`.
  Catch clause: discriminator byte `0x00` catch (tagidx, labelidx), `0x01` catch_ref (tagidx, labelidx), `0x02` catch_all (labelidx), `0x03` catch_all_ref (labelidx).
  Verified: `1f 7f 01 00 02 00` = `try_table (result i32) (catch tag:2 label:0)`; `1f 40 01 02 00` = catch_all.
- **Catch labels are resolved in the context *enclosing* the try_table** (label 0 = the innermost block *around* it — verified: `block $h { try_table (catch $e0 $h) … }` encodes label 0, and the two-handler case encodes `$h0`=1, `$h1`=0).
  A catch clause **branches** to that label with the tag's parameters (plus the `exnref` for `_ref` forms) as branch operands; it never falls into a handler body.
- **Catch label may be the function boundary**: `try_table.5` has `try_table (catch $e 0)` at function top level.
- **try_table can carry params** (`try-with-param`: `try_table (type 1) (param i32)`), multi-value results, and can be **catchless** (`catchless-try`) — a catchless try_table is semantically a plain block.
- **`return_call` / `return_call_indirect` inside try_table ARE exercised** (`try_table.1` funcs 22/23, lines 310/311) and are **`assert_exception`**: the tail call must *replace* the frame so the callee's throw is **not** caught by the enclosing try_table.
  A naive "plain call + ret inside the try" fallback is semantically **wrong**, not just slower.
- **Traps must escape catch_all**: `unreachable-not-caught` (assert_trap "unreachable" through `catch_all`) and `trap-in-callee` (assert_trap "integer divide by zero" through `catch_all`) assert it directly.
- **Cross-module tag identity**: `try_table.1` imports `"test" "e0"` **twice**; a catch on one import must catch a throw of the alias, and must catch an exception thrown by the *provider* module's own exported `throw` function.
  Tag identity is per *instance-level tag object*, shared through linking.
- **Typed-ref tag params**: `try_table.13` has `(tag $e (param (ref $t)))` and catch labels with `(result (ref null $t) (ref exn))`; `try_table.14`/`15` are `assert_invalid "type mismatch"` where the tag's param is only a *supertype-incompatible* match for the label (`(ref null $t)` vs label `(ref $t)`) — catch-clause validation is a per-element **subtype** check.
- **`assert_exception` carries no expected tag** — only an action: `{"type":"assert_exception","line":…,"action":{invoke…}}`.
  It asserts the action throws a WASM exception (any tag); a trap or a normal return fails it.
- **exnref flow**: `catch_ref` sends `[t* exnref]` to the label; `throw_ref` rethrows an exnref preserving identity (`throw_ref-recatch` catches the same exception a second time); exnref is stored in locals, block results, and function results (`(result exnref)`); `ref_null`'s exn lines already pass from Phase 5 (heap types `exn`/`noexn` parse, `ValType` maps the exn hierarchy to `object`).
  No fixture traps on `throw_ref` of null, but the spec requires the "null reference" trap and it will be implemented (reusing the Phase 5 `ThrowNullReference` helper).
- **rec groups contaminate `exceptions/tag`**: `tag.4`–`tag.7` (lines 30, 38, 40, 49, 60) all use `(rec …)` type groups, which both parse paths reject as Phase 8.
  These five lines **stay skipped** with a Phase 8 note; the rest of the category flips.
- **New harness texts**: invalid `"non-empty tag result type"`, `"unknown tag 0"`, `"type mismatch: instruction requires [i32] but stack has …"` (prefixed form); unlinkable `"incompatible import"` (shorter than the existing `"incompatible import type"`); malformed lines are `.wat` (auto-skipped, no WAT parser).
- **Parse-path camps** (resolved by reading the code): the Import section is parsed **independently** in `Compile.SectionImport` (its own `ExternalKind` switch) and in `Import.ParseFrom` — tag imports land twice.
  The Tag *section* is new; it will follow the **shared-reader camp** (like Element/Table/Global): one `Tag(Reader)` ctor used by both `Module.ReadFromBinary` and a new `case Section.Tag` in `Compile.FromBinary`.

## 1. Object model (additive)

**New file `WebAssembly/Tag.cs`** — `public class Tag`: `uint TypeIndex` (the exception signature, a func type with empty results), reader ctor `Tag(Reader)` (reads the attribute byte, throws `ModuleLoadException` on non-zero — "Unsupported tag attribute", future-proof against shared/other kinds), `WriteTo(Writer)` (emits `00` + typeidx), `Equals`/`GetHashCode`/`ToString` in the house style (mirror `Function.cs`).

**`WebAssembly/Section.cs`**: add `Tag = 13`.
Update `IsValid` (currently `section <= Section.DataCount` — must accept 13) and `BinaryOrder` (renumber the internal switch so Tag sorts between Memory and Global: None 0, Type 1, Import 2, Function 3, Table 4, Memory 5, **Tag 6**, Global 7, Export 8, Start 9, Element 10, DataCount 11, Code 12, Data 13).

**`WebAssembly/ExternalKind.cs`**: add `Tag = 4`.

**`WebAssembly/Module.cs`**: `IList<Tag> Tags` property (same lazy pattern as `Memories`); `case Section.Tag` in `ReadFromBinary`; `WriteToBinary` emits the Tag section between Memory and Global plus `WriteCustomSection(…, Section.Tag, …)`.
A module without tags emits no Tag section — byte-identical round-trips for all existing modules.

**`WebAssembly/Import.cs`**: new `public class Import.Tag : Import` (`uint TypeIndex`), new arm in `ParseFrom` for kind 4, `WriteTo` counterpart.
`Export.cs` needs nothing beyond the enum member (its reader/writer pass the kind byte through).

**Instruction classes** (five-touch pattern):

- `Instructions/Throw.cs` — immediate `uint Index` (tag index). OpCode enum: `Throw = 0x08`.
- `Instructions/ThrowRef.cs` — `SimpleInstruction`. `ThrowRef = 0x0a`.
- `Instructions/TryTable.cs` — derives `BlockTypeInstruction` (reuses the s33 blocktype reader incl. Phase 5 inline-ref results), plus `IList<CatchClause> Catches`.
  **`BlockTypeInstruction.WriteTo` is currently `internal sealed override`** — unseal it (internal member, not public API): extract the blocktype emission into a shared method, let `TryTable` override to append the catch vector.
  `TryTable = 0x1f` in `OpCode.cs`.
- **New file `WebAssembly/CatchClause.cs`** — `public class CatchClause`: `CatchKind Kind` (`public enum CatchKind : byte { Catch = 0, CatchRef = 1, CatchAll = 2, CatchAllRef = 3 }`), `uint Tag` (meaningful for Catch/CatchRef), `uint Label`, reader ctor, `WriteTo`, equality members.
- `Instruction.cs`: dispatch `case OpCode.Throw/ThrowRef/TryTable` in the **general** parser only (none are constant-expression opcodes).
  `try_table` must also be pushed on the parser's block-depth counter exactly like `Block`/`Loop`/`If` so `Instruction.Parse` finds the matching `End`.

Do **not** implement the legacy proposal's `try`/`catch`/`delegate`/`rethrow` (0x06/0x07/0x09/0x18/0x19) — not part of ratified 3.0.

## 2. Runtime types: tag identity, the exception, exnref

**Tag identity — what a tag IS at runtime**: a unique reference-identity object, one per (instance × tag definition), flowing through linking by reference.
Mirroring the `TableImport`/`FunctionTable` split:

- **New file `Runtime/TagImport.cs`** — `public abstract class TagImport : RuntimeImport` (`Kind => ExternalKind.Tag`), with an internal `string? Descriptor` (the Phase 5 `WasmTypeDescriptor` pattern).
- **New file `Runtime/ExceptionTag.cs`** — `public sealed class ExceptionTag : TagImport`.
  Identity is CLR reference identity; it carries no mutable state.
  Ctors: `ExceptionTag()` (niladic signature), `ExceptionTag(params WebAssemblyValueType[] parameterTypes)` (host convenience; renders the descriptor from the narrow types), and `ExceptionTag(string? descriptor)` (public because compiled-module IL must call it with the closed structural descriptor; documented as infrastructure, same precedent as the public `NativeExportAttribute.Descriptor`).

Because imports store and re-export the **same** `ExceptionTag` instance (see §3), the `imported-e0`/`imported-e0-alias` aliasing and provider-thrown/importer-caught cases work by construction.

**The WASM exception** — **new file `Runtime/WebAssemblyException.cs`**: `public sealed class WebAssemblyException : RuntimeException` with `public ExceptionTag Tag { get; }` and an `object?[]` of boxed operand values (public read via `IReadOnlyList<object?>` or `GetArgument(int)`; internal field access for IL).
Constructor `WebAssemblyException(ExceptionTag tag, object?[] arguments)` — public, called from emitted IL.
Message renders the tag/arity for diagnostics.
**Invariant that guarantees trap separation: no library code path ever throws `WebAssemblyException` except the compiled `throw`/`throw_ref` instructions.**

**exnref runtime representation**: the `WebAssemblyException` instance itself, typed `object` on the CLR stack/locals — exactly what Phase 5's `ValType.ToSystemType()` already returns for the exn hierarchy.
`catch_ref` ferries the caught exception object; `throw_ref` rethrows it; no new `ValType` machinery is needed (nullable exnref locals are already defaultable, `ref_null`'s exn lines already pass).

**Trap taxonomy — what stays outside**: the complete set of trap exceptions the library throws today, all of which must (and, under the filter design in §4, structurally do) pass through `catch_all`: `UnreachableException`, `DivideByZeroException`, `OverflowException` (integer overflow, invalid float→int conversion, Phase 4 overflow-checked u64 address arithmetic), `NullReferenceException` (Phase 5 null funcref/`ref.as_non_null`, uninitialized elements), `MemoryAccessOutOfRangeException`, `TableAccessOutOfRangeException`, `IndexOutOfRangeException`, `InvalidCastException` (indirect call type mismatch), `KeyNotFoundException` (uninitialized element legacy path), plus `StackOverflowException`/`OutOfMemoryException` and any host-import exception.
The boundary rule is a single type test: **a WASM exception is an `isinst WebAssemblyException` success; everything else is a trap or host failure and unwinds through every filter untouched.**
`WebAssemblyException : RuntimeException` does not blur this: `UnreachableException`/`ImportException` are siblings, not subtypes.

## 3. Tags through `Compile.cs` (sections, imports, exports, linking)

`CompilationContext` gains `List<FieldBuilder> Tags` (imported first, then defined, index order) and `List<Signature> TagSignatures` (parallel).

- **`case Section.Tag` (id 13)** in the `FromBinary` switch: per entry, read via the shared `Tag(Reader)` ctor; validate the type index (out-of-range typeidx ⇒ existing "unknown type" path) and **empty results** ⇒ `ModuleLoadException("non-empty tag result type …")`.
  Define a private readonly field of type `ExceptionTag` (NeutralizeType'd for the persisted path), emit into the instance constructor `ldarg.0; ldstr <descriptor>; newobj ExceptionTag(string); stfld` — fresh identity per instantiation, as the spec requires.
  Descriptor = a new internal `WasmTypeDescriptor.RenderTagSignature(Signature, Signature[])` rendering `func(<params>)->()` with concrete refs expanded structurally (reuses the Phase 5 renderer).
- **Tag imports** in `SectionImport` (`case ExternalKind.Tag`): read attr byte (reject non-zero) + typeidx; validate empty results; define the `ExceptionTag` field; emit `findImport` → `ImportException.EmitTryCast(…, typeof(TagImport))` → new **`ImportException.ValidateTag(TagImport, string? expectedDescriptor, string module, string field)`** (returns the `ExceptionTag`; static, public, following `ValidateTable`'s shape) → `stfld`.
  Tag import matching is **invariant** (exact descriptor equality; both-null falls back to accept) — this is what makes `imports.42–45` and `tag.json`'s `"incompatible import"` reject and `imports.5`'s matching pair link.
  The *same object* is stored, preserving identity.
- **Tag exports** in `SectionExport` (`case ExternalKind.Tag`): a public getter property returning the tag field (CLR type `ExceptionTag`), decorated `NativeExportAttribute(ExternalKind.Tag, name, descriptor)`.
- **`RuntimeImport.FromCompiledExports`**: new `case ExternalKind.Tag` — invoke the getter, take the `ExceptionTag` (it *is* a `TagImport`), copy `native.Descriptor` onto it, yield.
  This closes the register/`AddFromExports` loop the spec harness uses (`register "test"` → later modules import `test.tag-i32`).

**Section-order guarantee**: Tag (BinaryOrder 6) precedes Global/Element/Code, so tag fields exist before any `throw` compiles and before the start function runs; imported tags exist from `SectionImport`.
No disposal needed (tags hold no unmanaged state).

## 4. `try_table` IL mapping — the core

### The three IL facts that shape everything

1. ECMA-335 requires the evaluation stack to be **empty at try entry**, and control may leave a protected region **only via `leave`** (or an exception); `leave` **clears the evaluation stack**.
2. The existing codegen keeps the whole WASM operand stack **physically on the IL evaluation stack**; branches ferry *carried values* through locals (`BlockContext.ResultLocal(s)`, `BranchHelper.StashResults`) but values *below the target's baseline* stay physically on the stack across the jump.
3. Only filters can discriminate tags, so catch clauses compile to **filter handlers** (`BeginExceptFilterBlock`), tried in clause order — which matches WASM's first-matching-clause semantics because the CLR consults filters in order.

### Protected-region model

`CompilationContext` gains a stack of **protected-region records**, one per *live* try_table compiled in reachable code:

```
sealed class ProtectedRegion {
    int SpillStart;              // physical floor of the enclosing region (or 0): first stack slot this region spilled
    LocalBuilder[] SpillLocals;  // the spilled segment [SpillStart, entry stack height incl. block params)
    int Floor;                   // = try_table BlockContext.InitialStackSize: physical floor inside the region
    List<CatchClause> Clauses;   // + resolved per-clause (tag field, target depth key, target label, branch types)
    Dictionary<int, ExitStub> ExitStubs;  // keyed by target depth key
    bool EmitsIL;                // false when the try_table started in unreachable code (validation-only mode)
}
```

plus `bool InProtectedRegion`, `int PhysicalFloor` (innermost region's `Floor`, else 0), and `List<Action<CompilationContext>> PendingFunctionStubs` (cleared in `Reset`, drained by `SectionCode` after the body loop — IL after the final `ret` is a legal place for trampolines, and by then every exception block is closed, so `tail.` is legal there).

### `TryTable.Compile` (entry)

1. Validate the blocktype exactly like `Block` (`ValidateBlockTypeIndex`, `MakeBlockContext` — params/multi-value/inline-ref results all reuse the Phase 5 synthetic-signature machinery).
2. Validate every catch clause **against the context enclosing the try_table** (before pushing it on `Depth`): tag index in range (`ModuleLoadException "unknown tag N"`), label depth in range ("unknown label"), and the clause's sent types — tag params, plus `(ref exn)` appended for `_ref` forms — must be per-element **subtypes** of the target label's branch types with equal arity (`TypeAlgebra.IsSubtype`; failures surface through the runner's "type mismatch" family).
   This is what makes `try_table.14`/`15` invalid and `try_table.13` valid.
   Loop targets compare against loop *parameters*; the function boundary compares against the function results (the `Branch` conventions in `BranchHelper.BranchTypes`).
3. **Catchless try_table, or try_table starting in unreachable code ⇒ plain-block mode**: push Depth/labels/BlockContext exactly like `Block`, no EH IL, no region record (`catchless-try` and dead-code shapes).
4. Otherwise: **spill the physically-present stack** — every `context.Stack` slot in `[PhysicalFloor, Stack.Count)`, top-down `stloc` into fresh locals typed by `ValType.ToSystemType()` (all concrete in reachable code).
   `BeginExceptionBlock()`.
   Reload the try_table's *params* (the top `paramCount` spilled slots) inside the try.
   Push the region record; push Depth/label/BlockContext as usual.

### Branch rewiring: where the leave-vs-br decision lives

One choke point: **`BranchHelper.EmitBranchTransfer(context, targetDepthKey, label)`** replaces every raw `context.Emit(OpCodes.Br, label)` at a WASM-branch site (`Branch`, `BranchIf`'s taken paths, `BranchTable`'s per-target trampolines, `BranchOnNull`/`BranchOnNonNull`, and the dead-code "keep IL well-formed" emissions).
Logic:

- No protected region strictly deeper than the target ⇒ `br label` (byte-identical IL for every existing module — the zero-region fast path).
- Otherwise let **R** = the *outermost* region whose block depth is greater than the target's: get-or-create R's exit stub for this target and emit `leave stubLabel` (a single `leave` legally exits all nested regions between the source and R).

Ferrying interaction (the "is leave safe" audit): carried values are **already stashed into locals** before the jump (`StashResults` into the target's `ResultLocals`/`ResultLocal` — existing machinery), so `leave`'s stack-clearing destroys nothing that matters, **except**:
(a) `DiscardIntermediates` must be **skipped** on leave paths — the intermediates it would pop include slots below `PhysicalFloor` that are *not physically present*; popping them would corrupt the stack, and `leave` clears the physically-present remainder for free.
(b) **Loop targets**, whose normal path reloads carried params back *onto the stack* before `br`, instead stash into **stub-owned locals** and the stub reloads them.
(c) `BranchIf`'s `brtrue label` fast paths must fall back to the long shape (`brfalse.s skip; <ferry>; leave stub; skip: <reload>`) when crossing.
All of this lives in `BranchHelper` + a small `EmitReachableBranch` variant, gated on "crossing", so non-EH modules compile through untouched code.

**Exit stubs** (emitted by the try_table's `End`, immediately after `EndExceptionBlock` — lexically in R's parent scope, which is inside any region that also encloses the target, so a plain `br` suffices): `MarkLabel(stub)`; reload R's `SpillLocals[0 .. targetBaseline − SpillStart)` (reconstructing exactly the physical stack `[SpillStart, targetBaseline)` that non-crossing `br` paths deliver at the target label); for loop targets additionally reload the stub's carried-param locals; `br` the real target label.
This makes crossing and non-crossing paths arrive at every label with identical physical stack shapes — the IL merge-point consistency requirement.

**`Return` inside a region**: WASM `return` = branch to the function boundary.
Rework `Return.Compile`'s in-region path to stash results into the function block's `ResultLocal(s)` (the machinery `CompileFunctionEnd` already merges with) and `leave context.Labels[0]` directly — the function-end label is outside every region and is reached with an empty physical stack on all paths, so no stub is needed.
Outside regions: unchanged code path.

**`throw` inside its own try**: plain IL `throw` inside the try — caught by the local filters as required (`simple-throw-catch`).

### The try_table's `End`

Extend `End.Compile` (branch on the block context carrying a region record):

1. Existing result validation + stash into `ResultLocals`/`ResultLocal` (unchanged code).
2. Per catch clause, in order:
   - `BeginExceptFilterBlock()` (ILGenerator auto-inserts the body's `leave` to the block end). Filter IL (exception object incoming):
     - catch/catch_ref: `isinst WebAssemblyException; dup; brtrue.s HAVE; pop; ldc.i4.0; br.s DONE; HAVE: ldfld Tag; <EmitLoadThis(); ldfld tagField>; ceq; DONE: endfilter`.
     - catch_all/catch_all_ref: `isinst WebAssemblyException; ldnull; cgt.un; endfilter`.
   - `BeginCatchBlock(null)` — handler, exception object incoming: `castclass WebAssemblyException`, store to a temp; for each tag param i: `ldloc exn; ldfld Arguments; ldc.i4 i; ldelem.ref;` then `unbox.any T` (numerics/v128) or `castclass T` (reference types — `Delegate` for func-hierarchy per `ValType.ToSystemType`), `stloc ferry[i]` where `ferry` = the target label's `ResultLocals` (allocating them via the existing `DeclareResultLocals` path); for `_ref` forms additionally `ldloc exn; stloc ferry[n]` (the exnref rides last).
     Then transfer to the target via the same `EmitBranchTransfer` logic (a `leave` from a handler exiting its own and any enclosing regions is legal IL).
3. `EndExceptionBlock()`.
4. Mark the try_table's WASM end label (all `leave`s to it and the implicit fall-through leave land here with an empty stack), reload the region's `SpillLocals` (restoring `[SpillStart, Floor)`), then the existing `ResultLocals` reload — parent continues with an exact physical stack.
5. Emit the region's exit stubs; pop the region record.

The filter's `isinst WebAssemblyException` is the entire **traps-escape guarantee**: every trap type in §2's taxonomy, plus SOE/OOM/host exceptions, fails the `isinst` in every filter, including `catch_all`, and unwinds out of the module — exactly what `unreachable-not-caught` and `trap-in-callee` assert.
No type-based catch clauses are emitted at all.

### `throw` / `throw_ref` compilation

- **`Throw.Compile`**: validate tag index; pop the tag params (subtype-checked, reverse order); `MarkUnreachable()`.
  Reachable IL (inline; throw sites are rare enough not to warrant helpers): stash operands to temps (reverse), `<EmitLoadThis; ldfld tagField>`, `ldc.i4 n; newarr object`, per element `dup; ldc.i4 i; ldloc tmp_i; box/nop; stelem.ref`, `newobj WebAssemblyException(ExceptionTag, object[]); throw`.
  In unreachable code: validate only, emit nothing (the `throw-polymorphic` shapes).
- **`ThrowRef.Compile`**: pop an exnref-compatible ref; `MarkUnreachable()`.
  IL: `dup; brtrue.s OK; call ThrowNullReference("null reference"); OK: castclass WebAssemblyException; throw`.
  IL `throw` of an existing object preserves reference identity — the `Tag` and `Arguments` survive re-catch (`throw_ref-recatch`, `throw_ref-nested`); stack-trace reset is unobservable to the spec.

### The tail-call interaction (resolved without losing constant stack)

Fixtures prove the semantics (see ground truth): the tail-called callee's throw must escape.
Design: `ReturnCall`/`ReturnCallIndirect`/`ReturnCallRef`, when `context.InProtectedRegion`, spill their operands with `TailCallHelper.SpillOperands` **minus its trailing pops** (skipped entirely — `leave` clears), register a **per-callsite function-level stub** capturing the spilled locals and the callee, and emit `leave stubLabel`.
The stub — emitted by `SectionCode` after the body's final `ret`, *outside every protected region* — reloads the args and emits the **unchanged** `tail. call/callvirt Invoke; ret` sequence.
Both requirements met simultaneously: the frame's handlers are gone before the call (spec semantics), and `tail.` is legal because the call site is no longer inside a protected region (ECMA-335), so the constant-stack guarantee is **kept**, not surrendered.
The "am I inside a protected region" state lives on `CompilationContext` (`InProtectedRegion`, derived from the region stack).

## 5. Harness (`SpecTestRunner`)

- **`assert_exception`** (replacing the current deferral throw): resolve the action like `AssertTrap`, invoke unwrapping `TargetInvocationException`; pass iff the unwrapped exception **is `WebAssemblyException`**; a trap type, any other exception, or a normal return ⇒ `AssertFailedException` naming what happened.
  (The command carries no expected tag — nothing more to check.)
- **New text mappings**: `assert_invalid` — `"non-empty tag result type"` and prefix `"unknown tag"` ⇒ `ModuleLoadException`; generalize the exact `case "type mismatch"` to also match the prefixed `"type mismatch: instruction requires …"` (a `when StartsWith` arm beside it).
  `assert_unlinkable` — `"incompatible import"` joins `"incompatible import type"` ⇒ `ImportException`.
  `assert_trap` `"unreachable"` / `"integer divide by zero"` already mapped.
- `ExnRefValue` deserialization already exists; no exceptions fixture returns a non-null exnref to the harness (the `try_table.13` asserts expect `funcref`, which `CompareReturnValue` already handles as "any non-null Delegate").

## 6. Scope boundary — the checkable target

**Flips fully green**: `exceptions/throw` (all lines), `exceptions/throw_ref` (all), `exceptions/try_table` (all — the `.wat` malformed lines are shape-skipped already), and **`imports`** (all 20 skip lines trace to tags: 3/19 the tag-defining+registered module, 35 tag imports, 97/98/128–134/291–293 cascades, 240–256 tag link asserts — the whole skip list empties and the category goes green).

**Flips partially**: `exceptions/tag` — lines 3, 11, 13, 19, 23 go green; lines **30, 38, 40, 49, 60 stay skipped** (rec-group type identity, Phase 8) with a precise note; category stays Inconclusive by design.

**Unchanged**: `linking` is already fully green (verified — no tag lines exist there); `ref_null`'s exn/noexn lines already pass from Phase 5 (verified green, no skips); all `gc/*`, `relaxed-simd/*` untouched.
Per CLAUDE.md, re-derive the full skip set with a fresh `SpecDiscovery` run at the end.

## 7. Milestones (relay pattern — each ends fully green, Debug+Release, net8/9/10)

**M6.1 — Object model & byte-identical parsing** (no behavior change).
`Section.Tag` + `IsValid`/`BinaryOrder`, `Tag`, `ExternalKind.Tag`, `Import.Tag`, `Module.Tags` read/write, `CatchClause`, `Throw`/`ThrowRef`/`TryTable` classes (parse/write/equality; `Compile` may throw a clear placeholder), `BlockTypeInstruction.WriteTo` unseal, parse dispatch incl. block-depth accounting.
Tests: round-trip suite over every encoding (tag section, tag import/export, all four catch kinds, catchless, params/multi-value/inline-ref blocktypes) asserting byte-identical re-emission, plus a `Module`-built construction test.
Gate: full suite green, baseline 34 skips unchanged.

**M6.2 — Runtime tags, linking, harness.**
`TagImport`/`ExceptionTag`/`WebAssemblyException`, `WasmTypeDescriptor.RenderTagSignature`, `Compile.cs` `Section.Tag` + `SectionImport`/`SectionExport` tag arms + `ImportException.ValidateTag`, `FromCompiledExports` tag case, runner `assert_exception` + all new text mappings.
Tests: cross-module tag identity (export→register→import twice, alias equality), import mismatch rejection, host-supplied `ExceptionTag` import.
Flip: `exceptions/tag` (minus the five Phase 8 lines) and the 20 `imports` lines.
Gate: everything else unchanged.

**M6.3 — `throw` / `throw_ref` compile.**
Validation + IL per §4; per-instruction test files (`ThrowTests`, `ThrowRefTests`) covering each operand type, multi-operand, unreachable-code shapes, and the null-`throw_ref` "null reference" trap; a runtime test asserting a thrown tag surfaces to .NET callers as `WebAssemblyException` with correct `Tag`/`Arguments`.
Spec categories stay skipped (they need try_table).
Gate: full suite green.

**M6.4 — `try_table` core** (the phase's center of gravity).
Region records + spill/reload, `EmitBranchTransfer` choke point + `Branch`/`BranchIf`/`BranchTable`/`BranchOnNull`/`BranchOnNonNull`/`Return` rewiring (zero-region fast path byte-identical), `TryTable.Compile`/`End` epilogue with filters/handlers/stubs, catchless/unreachable plain-block modes, tail-call trampolines.
`TryTableTests` covering: each catch kind; operand extraction per value type incl. typed refs; nested try_tables incl. catch-to-outer and br-crossing-two-regions; try_table with params and with a value *below* it on the operand stack; loop back-edge crossing a region; `return` inside a region (single/multi-value); **a trap-escape test asserting every §2 trap type passes `catch_all`**; rethrow identity (catch_ref → throw_ref → second catch, same `Arguments`); the 5M-deep tail-call proof with the call site inside a try_table (constant stack + exception escape).
Gate: full suite green with categories still skipped.

**M6.5 — Curation & close-out.**
Flip `exceptions/throw`, `throw_ref`, `try_table`; re-derive the whole skip set via `SpecDiscovery`; verify `PersistedAssemblyTests` against a tag-using module (NeutralizeType coverage for `ExceptionTag`/`WebAssemblyException`); update `Wasm3Plan.md` Phase 6 checkboxes incl. the tail-call-resolution note; Debug+Release on net8/9/10.

## 8. Risks, ranked

1. **Branch rewiring regressions** — `EmitBranchTransfer` touches every branch instruction.
   *Mitigation*: zero-region fast path is the literal existing emission; land M6.4 only when the full pre-existing suite is green in both configs; the spill/stub machinery activates solely inside try_tables.
2. **Physical-vs-model stack arithmetic** (spill floors, stub reload extents, `DiscardIntermediates` capping) — the likeliest source of `InvalidProgramException`s.
   *Mitigation*: the M6.4 test matrix deliberately includes the below-baseline-value and multi-region-crossing shapes the spec suite mostly lacks.
   *Stop-and-report trigger*: a shape where merge-point stack consistency can't be met without restructuring block compilation — report rather than hack.
3. **ILGenerator filter-block behavior** (implicit `leave` insertion points, filter + `MarkLabel` interactions, Debug vs Release JIT strictness).
   *Mitigation*: prototype the minimal filter/handler/leave skeleton first inside M6.4; ILGenerator's auto-leave targets the post-`EndExceptionBlock` point, which is exactly our resume point, so we lean on it rather than fight it.
4. **Tail-call trampoline verifiability** — `tail.` emitted after the method's structural end, args via locals.
   Same operand kinds as the proven Phase 2/5 shapes.
   *Trigger*: if any (framework × config) cell stack-overflows the 5M proof or the JIT rejects post-`ret` trampolines, fall back to placing stubs before `ret` behind an unconditional jump — semantics unchanged.
5. **`exceptions/tag` rec-group lines** — unfixable until Phase 8; keep the five-line skip list precise so Phase 8 curation finds it.
6. **Public API shape** (`ExceptionTag(string)` ctor, `WebAssemblyException` naming) — maintainer-visible surface; all additive, follows the `NativeExportAttribute.Descriptor` precedent; flag in the M6.2 hand-off for the maintainer's naming veto.
7. **Persisted-assembly path** — new type references must route through `configuration.NeutralizeType`; covered by an M6.5 test, low risk (mechanical).

## Implementation notes (deviations, appended during build)

Only places where the build departed from the design above; the design's constraints (additive API, byte-identical round-trips) held throughout.

- **M6.1 — object model & byte-identical parsing (complete, gated green).** Landed exactly as designed: `Section.Tag = 13` with `IsValid` (now `<= Section.Tag`) and the renumbered `BinaryOrder` (Tag = 6, between Memory 5 and Global 7); `ExternalKind.Tag = 4`; new `Tag`/`CatchClause`/`Throw`/`ThrowRef`/`TryTable` types; `Import.Tag`; `Module.Tags` read/write between Memory and Global; `BlockTypeInstruction.WriteTo` unsealed via an extracted `WriteBlockTypeHeaderTo` (private protected) that `TryTable.WriteTo` reuses before appending its catch vector; parse dispatch for all three opcodes with `try_table` counted on the block-depth counter like `block`/`loop`/`if`. `Throw`/`ThrowRef`/`TryTable` `Compile` are milestone placeholders (`NotSupportedException`), filled in M6.3/M6.4. Deviations: (a) the local `HashCode.Combine` overload set is int-only, so tag/catch/throw hash codes cast their `uint`s to `int` (matching `RefFunc`'s existing style) rather than the `System.HashCode` generic the design implied. (b) `CatchClause.IsRef`/`IsCatchAll` are `internal` convenience members (used by the compiler), not public — the design listed only `Kind`/`Tag`/`Label` as public, so the instruction tests assert against `Kind` instead. Tests: `ExceptionModelTests` round-trips 28 fixtures (all `exceptions/*` well-formed `.wasm` except the rec-group tag.4–6, plus `imports.0`/`imports.5`) byte-identically and a hand-built module carrying a tag section, tag import, tag export, and a `try_table` with all four catch kinds + `throw` + `throw_ref`; plus `ThrowTests`/`ThrowRefTests`/`TryTableTests` (equality/opcode/catch-field coverage, satisfying `Instruction_HasTestClass`). Gate: net8 960 / net9 963 / net10 963 passed, 0 failed, 34 skipped, Debug and Release identical.
  - **`Writer`/`Reader` are not source-linked into the test project** (only `RegeneratingWeakReference.cs` is), so the round-trip tests exercise encodings through `Module.WriteToBinary`/`ReadFromBinary` rather than a raw `Writer` — the design's "byte-identical re-emission" assertion is met at the module level, which is stronger.

- **M6.2 — runtime tags, linking, harness (complete, gated green).** Landed as designed. New runtime types `TagImport` (abstract, `Kind => Tag`, internal `Descriptor`), `ExceptionTag` (sealed; niladic / `params WebAssemblyValueType[]` / `string?` ctors), and `WebAssemblyException : RuntimeException` (public `Tag`, `IReadOnlyList<object?> Arguments`, `GetArgument(int)`, public `(ExceptionTag, object?[])` ctor for emitted IL). `WasmTypeDescriptor.RenderTagSignature(Signature, Signature[]?)` renders `func(<params>)->()` reusing the Phase 5 renderer. `CompilationContext` gained `List<FieldBuilder> Tags`, `List<Signature> TagSignatures`, and `GetTag(uint)`. `Compile.cs` gained `case Section.Tag` (validates typeidx + empty results, defines an `ExceptionTag` field, emits `ldarg.0; ldstr <descriptor>; newobj ExceptionTag(string); stfld` for fresh per-instantiation identity), a `SectionImport` `ExternalKind.Tag` arm (findImport → `EmitTryCast(TagImport)` → `ValidateTag` → `stfld`), a `SectionExport` `ExternalKind.Tag` arm (getter property + `NativeExportAttribute(Tag, name, descriptor)`), and `ExceptionTagConstructor(configuration)` resolving the ctor through `NeutralizeType`. `ImportException.ValidateTag(TagImport, string?, string, string)` returns the `ExceptionTag`, matching invariant (exact descriptor equality, both-null accept), with the `"incompatible import"` substring in its message. `RuntimeImport.FromCompiledExports` gained the `ExternalKind.Tag` case (invoke getter → `ExceptionTag`, copy `native.Descriptor`, yield). Harness: `assert_exception` now really invokes the action and passes iff the unwrapped throwable is a `WebAssemblyException` (a trap/other/normal-return fails); `assert_invalid` maps `"non-empty tag result type"` and the `"unknown tag…"` prefix to `ModuleLoadException`, and generalizes `"type mismatch"` to a `StartsWith` arm (covers the prefixed 3.0 form); `assert_unlinkable` accepts the shorter `"incompatible import"` alongside `"incompatible import type"`.
  - **Deviation — `WebAssemblyException.arguments` renamed to `argumentValues`.** `AnalysisMode=Recommended`'s CA1708 forbids an internal field differing only in case from the public `Arguments` property. The internal field the emitted IL reads is `argumentValues` (M6.3/M6.4 will `ldfld` it by that name).
  - **Empirical confirmation the flip is safe without M6.3/M6.4:** `wasm-tools print` over every `imports/*.wasm` and `exceptions/tag/*.wasm` fixture shows **none** contains a `throw`/`try_table` instruction — tag definition/import/export/linking is all they exercise — so both categories flip on M6.2 alone (as the design predicted). `imports` flips fully green (former 20-line skip list emptied, so the category leaves Inconclusive); `exceptions/tag` flips to a 5-line skip (`30/38/40/49/60` — tag.4–7 define their exception type inside a `(rec …)` group, a Phase 8 rec-group-identity deferral), staying Inconclusive by design. Tests: `ExceptionTagTests` proves fresh-per-instance identity of a defined+exported tag, by-reference preservation of a host-supplied `ExceptionTag` import, and cross-module by-reference linking (export → `AddFromExports` → import). Gate: net8 964 passed / 33 skipped, 0 failed (imports moved Inconclusive→green), Debug and Release identical.
  - **Maintainer naming-veto flags (M6.2 public surface):** `ExceptionTag` (+ its `string?` "infrastructure" ctor), `WebAssemblyException` (`Tag`/`Arguments`/`GetArgument`), `TagImport`, `CatchClause`/`CatchKind`, `Tag`, `Import.Tag`, `Module.Tags`, `ExternalKind.Tag`, `Section.Tag`, `ImportException.ValidateTag`, `WasmTypeDescriptor.RenderTagSignature` (internal). All additive; none alters an existing signature.

- **M6.3 — `throw` / `throw_ref` compile (complete, gated green; spec categories still skipped as designed).** `Throw.Compile` follows §4: validate the tag index via `context.GetTag` ("unknown tag N" when out of range), pop the operands subtype-checked in reverse, and — in reachable code — stash each into a temp (`stloc`), then emit `EmitLoadThis; ldfld <tagField>; ldc.i4 n; newarr object; (dup; ldc.i4 i; ldloc temp_i; box T?/nop; stelem.ref)*; newobj WebAssemblyException(ExceptionTag, object[]); throw`, then `MarkUnreachable()`. In unreachable code it validates only (pops, emits nothing). `ThrowRef.Compile` pops a `(ref null exn)`-compatible operand, then `dup; brtrue OK; newobj NullReferenceException(); throw; OK: castclass WebAssemblyException; throw; MarkUnreachable()` (reusing the Phase-5-style bare `NullReferenceException`, which the runner already maps for the "null reference" trap). All type references route through `context.Configuration.NeutralizeType`. Deviation from §4's letter: the constant/array emission uses `context.Emit(OpCodes.Ldc_I4, …)` directly rather than an `Int32Constant.Emit(context, …)` helper (no such overload — the existing helper takes an `ILGenerator`). Tests: `ThrowTests` proves a thrown tag surfaces to a .NET caller as a `WebAssemblyException` with the exported tag identity and boxed operands (single/no/multi-operand, incl. i32/i64/f64 ordering) and that an out-of-range tag index is rejected with `ModuleLoadException`; `ThrowRefTests` proves `throw_ref` of a null exnref traps with `NullReferenceException`. The exceptions/throw · throw_ref · try_table spec categories stay `RunAllSkipped` (they need `try_table` to observe a throw), flipping in M6.5. Gate: net8 969 passed / 33 skipped, 0 failed, Debug and Release identical.

- **M6.4 — `try_table` core (complete, gated green Debug+Release, net8/9/10).** Landed close to §4 with a few resolved subtleties.
  New internal `ProtectedRegion` (`Runtime/Compilation/ProtectedRegion.cs`): `SpillStart`, `SpillLocals` (the spilled `[SpillStart, entryHeight)` segment, bottom-to-top), `Floor`, `DepthKey`, `EhEndLabel`, `Clauses`, and `ExitStubs` (keyed by absolute target depth key); public readonly fields per the maintainer preference.
  `CompilationContext` gained `Regions` (a list, outermost-first), `InProtectedRegion`, `PhysicalFloor`, `PendingFunctionStubs`, `CrossingRegion`, `GetOrCreateExitStub`, and thin `BeginExceptionBlock`/`BeginExceptFilterBlock`/`BeginCatchBlock`/`EndExceptionBlock` wrappers; `Reset` clears `Regions` and `PendingFunctionStubs`.
  - **Crossing rule is `>=`, not `>` (deviation from §4's "greater than").** Empirically (try_table.1 func 6's `br 1` targets the *outer* try_table's own end from inside it), a branch to a region's own end escapes the region and must `leave`, so `CrossingRegion(targetDepthKey)` returns the outermost region with `DepthKey >= targetDepthKey`. When the returned region's `DepthKey == targetDepthKey` (the region's own end) the branch `leave`s straight to the region's `EhEndLabel` — its resume point, where the spill + results reload — with no exit stub; otherwise it `leave`s to a per-target stub that reloads `[SpillStart, targetBaseline)` from the spill (plus, for a loop target, the carried params) and `br`s the real label.
  - **`EmitBranchTransfer`/`EmitDeadBranchTransfer` choke points in `BranchHelper`.** The zero-region fast path is the literal pre-existing emission (`EmitReachableBranch` + `br`), so non-EH modules compile through byte-identically (RISK-1 gate: the full pre-existing suite stayed green in both configs before the epilogue landed). `Branch`, `BranchOnNull`, `BranchOnNonNull` route their reachable transfer through it; `BranchIf` gained an `EmitCrossing` long-shape (save condition, ferry, `brfalse skip`, `leave`, `skip:` reload) taken only when crossing; `BranchTable` gained a crossing trampoline form (per-target `leave` vs discard-and-`br`, no shared pre-discard). Dead-code branches inside a region emit `leave` too (a `br` across a region boundary is invalid IL even when unreachable).
  - **`Return` inside a region** stashes into the function block's result local(s) and `leave`s `Labels[0]` (which `CompileFunctionEnd` reloads and `ret`s), since a `ret` inside a protected region is illegal.
  - **`TryTable.Compile` entry**: validates the block type and every catch clause against the *enclosing* context (tag bounds → "unknown tag N", label bounds → "unknown label", per-element subtype of sent types — tag params, plus a non-null `(ref exn)` for `_ref` forms — against the target label's branch types → "type mismatch", making try_table.14/15 invalid and 13 valid); catchless or unreachable ⇒ plain-block mode (no region); otherwise spill `[PhysicalFloor, height)` top-down, `BeginExceptionBlock`, reload the params, push the region. The try block's WASM end label *is* the exception block's `EhEndLabel` (branches use `leave`, never `br`).
  - **`TryTable.CompileRegionEnd`** (dispatched from `End.Compile` when `Regions[^1].DepthKey == Depth.Count`): stash fall-through results; one `BeginExceptFilterBlock`/filter + `BeginCatchBlock`/handler per clause; `EndExceptionBlock`; reload the spilled below-params segment then the results at the resume; then emit the exit stubs (skipped over on fall-through via a `br`). The filter is `isinst WebAssemblyException` (the entire traps-escape guarantee — every trap and host exception fails it and unwinds through every filter, `catch_all` included) plus, for a specific tag, a reference-identity `ceq` against the tag field; `BeginCatchBlock(null)` auto-emits the `endfilter`, so the design's explicit `endfilter` is *not* emitted.
  - **Deviation — the handler reads operands via the public `GetArgument(int)`, not `ldfld argumentValues`.** The emitted IL lives in a separate dynamic assembly with no access to the `internal` field; `GetArgument` is the public accessor. Each operand is `unbox.any T` (numerics/v128) or `castclass T` (references, `object` skipped); a `_ref` clause appends the caught exception object.
  - **Tail-call trampolines** (`ReturnCall`/`ReturnCallIndirect`/`ReturnCallRef`): when `InProtectedRegion`, spill operands with `TailCallHelper.SpillOperands(…, discardBeneath: false)` (the `leave` clears the beneath-values), register a per-callsite `Action` on `PendingFunctionStubs`, and `leave` to it; `SectionCode` drains the stubs after the body's final `ret`, outside every region, where each emits the **unchanged** `tail.`-prefixed call + `ret`. **RISK-4 discharged**: the post-`ret` trampoline verified and JIT-accepted on all six cells — no fallback needed; the 5,000,000-deep self-recursion with the `return_call` inside a try_table runs in constant stack, and a tail-called callee's `throw` escapes the enclosing try_table.
  - **Latent bug fixed en route**: `BlockType.TryToValueType` only recognized `funcref`/`externref`, so `block (result exnref)` (and the other 3.0 abstract-ref shorthands) was mis-treated as a void block — breaking every exnref-result block in throw_ref. Extended to map all ten 3.0 shorthands (each shares its s7 encoding with the matching value type).
  - Tests: `TryTableRuntimeTests` covers simple throw/catch (both paths), trap-escape through `catch_all` (unreachable + divide-by-zero), the 5M tail-call constant-stack proof, tail-called-callee throw-escape, a value below the try surviving the spill, in-region `return`, a branch crossing two nested regions, and a loop back-edge crossing a region; the spec suite covers each catch kind, per-type operand extraction incl. typed refs, nested catch-to-outer, and rethrow identity.

- **M6.5 — curation & close-out (complete).** Flipped `exceptions/throw`, `throw_ref`, `try_table` to full `Run` (all green, `.wat` malformed lines shape-skipped by the harness). Re-derived the skip set via a fresh `SpecDiscovery` run: `exceptions/tag` narrows from the M6.2 five-line skip to **[30, 40, 49]** — lines 38 (`register`) and 60 (`assert_unlinkable`) now pass; 30/40 are rec-group module loads (Phase 8) and 49 is an `assert_unlinkable` importing a rec-group tag (fails to load with a rec-group error before its import mismatch can be observed), so the category stays Inconclusive by design. Persisted-assembly verification: `PersistedAssemblyTests.PersistedAssembly_TagThrowCatch_RoundTrip` compiles a defined-tag throw/try_table module to a DLL and round-trips it, exercising `ExceptionTag`/`WebAssemblyException` and the filter/handler IL under `NeutralizeType`. (Flagged concern re: `ImportException.ValidateTag` returning a non-neutralized `ExceptionTag` into a `NeutralizeType`'d field is moot in the standard persisted config, where the reference assembly equals the compile assembly so `NeutralizeType` is identity for `WebAssembly.Runtime`; a genuinely cross-assembly persisted tag *import* is untested and would want a follow-up, but the emitted `ValidateTagImport` method reference is likewise not neutralized — a pre-existing pattern shared with the other `Validate*` imports.) Gate: Debug + Release, net8/9/10, 0 failures; exceptions/tag Inconclusive with its three Phase 8 lines.
