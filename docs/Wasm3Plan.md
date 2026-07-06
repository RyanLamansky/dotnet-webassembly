# WebAssembly 3.0 Upgrade Plan

> **Status: complete as of 2026-07.**
> All phases (1–8) plus the cross-cutting deterministic-profile stance landed on `main`; every ratified-3.0 spec-suite category runs green.
> The per-phase specifics live in the `Wasm3Phase*Design.md` records; the checklist below is retained as the historical plan.

Planning notes for moving this library from ratified WASM 2.0 to WASM 3.0 (released 2025-09-17).
This is a rough, living plan: a strategy for the spec-test harness plus a phased implementation checklist.
It is meant to be edited as work proceeds — check items off, split phases, record decisions inline.

## Scope

WASM 3.0 bundles a set of previously-separate proposals into one ratified version.
The official feature list, and how each maps onto this library's "emit .NET IL via Reflection.Emit" execution model:

| 3.0 feature | In scope here? | Notes |
|-------------|----------------|-------|
| Extended constant expressions | Yes | Smallest, fully self-contained. Good warm-up. |
| Tail calls | Yes | Maps to IL `tail.` prefix; mostly contained. |
| Multiple memories | Yes | Plumbing-heavy but conceptually simple. |
| 64-bit address space (memory64 / table64) | Yes | Touches addressing throughout; medium-large. |
| Typed function references | Yes | Foundational; required before GC. |
| Exception handling (`try_table` model) | Yes | Maps onto .NET exceptions; new section + opcodes. |
| Relaxed SIMD | Yes | ~12 opcodes; we implement the deterministic default. |
| Garbage collection (struct/array/i31) | Yes | By far the largest; depends on typed references. |
| Deterministic profile | Yes / stance | Not an opcode set — a semantics commitment. Documented and honored: IEEE-754 floats via the CLR, deterministic relaxed-SIMD defaults, with the `float_exprs` JIT-folding NaN caveats called out (see below). |
| Custom annotation syntax | No | Text-format only; this library is binary-focused and has no `.wat` parser. |
| JS string builtins | No | Host/JS integration with no meaning under .NET. Out of scope (matches the "no accessories" stance). |

Explicitly **out of scope** because they are *not* part of ratified 3.0 (still proposals), even though `RefreshSpecTests` enables their wabt flags only to keep mixed `.wast` files parseable:

- Custom page sizes
- Wide arithmetic

Keep these in `proposalSubdirs` / excluded so they never leak into the regenerated fixtures.

---

## Part A — Upgrade the spec-test harness to 3.0

The spec data is generated, the test list is hand-curated.
The harness flips from "ratified 2.0, everything else excluded" to "ratified 3.0, only true proposals excluded."

### A1. Re-tool the generator (`Tools/RefreshSpecTests/Program.cs`) — wabt cannot convert the 3.0 suite

**Verified 2026-07-04:** wabt's latest release (`1.0.41`, 2026-05-07) fails to parse the 3.0 test suite.
GC syntax (`struct`/`array` composite types, `rec`/`sub` groups, `anyref` and the other new heap types), the `exn` heap type used by `try_table.wast`, and the new `(module definition …)` script syntax used by `instance.wast` and much of `memory64/` all error out.
wabt's README doesn't list GC among its supported proposals at all; it is effectively not tracking 3.0.
`wasm-tools json-from-wast` (Bytecode Alliance, tested at `v1.252.0`) converts every sampled file — including all of the wabt failures — and is what wasmtime uses to run this same suite.

- [x] Replace wabt with **wasm-tools**: swap `EnsureWabt` for a downloader of the `wasm-tools-<ver>-{x86_64,aarch64}-{linux,macos,windows}` release assets and pin a `WasmToolsVersion`.
      Done: `EnsureWasmTools` downloads `v1.252.0` from the Bytecode Alliance repo; arch mapping is now `x86_64`/`aarch64` and the extracted binary is `wasm-tools[.exe]`.
- [x] Drop the `enableFlags` block entirely: `json-from-wast` has no per-feature enable flags and parses the full 3.0 suite by default; the `--enable-compact-imports` hazard disappears with it (wasm-tools emits only standard encodings).
      Done: invocation is now `json-from-wast <wast> -o <json> --wasm-dir <dir>`.
- [x] Bump `SpecCommit` from the current 2.0-clean pin (`debdda8…`, 2024-04-30) to the **`wg-3.0` tag** (`9d36019…`, 2025-09-26) — the Working Group's ratified-3.0 snapshot.
      Done. Verified 2026-07-04: `wasm-tools` `v1.252.0` cleanly converts the suite at `wg-3.0`; wabt `1.0.41` fails.
- [x] Shrink `proposalSubdirs` — all five are now ratified, so remove all five.
      Done: `proposalSubdirs` is now empty; no unratified-proposal subdirs exist at the pin.
- [x] Remove the `proposalFileStems` memory64 exclusions — upstream relocated those into `memory64/`, and they're in scope now anyway.
      Done: `proposalFileStems` is now empty. Regeneration surfaced no file needing a targeted re-exclusion (custom-page-sizes / wide-arithmetic live in their own repos).
- [x] Update the long header comment block: the "Scope: ratified 2.0" paragraphs, the wabt compatibility-window discussion, and the stale `bulk-memory` note.
      Done: header rewritten for the 3.0 baseline (scope rationale, wasm-tools-vs-wabt, pin compatibility, maintenance procedure, bulk-memory-is-now-a-subdir note).

**Output-format deltas from wast2json** (the JSON is intentionally near-compatible; these gaps are verified):

- Integer values are emitted as *signed* decimal strings (`"-2"`) where wabt used unsigned (`"4294967294"`); the runner's `NanCapableUInt32Converter`/`NanCapableUInt64Converter` use `uint.Parse`/`ulong.Parse` and will throw. Accept both forms.
- New command types appear: `module_definition` / `module_instance` (the 3.0 script syntax behind `instance.wast` and parts of `memory64/`), `assert_exception` (exceptions), and `either` for multi-alternative expected results (relaxed-simd).
- New expected-value type strings: `anyref`, `exnref`, `structref`, `nullref`, `nullfuncref`, `nullexternref`, `nullexnref`, `refnull`.
- Module commands carry an extra `module_type` field (`"binary"`/`"text"`); harmless as long as unmapped JSON fields stay ignored.

**Surprises found during A3 (beyond the verified deltas above):**

- The signed-integer issue also hits SIMD *lane* values: `V128Value.ParseLane` used `ulong.Parse` and threw `OverflowException` on `"-1"` lanes, breaking every `simd_*_cmp` category. Fixed the lane parser the same way as the scalar converters.
- Two more value-type strings appear that weren't in the pre-flight list: `eqref`, `arrayref`, `i31ref`. Added alongside the others.
- Text (`.wat`) modules carry a `binary_filename` field pointing to an assembled `.wasm`; the runner now loads that (the library has no WAT parser), which un-cascaded `comments`, `annotations`, and others.

### A2. Regenerate and diff

- [x] `dotnet run --project Tools/RefreshSpecTests` — exits 0, converts all 258 `.wast` files with zero failures.
- [x] Review the `SpecTestData/` diff: new directories (`gc/`, `memory64/`, `multi-memory/`, `relaxed-simd/`, `exceptions/`, plus `bulk-memory/`) all present; existing categories shifted as expected.
- [x] New top-level categories present as predicted: `call_ref`, `return_call{,_indirect,_ref}`, `br_on_null`, `br_on_non_null`, `ref_as_non_null`, `local_init`, `type-rec`, `type-canon`, `type-equivalence`, `instance`, `ref`, `id`, `annotations`.
      258 categories total; at M0, 136 pass (green) and 122 are Inconclusive (deferred to Part B).

### A3. Re-curate `WebAssembly.Tests/Runtime/SpecTests.cs` (hand-maintained)

- [x] Added `[TestMethod]` entries for every newly-generated category, grouped under `// ==== WASM 3.0 — <feature> ====` section headers (Phases 2–8, plus a module-instances/annotations section).
      The file was regenerated wholesale from the `SpecDiscovery` report rather than hand-edited line-by-line, keeping every skip list exact.
- [x] Extended `SpecTestRunner` for the new command shapes.
      Signed integers: scalar converters + the SIMD lane parser.
      `module_definition`/`module_instance`: **implemented for real** — a definition compiles once (kept as an `InstanceCreator`), each instance re-instantiates it; `register` was also fixed to publish the *named* module's own exports (`ObjectMethods.Host`) instead of the last-compiled module, so `instance.wast`-style registration resolves correctly.
      `assert_exception`: deserialized, then throws a clear "Phase 6" deferral so it flows through the normal skip mechanism (all its users are in the wholesale-skipped `exceptions/*`).
      `either`: deserialized only (a `TypedValue` with a `values[]`); comparison support waits for Phase 7 (its `relaxed-simd/*` users are wholesale-skipped).
      Added a `RunAllSkipped(category, reason)` helper so whole-new-feature categories defer with one documented line instead of hundreds of line numbers.
- [x] Re-derived every `skip:` / `unsupported:` line list from a fresh `SpecDiscovery` run (line numbers shifted throughout).
      Classification rule used: categories where ≥80% of commands fail (module can't load, so everything cascades) use `RunAllSkipped`; mixed categories that retain real green coverage (e.g. `global`, `elem`, `data`, `memory`, `table`, `linking`, `imports`, and the partially-working `memory64/*` that accidentally run on i32-compatible paths) keep explicit per-line skip lists.
- [x] Removed now-obsolete skips: `binary-leb128` (old line-881 memory64 skip) is green again, as are the moved `bulk-memory/*` categories.
- [x] `float_exprs` `unsupported` cases preserved as `unsupported` (not skip) at their new line numbers `2403, 2405, 2407, 2409, 2411, 2413, 2415` — same 7 no-fold assertions, category stays green.
      No *new* `unsupported` classifications were added: everything else defaults to `skip`, per the plan.

> Tip: land A1–A3 **first**, before implementation. The freshly-generated-but-failing 3.0 categories become the TDD scaffold that drives Part B — each phase is "make category X go from red/Inconclusive to green."

---

## Part B — Implementation checklist

Ordered by dependency and ascending difficulty.
Each phase lists object-model changes (read/write/modify), execution changes (IL emission), the .NET mapping risk, and the spec categories it should turn green.

For every new instruction, remember the established five-touch pattern (see `CLAUDE.md` → "How instructions work"):
1. instruction class in `Instructions/` (`OpCode`, `WriteTo`, `Compile` + `ValidateStack`, `Equals`, reader ctor);
2. enum value in `OpCode.cs` / `MiscellaneousOpCode.cs` / `SimdOpCode.cs`;
3. parse dispatch in `Instruction.cs` (both the general and initializer-expression parsers);
4. a test file per instruction under `Tests/Instructions/`;
5. flip the relevant spec category in `SpecTests.cs`.

### Phase 1 — Extended constant expressions  *(small, self-contained)*

- [x] Allow `i32/i64 add/sub/mul` inside initializer expressions, and widen `global.get` to any **previously declared** immutable global — 3.0 (via the GC proposal) lifted the old imports-only restriction.
      Done: `GlobalGet.Compile` now bounds the index by `CompilationContext.ConstantExpressionGlobalBound` (imported + already-declared module globals) and still rejects mutable targets.
- [x] Extend the **initializer-expression parser** in `Instruction.cs` (the second, restricted `switch`) to accept these opcodes.
      Done: the six arithmetic opcodes were added to `ParseInitializerExpression`.
- [x] Ensure constant-expression evaluation (globals, element/data offsets) folds the arithmetic at instantiation.
      No folding needed — initializers already compile straight to IL (arithmetic emits unchecked `add`/`sub`/`mul`), so the value is computed in the instance constructor / getter at instantiation.
- [x] Categories: `const`-adjacent init-expr cases; assorted lines currently skipped across `global`, `elem`, `data`.
      Done: `data` and `const` are fully green; `global`, `elem`, `table` lost their Phase-1 skips (their remaining skips are all Phase 5: typed-ref element/global types and the 3.0 table-with-initializer encoding, flag `0x40` / `0x64`). Added a runner case for the `immutable global` assert_invalid text.
- Risk: low. No new runtime concepts.

### Phase 2 — Tail calls  *(small)*

- [x] New instructions: `return_call` (0x12), `return_call_indirect` (0x13). `return_call_ref` stays deferred to Phase 5.
      Done: `Instructions/ReturnCall.cs` and `Instructions/ReturnCallIndirect.cs`, with opcode enum entries and parse dispatch.
- [x] Emit the IL `tail.` prefix before the `call` / `calli`, honoring the stack-shape rules; validate caller/callee signature compatibility.
      Done: `TailCallHelper.SpillOperands` spills the arguments into locals and pops any operand-stack values beneath them so the CLR stack holds exactly the arguments, then `tail.` + `call`/direct-delegate-`call` + `ret` is emitted. `return_call_indirect` tail-calls the resolved delegate's `Invoke` directly (not via the shared remapper) so no intermediate frame survives. Result types must exactly equal the enclosing function's, else a `type mismatch` (ModuleLoadException).
- [x] Verify the CLR actually performs the tail call (or accept best-effort) — confirm deep-recursion fixtures don't stack-overflow.
      Done: `ReturnCallTests` / `ReturnCallIndirectTests` each run 5,000,000-deep mutual recursion; both pass on net8.0, net9.0, and net10.0 in Debug and Release. The CLR honors the explicit `tail.` (helper-based when needed) in every shape here.
- [x] Categories: `return_call` and `return_call_indirect` are fully green (no skips). `return_call_ref` stays Phase 5.
- Risk: medium — IL `tail.` has constraints (no byref/stack args left); the verifier may reject some shapes.
      No shape required deferral: the arguments are value types / delegates / the exports instance (never byrefs), and function bodies have no protected regions, so `tail.` is accepted everywhere here.

### Phase 3 — Multiple memories  *(medium, plumbing)*  — **Done**

- [x] Remove the single-memory guards in `Compile.cs`: the section-count check, the import-vs-defined check, and the duplicate-import check.
      Done: the Memory section now loops over the declared count, and each memory import appends to the memory list, so both accept any number of memories.
- [x] Plumb a **memory index** through every memory instruction. The memarg encoding uses bit 6 (0x40) of the alignment/flags LEB to signal a following memory-index `varuint32`; the reader/writer of both `MemoryImmediateInstruction` (scalar loads/stores) and `SimdMemoryImmediateInstruction` (v128 loads/stores) now handle it.
      Verified empirically with `wasm-tools dump`: memory 0 emits `28 02 08` (no flag bit); memory 1 emits `28 42 01 08` (flags | 0x40, index, offset). When the index is 0 the writer emits the pre-3.0 minimal encoding, so existing modules round-trip byte-identically (asserted in `MultiMemoryTests`).
- [x] `memory.copy` gains *two* memory indices (dst, src); `memory.init` gains a memory index alongside the data index; `memory.size` / `memory.grow` / `memory.fill` each take a memory index.
      All five object-model classes already carried the immediates; the compiler now honors them. Verified order for `memory.copy`: `fc 0a <dst> <src>` — destination first, source second. A cross-memory copy uses a new `UnmanagedMemory.CopyBetween` static helper; a same-index copy keeps the existing instance `Copy` (byte-identical codegen).
- [x] Compilation context holds a list of memory fields instead of a single `memory`.
      Done: `CompilationContext.Memories` (imported first, then defined, in index order), with `ResolveMemory(index)` throwing the spec's `unknown memory N` for out-of-range indices, plus per-(index,size) range-check and per-(index,helper) store-helper caches so each memory's bounds check / store writes through its own field. Disposal was moved out of the Memory section into a single deferred `Dispose` that releases every module-defined memory (imported ones are owned by their provider).
- [x] Data segments carry a target memory index.
      Done: the Data section reads the kind-2 explicit memidx (kind 0 targets memory 0) and initializes against the resolved memory; out-of-range indices trap with `unknown memory N`.
- [x] Categories: `multi-memory/*` (all 41 green, zero skips), `simd/simd_memory-multi` (green), and the `memory` category's `memory size` asserts (green after the runner learned the shorter wasm-tools text).
      Surprise: the `memory` category's line-8 anonymous `module_definition` tripped a harness null-key bug (fixed: anonymous definitions are compiled for validation but not registered). The remaining `memory` skips (78/82/86/91/95/99) declare a 32-bit memory with 2^32 pages, which the `varuint32` reader silently truncates rather than rejecting — detecting that overflow is entangled with reading memory sizes as `varuint64` for memory64, so it is deferred to Phase 4 (an experimental `ReadVarUInt32` overflow guard regressed `align` and `memory64/*`, which legitimately rely on the truncating read, and was reverted). No memory-index skips remained in `binary`.
- Risk: medium — broad but mechanical. The `UnmanagedMemory` type itself is unchanged apart from the additive cross-memory `CopyBetween`; only selection-by-index is new.

### Phase 4 — 64-bit address space (memory64 / table64)  *(medium-large)*  — **Done**

- [x] `ResizableLimits`: added an **address-type** flag (the `0x04` limit-flag bit selects i64 addressing; combined with `0x01` has-max it becomes `0x05` — verified empirically with `wasm-tools dump` on the fixtures) plus a parallel 64-bit surface, keeping the pre-3.0 `uint` semantics intact (see Open Questions for the additive design chosen over in-place widening).
      Done: the class stores `ulong minimum` / `ulong? maximum` internally; `Minimum`/`Maximum` (`uint`) are narrowing views, `Minimum64`/`Maximum64` (`ulong`) the full width, and `Is64Bit` the address type. The reader reads both bounds as `varuint64` (a 32-bit value's LEB128 is a prefix of the 64-bit one, so i32 modules read unchanged); the writer emits `flags | 0x04` when 64-bit and writes the bound as a u64 LEB, which is byte-identical to the u32 encoding for any value ≤ `uint.MaxValue`, so 32-bit modules round-trip byte-for-byte. Reading a `varuint64` also lets a 32-bit memory declaring 2^32 pages be **rejected** rather than silently truncated (the six `memory` lines 78–99).
- [x] `Memory` / `Table` expose their address type (i32 vs i64) via a forwarding `Is64Bit` property over `ResizableLimits.Is64Bit`.
- [x] Load/store and `memory.*` / `table.*` instructions and `call_indirect`: the dynamic address/index operand type is validated against the target's address type (`CompilationContext.MemoryAddressType` / `TableIndexType`, backed by per-index `MemoryIs64` / `TableIs64` lists). The memarg offset immediate is read as `varuint64` (`MemoryImmediateInstruction.Offset64`, `Offset` narrowing view) and a 32-bit access whose offset exceeds `uint.MaxValue` is rejected ("offset out of range", clearing `align:1005`). `memory.grow`/`memory.size` and `table.grow`/`table.size` return i64 for a 64-bit target; `memory.copy`/`table.copy` between a 32-bit and 64-bit target take differently-typed operands per side (i64 dst/src, i32 length = the narrower type) — the mixed cases are exercised by `table_copy_mixed` and covered by a unit test.
- [x] **Honest 64-bit bounds checking**: a memory64 range check (`CreateRangeCheck64`) takes the i64 address, computes `address + accessSize` in overflow-checked u64 (`Add_Ovf_Un`, trapping via `OverflowException`), compares against the memory's byte size widened to u64, and only then narrows the validated (< 4 GiB) address to a native pointer — never wrapping or truncating. `UnmanagedMemory` gained u64 `Copy`/`CopyBetween`/`InitFromSegment`/`Fill` overloads and a `Grow64` that fails cleanly (returns -1) for any unallocatable delta; `FunctionTable`/`ExternRefTable` gained u64 `Get64`/`Set64`/`Grow64`/`Fill`/`Copy`/`InitFromSegment` that trap on an out-of-range i64 index rather than wrapping into a valid slot. Memory64 lines that formerly passed by *accident* (truncating `ReadVarUInt32`) now pass by *construction*, and the overflow/huge-address edge cases they were silently mis-handling are covered.
- [x] Import address-type compatibility: `UnmanagedMemory` and `TableImport` carry an `Is64Bit` flag (set at defined-memory/table creation, emitted only for 64-bit modules so 32-bit IL is unchanged); `ImportException.ValidateMemory`/`ValidateTable` reject an i32-vs-i64 mismatch, clearing the `memory64-imports` "incompatible import type" asserts.
- [x] Categories: **all `memory64/*` are green** (25 categories, including the table64 files `table64`, `call_indirect64`, `table_copy64`, `table_copy_mixed`, `table_get64`/`set64`/`grow64`/`size64`/`fill64`/`init64`), plus the six `memory` size-limit lines and `align` (offset-out-of-range) cleared. No typed-ref (Phase 5) lines turned up in the table64 files, so nothing there needed relabeling. `binary-leb128` was already green from Phase 3.
- Risk: medium-large — resolved. The hard part (honest trapping when a 64-bit address exceeds a .NET buffer) is handled by overflow-checked u64 arithmetic plus the fact that `UnmanagedMemory` is bounded under 4 GiB, so any address ≥ 2^32 is necessarily out of range.

### Phase 5 — Typed function references  *(foundational)*

> Full design (representation decision, subtyping lattice, milestones M5.1–M5.5, ranked risks): [Wasm3Phase5Design.md](Wasm3Phase5Design.md), produced and reviewed 2026-07-05.

**Progress: Phase 5 is complete.** M5.1–M5.5 are all done, including the closed structural import/export descriptors.
All Phase 5 spec categories are green — `call_ref`, `return_call_ref`, `br_on_null`, `br_on_non_null`, `ref_as_non_null`, `local_init`, `br_table`, `ref_is_null`, `ref_null`, `ref`, `unreached-valid`, plus the per-line clears on `select`, `table`, `elem`, `global`, `func`, `call_indirect`, `bulk-memory/table-sub` — and the last twelve typed-reference linking lines (`table:93,112–116` and `linking:112,182,186,190,442,451`) now flip green as well.
Those needed a new `WasmTypeDescriptor` (a closed, canonical string rendering of a reference type, concrete indices expanded structurally): the exported global's/table's descriptor rides an additive `NativeExportAttribute.Descriptor`, `RuntimeImport.FromCompiledExports` copies it onto the `GlobalImport`/`TableImport`, and descriptor-aware `ImportException.ValidateGlobal`/`ValidateTable` overloads compare them — immutable global supplied `<:` expected, mutable global and table element type invariant — falling back to the coarse CLR-type check whenever either side lacks a descriptor (so host-supplied imports are unchanged).
The skip count dropped from 52 → 36 → 34 across the phase on net8/9/10 × Debug/Release (0 failures).
`table.6` (design risk 7) needed no work — it was already green from the Phase 4 varuint/limits work.
See `Wasm3Phase5Design.md`'s Implementation notes (M5.4/M5.5) for the per-milestone detail.

- [x] New value-type encoding: `(ref null? $t)` — a reference carrying a concrete type index, plus the abstract heap types (`func`, `extern`, and the 3.0 additions `any`, `eq`, `i31`, `struct`, `array`, `none`, `nofunc`, `noextern`, plus `exn`/`noexn`). This needs a richer representation than today's flat `WebAssemblyValueType` enum (`WebAsssemblyValueType.cs`) and `ElementType` enum — likely a `ReferenceType` struct (nullable flag + heap type / type index).
      Done (M5.1): new public `WebAssemblyHeapType` enum + `ReferenceType` struct; additive `Reference`/`NullableReference` + ten shorthand members on `WebAssemblyValueType`/`BlockType`/`ElementType`; `Reader.ReadHeapTypeS33`/`Writer.WriteHeapType`; narrow/wide sibling pairs on `Local`, `Global`, `Import.Global`, `Table` (+ `InitializerExpression` for the `0x40 0x00` form), `Element`, `RefNull`, `SelectWithType`, `BlockTypeInstruction`; positional ref sidecar on `WebAssemblyType`; GC composite-form rejection in both parse paths. Every pre-3.0 and new encoding re-emits byte-identically (`ReferenceTypeTests`); full suite green on net8/9/10 × Debug/Release with the baseline 52 skips unchanged.
- [x] Non-defaultable locals: locals of non-nullable ref type require definite-assignment validation before use.
      Done (M5.3): block-scoped init tracking with rollback; "uninitialized local" ModuleLoadException; `LocalInitTests` covers all seven `local_init` shapes.
- [x] New instructions: `call_ref`, `return_call_ref`, `ref.as_non_null`, `br_on_null`, `br_on_non_null`, typed `ref.func`, `ref.null ht`.
      Done (M5.4): all five new instruction classes (opcode + parse dispatch + `Compile` + tests) plus the `ref.func`/`ref.null`/`ref.is_null` upgrades. `return_call_ref` tail-calls the spilled delegate directly; its 5,000,000-deep mutual-recursion proof passes on net8/9/10 × Debug/Release.
- [x] Subtyping in the validator: stack-type matching becomes "is-subtype-of," not equality.
      Done (M5.2): internal `ValType` currency + `TypeAlgebra` lattice; every pop/validate/branch/return equality site relaxed to subtyping (equality for numerics, so zero behavior change on pre-3.0 modules).
- [x] .NET mapping: typed funcrefs are still `Delegate`-shaped at runtime; the new machinery is mostly *validation-time*.
      Done: `ValType.ToSystemType()` maps the func hierarchy (and every concrete type index, all functions in Phase 5) to `Delegate`, everything else to `object`; `call_ref` applies the concrete delegate type by `castclass` at the call site only.
- [x] Remaining M5.4: inline reference block results (`block (result (ref …))`), typed `SelectWithType`, typed tables/globals/elements + the `0x40 0x00` table-with-initializer in `Compile.cs`, and the `table.6` investigation.
      Done: inline reference block results route through a synthetic single-result `Signature` so the existing multi-value block machinery (End/Else/Branch/BranchTable) handles them; `SelectWithType` subtype-matches reference operands; the `Compile.cs` Table/Global/Element sections read full reftypes (`TableElementTypes`/`ElementSegmentTypes` and `GlobalInfo` are now `ValType`), the `0x40 0x00` initializer form fills every slot with the evaluated const expr, a non-nullable element type without an initializer is a "type mismatch", and `table.get/set/grow/fill/copy/init`/`call_indirect` use the table's precise element `ValType`. `table.6` was already green.
- [x] M5.5 (mostly): runner trap texts (`"null function reference"`/`"null reference"` → `NullReferenceException`); flipped every Phase 5 spec category listed above; removed the temporary `ref:47` skip; re-derived the full skip set from a fresh `SpecDiscovery` run (52 → 36).
- [x] M5.5 remainder — **import/export linking descriptors**: `table:93,112–116` and `linking:112,182,186,190,442,451` flipped green via closed structural descriptors (`WasmTypeDescriptor`) on `NativeExportAttribute` + `GlobalImport`/`TableImport`, with descriptor-aware `ImportException.ValidateGlobal`/`ValidateTable` overloads comparing them (immutable global: supplied <: expected; mutable + table elem: invariant), falling back to the CLR-type check when either side lacks a descriptor. See design §3g and its final Implementation note.
- Risk: large design surface (the type algebra), modest runtime surface. **Resolved** — the descriptor grammar the fixtures require turned out closed and small (see §3g note).

### Phase 6 — Exception handling  *(medium)*

> Full design (encodings, trap taxonomy, try_table IL mapping, milestones M6.1–M6.5, ranked risks): [Wasm3Phase6Design.md](Wasm3Phase6Design.md), produced and reviewed 2026-07-05.

**Progress: Phase 6 is complete.** M6.1–M6.5 are all done, gated green (Debug+Release, net8/9/10, 0 failures).
Object model, byte-identical parsing, runtime tags + linking + harness, `throw`/`throw_ref`, and the full `try_table` core (region records, branch rewiring, filter/handler/exit-stub epilogue, tail-call trampolines) are done.
`imports` and `exceptions/{throw,throw_ref,try_table}` are fully green; `exceptions/tag` is Inconclusive with exactly its three Phase 8 lines (30/40/49, rec-group type identity).
See `Wasm3Phase6Design.md`'s Implementation notes (M6.1–M6.5) for detail.

- [x] New **Tag section** + object model `Tag` type (a tag references a function-type as its exception signature). Add `Tags` to `Module`, an `ExternalKind.Tag` for imports/exports, and reader/writer support.
      Done (M6.1): `Section.Tag = 13` (BinaryOrder 6, between Memory and Global), `Tag`, `Import.Tag`, `Module.Tags`, `ExternalKind.Tag`, all byte-identically round-tripped.
- [x] Instructions: `throw`, `throw_ref`, and the 3.0 `try_table` block (with `catch` / `catch_ref` / `catch_all` / `catch_all_ref` clauses). Implement the **new** `try_table` model, not the legacy `try/catch/delegate` exception proposal.
      Done: object model + parse/write + `throw`/`throw_ref` compile (M6.1/M6.3) and the full `TryTable.Compile`/`CompileRegionEnd` (M6.4). `CatchClause`/`CatchKind` model all four clause kinds.
- [x] Map WASM exceptions onto .NET exceptions: a thrown tag becomes a CLR exception carrying the tag identity + operands; `try_table` emits IL `try`/`catch` that filters by tag and re-pushes operands or rethrows.
      Done: `throw` → `WebAssemblyException(ExceptionTag, object[])` (M6.3); `try_table` compiles to `BeginExceptionBlock` + one `BeginExceptFilterBlock`/`BeginCatchBlock` per clause, the filter an `isinst WebAssemblyException` + reference-identity tag `ceq` (traps unwind through every filter, `catch_all` included), the handler unpacking operands into the label's ferry locals and `leave`-ing to it (M6.4).
- [x] `throw_ref` / `catch_ref` need a first-class `exnref` value type.
      Done: exnref is the `WebAssemblyException` object typed `object` on the CLR stack (Phase 5 already parses/defaults exn/noexn); `throw_ref` compiled in M6.3.
- [x] Interaction with Phase 2: ECMA-335 forbids the IL `tail.` prefix inside a protected region, and the fixtures prove the naive fallback (plain call inside the try) is semantically wrong — a tail call replaces the frame, so the callee's throw must NOT be caught by the enclosing `try_table` (`try_table.1` asserts this).
      Done (M6.4): a `return_call`/`return_call_indirect`/`return_call_ref` inside a region spills its operands and `leave`s to a per-callsite trampoline emitted (via `PendingFunctionStubs`) after the body's final `ret`, outside all protected regions, which performs the unchanged `tail.` sequence. The post-`ret` trampoline was JIT-accepted on all six framework×config cells (no fallback needed); the 5,000,000-deep proof runs in constant stack and the tail-called callee's throw escapes — spec semantics and the constant-stack guarantee both kept.
- [x] Harness: implement the `assert_exception` command in `SpecTestRunner` (see A3) — the exceptions fixtures are the only users.
      Done (M6.2): passes iff the action throws a `WebAssemblyException`; plus the new `assert_invalid`/`assert_unlinkable` text mappings.
- [x] Categories: `exceptions/*` (`tag`, `throw`, `throw_ref`, `try_table`).
      Done: `imports`, `exceptions/throw`, `exceptions/throw_ref`, and `exceptions/try_table` are fully green; `exceptions/tag` is Inconclusive with exactly its three Phase 8 rec-group lines (30/40/49).
- Risk: medium — resolved. .NET EH proved a good fit: `isinst WebAssemblyException` in every filter is the whole trap-vs-exception boundary (traps unwind through `catch_all`), and the physical-vs-model stack arithmetic (entry spill, exit-stub reload extents, skipped intermediate-discards on `leave` paths) held across the RISK-2 shapes (value below the try, multi-region crossing, loop back-edge crossing) without restructuring block compilation.

### Phase 7 — Relaxed SIMD  *(small-medium)*

> Full design (opcode table, deterministic-profile semantics, harness changes, milestones M7.1–M7.3): [Wasm3Phase7Design.md](Wasm3Phase7Design.md).

**Progress: Phase 7 is complete.** M7.1–M7.3 are all done, gated green (Debug+Release, net8/9/10, 0 failures).
All 20 relaxed-SIMD opcodes (`0x100`–`0x113`) are implemented under the deterministic profile, and all seven `relaxed-simd/*` categories pass outright (skip count 30 → 23).
See `Wasm3Phase7Design.md`'s Implementation notes for the opcode verification, the resolved dot-product semantics, and surprises.

- [x] 20 opcodes under the `0xFD` SIMD prefix (`SimdOpCode.cs`): relaxed `madd`/`nmadd` (FMA), `laneselect`, relaxed `min`/`max`, relaxed `q15mulr`, relaxed `dot` (i16x8, i32x4), relaxed `trunc` families, and relaxed `swizzle`.
      Done: sub-opcodes `0x100`–`0x113`, one instruction class each, a new `SimdTernaryV128Instruction` base for the four-operand-shape ops (madd/nmadd, laneselect, i32x4 dot-add), plus general-parser dispatch.
- [x] Implement the **deterministic-profile default** for each (the spec permits implementation-defined results; we pick the deterministic one, consistent with our stance below). Reuse existing `Vector128<T>` lowering patterns from the 2.0 SIMD instructions.
      Done: most ops delegate to their existing non-relaxed twin's `Execute`; madd/nmadd use scalar `MathF`/`Math.FusedMultiplyAdd` (correctly-rounded and portable, no hardware-FMA dependence); the two dot products are new per-lane implementations (signed × signed, saturating i16 pairwise add, wrapping i32 accumulate).
- [x] Harness: relaxed-simd fixtures use the `either` expected-result form (see A1); assert the deterministic result matches one of the listed alternatives.
      Done: `CompareReturnValue` handles `EitherValue` by trying each alternative (non-final ones under a `UnitTestAssertException` catch, the final one propagating).
- [x] Categories: `relaxed-simd/*` — all seven green outright, no per-line skips.
- Risk: small-medium — resolved. FMA needed no `Vector128` intrinsic path: the scalar `FusedMultiplyAdd` fallback is already deterministic and portable, so there is nothing hardware-dependent to guard.

### Phase 8 — Garbage collection  *(largest; depends on Phase 5)*

> Full design (verified encodings, canonicalization, CLR representation, milestones M8.1–M8.7): [Wasm3Phase8Design.md](Wasm3Phase8Design.md).

- [x] Type section overhaul: composite types (`func` | `struct` | `array`), **recursion groups**, explicit `sub`/`sub final` subtyping. This restructures `WebAssemblyType` / `FunctionType` reading and writing. (M8.1: byte-identical round-trip, rec-group scoping.)
- [x] Heap types `any`/`eq`/`i31`/`struct`/`array`/`none` and their value-type encodings (built on Phase 5's `ReferenceType`). (M8.1.)
- [x] Instructions: `struct.new`/`struct.new_default`/`struct.get`(`_s`/`_u`)/`struct.set`; `array.new`/`array.new_default`/`array.new_fixed`/`array.new_data`/`array.new_elem`/`array.get`(`_s`/`_u`)/`array.set`/`array.len`/`array.fill`/`array.copy`/`array.init_*`; `ref.test`/`ref.cast` (nullable + non-nullable); `ref.i31`/`i31.get_s`/`i31.get_u`; `any.convert_extern`/`extern.convert_any`. (Structs M8.3, arrays M8.4, i31/`ref.eq` M8.5, casts/`br_on_cast`/conversions M8.6.)
- [x] .NET mapping — the central design question: represent WASM structs/arrays as runtime-generated .NET types (one CLR type per canonical WASM struct/array, leaning on the GC we already get for free), with `ref.cast`/`ref.test` lowering to `castclass`/`isinst`. `i31` is a `WasmI31` object (value-comparing `ref.eq`). Field/element access becomes `ldfld`/`stfld`/`ldelem`/`stelem`. (M8.2 canonicalization, M8.3 emission.)
- [x] Categories: every `gc/*` category — `struct`, `array`, `i31`, `ref_test`, `ref_cast`, `br_on_cast`/`br_on_cast_fail`, `extern`, `ref_eq`, `array_*`, `binary-gc`, and `type-subtyping` — plus `type-canon`, `type-equivalence`, and `type-rec` are all fully green. `gc/array`'s `array.8` closed by routing the general constant-expression compiler into the element-segment item path (M8.7); `type-subtyping` and `type-rec` 149/157/183/192 closed by runtime function-type identity (M8.8, design §10) — a per-delegate canonical-ancestry registry that makes `call_indirect`, concrete-func-index `ref.cast`/`ref.test`, and function-import linking exact rather than arity-shaped.
- Risk: high — both the type algebra and the runtime representation are large. Delivered on `main` across M8.1–M8.8 (net8/9/10 × Debug/Release, zero failures); no GC gaps remain — the only skipped test in the whole suite is the always-ignored `DiscoverAllFailures` diagnostic helper.

### Cross-cutting — Deterministic profile (stance, not a phase)

- [x] This library targets the **deterministic profile**.
  Float operations follow IEEE-754 via the CLR (the same arithmetic the JIT emits for `float`/`double`), so the ratified `f32`/`f64` suites pass under the runner's NaN-canonicalization tolerance.
  Every relaxed-SIMD operation takes its deterministic default lowering (Phase 7): most `relaxed_*` ops delegate to their non-relaxed twin, `relaxed_madd`/`relaxed_nmadd` use the correctly-rounded portable scalar `Math.FusedMultiplyAdd`, and the relaxed dot products have explicit per-lane implementations — so results are reproducible across hosts rather than implementation-defined.
  The one honest exception is NaN-payload bit patterns under the JIT's identity-op folding (below).
- [x] Non-determinism caveats are captured as `unsupported` (intentional avoidance) lines in `SpecTests.cs`, so the profile claim stays honest.
  `float_exprs` lines 2403/2405/2407/2409/2411/2413/2415 are the `no_fold` assertions for `x±0`, `x*1`, `x/1`, and `demote(promote(x))`: the spec requires these identities are *not* folded (so a signaling-NaN operand is quieted by the real operation), but the .NET JIT folds them and leaves the sNaN unquieted.
  These cannot reasonably be fixed under the CLR execution model and do not affect normal execution, so they are marked `unsupported` and leave the category green.

---

## Suggested milestones

1. **M0 — Harness** (Part A): regenerate fixtures, re-curate, everything new lands as Inconclusive/red. No behavior change yet.
2. **M1 — Quick wins**: Phase 1 (extended const) + Phase 2 (tail calls). **Done** — `data`, `const`, `return_call`, `return_call_indirect` are green; net8/9/10 × Debug/Release all pass with zero failures.
3. **M2 — Memory**: Phase 3 (multi-memory) + Phase 4 (memory64). **Done** — every `multi-memory/*` and `memory64/*` category is green (net8/9/10 × Debug/Release, zero failures).
4. **M3 — References**: Phase 5 (typed refs) + Phase 6 (exceptions) + Phase 7 (relaxed SIMD).
5. **M4 — GC**: Phase 8, on a branch.

Each milestone ends with both Debug **and** Release green (CI runs both; conditional compilation makes them genuinely different).

## Open questions / risks to resolve early

- **`ResizableLimits` widening** (Phase 4): **resolved — additive.** `Minimum`/`Maximum` stay `uint` with unchanged semantics; the class now stores the value at full `ulong` width internally, with the `uint` members as narrowing views and new `Minimum64`/`Maximum64` (`ulong`) plus `Is64Bit` as the parallel 64-bit surface. Nothing became API-breaking: existing callers see identical `uint` behavior for 32-bit modules, and 32-bit modules serialize byte-for-byte as before (a value ≤ `uint.MaxValue` encodes identically as a u32 or u64 LEB128). The same pattern was applied to `MemoryImmediateInstruction.Offset`/`Offset64` and to the `MemoryAccessOutOfRangeException`/`TableAccessOutOfRangeException` offset/index/length properties (32-bit `Offset`/`Index`/`Length` views over new `*64` full-width values). A 64-bit-only quirk to note before committing: `.Minimum`/`.Maximum` narrow (truncate) a memory64 bound that exceeds `uint.MaxValue`, which is documented on those members — code handling memory64 must read `Minimum64`/`Maximum64`.
- **Reference-type representation** (Phases 5/8): the flat `WebAssemblyValueType : sbyte` enum cannot encode `(ref null? $typeidx)`. A richer type is a prerequisite and ripples through validation, signatures, imports/exports, and `ToSystemType`. Design it once, up front.
- **64-bit addressing vs. .NET buffers** (Phase 4): **resolved.** `UnmanagedMemory` stays bounded under 4 GiB, so the memory64 range check computes the effective address (dynamic address + static offset) in overflow-checked u64 (`Add_Ovf_Un`), and any result at or above 2^32 necessarily exceeds the memory's byte size and traps — no truncation, no wrap. Declared-but-unallocatable sizes (e.g. an astronomical minimum inside a `module_definition` that is never instantiated) parse and validate fine and only fail at instantiation/grow. One known limitation left for later: **SIMD** (`v128.load`/`store`) memarg handling is still i32-only; no ratified 3.0 fixture combines fixed-width SIMD with a memory64, so this was left out of scope rather than speculatively widened.
- **GC runtime model** (Phase 8): per-WASM-type generated CLR types vs. a generic boxed representation — a fork in the road that determines the whole phase's shape. Prototype both before committing.
- **Tail-call verifiability** (Phase 2): **resolved for `return_call`/`return_call_indirect`.** The IL `tail.` prefix is accepted and honored (no stack growth over 5,000,000-deep mutual recursion) on net8/net9/net10 in Debug and Release. `return_call_indirect` tail-calls the resolved delegate's `Invoke` directly rather than through the shared `call_indirect` remapper, which would otherwise add a surviving frame per hop. The `call_ref` shape remains to be confirmed when Phase 5 lands.
- **`ref.eq` vs boxed `i31`** (Phase 8): `i31ref`s with equal values must compare equal under `ref.eq`, but boxed ints compare by reference identity.
  The `ref.eq` lowering must special-case i31 (value comparison), or the i31 representation must intern.
- **Structural type identity across instances** (Phase 8): `type-canon` / `type-equivalence` / the linking tests require structurally-equivalent rec groups to be *the same* type across module boundaries (`call_indirect`, `ref.cast` between linked modules).
  Per-module Reflection.Emit types are nominally distinct, so a canonicalization layer (or structural comparison at cast/call sites) is needed.
- **wasm-tools/spec pin compatibility** (Part A): mostly resolved — wabt is out (cannot parse 3.0; verified 2026-07-04 against `1.0.41`), and `wasm-tools json-from-wast` `v1.252.0` converted everything sampled.
  Keep the `WasmToolsVersion` and `SpecCommit` pins moving together; mismatches still surface as per-file conversion failures in the refresh run.
