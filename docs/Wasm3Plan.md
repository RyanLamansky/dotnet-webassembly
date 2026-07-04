# WebAssembly 3.0 Upgrade Plan

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
| Deterministic profile | Partial / stance | Not an opcode set — a semantics commitment. We document which profile we honor (see below). |
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

- [ ] Allow `i32/i64 add/sub/mul` inside initializer expressions, and widen `global.get` to any **previously declared** immutable global — 3.0 (via the GC proposal) lifted the old imports-only restriction.
- [ ] Extend the **initializer-expression parser** in `Instruction.cs` (the second, restricted `switch`) to accept these opcodes.
- [ ] Ensure constant-expression evaluation (globals, element/data offsets) folds the arithmetic at instantiation.
- [ ] Categories: `const`-adjacent init-expr cases; assorted lines currently skipped across `global`, `elem`, `data`.
- Risk: low. No new runtime concepts.

### Phase 2 — Tail calls  *(small)*

- [ ] New instructions: `return_call`, `return_call_indirect` (and later `return_call_ref` once typed refs land in Phase 5).
- [ ] Emit the IL `tail.` prefix before the `call` / `calli`, honoring the stack-shape rules; validate caller/callee signature compatibility.
- [ ] Verify the CLR actually performs the tail call (or accept best-effort) — confirm deep-recursion fixtures don't stack-overflow.
- [ ] Categories: `tail_call` (return_call_ref portion deferred to Phase 4).
- Risk: medium — IL `tail.` has constraints (no byref/stack args left); the verifier may reject some shapes.

### Phase 3 — Multiple memories  *(medium, plumbing)*

- [ ] Remove the single-memory guards in `Compile.cs`: the section-count check (~line 535, "Multiple memory values are not supported"), the import-vs-defined check (~line 538), and the duplicate-import check (~line 900).
- [ ] Plumb a **memory index** through every memory instruction. The memarg encoding now uses a flag bit to signal a non-zero memory index; update the memarg reader/writer and every load/store/`memory.*` instruction.
- [ ] `memory.copy` gains *two* memory indices (dst, src); `memory.init` gains a memory index alongside the data index; `memory.size` / `memory.grow` / `memory.fill` each take a memory index.
- [ ] Compilation context must hold an array of memory fields/instances instead of a single `memory`.
- [ ] Data segments carry a target memory index.
- [ ] Categories: `multi-memory/*`, plus memory-index lines currently skipped in `memory`, `binary`.
- Risk: medium — broad but mechanical. The `UnmanagedMemory` type itself is unchanged; only selection-by-index is new.

### Phase 4 — 64-bit address space (memory64 / table64)  *(medium-large)*

- [ ] `ResizableLimits`: widen `Minimum`/`Maximum` to 64-bit and add an **address-type** flag (the `0x04`/`0x05` limit-flag bits select i64 addressing). Update its reader/writer and the `Memory(Reader)` / `Table` paths. *(Breaking-ish: surface via new members rather than silently changing `uint` semantics — see Open Questions.)*
- [ ] `Memory` / `Table` expose their address type (i32 vs i64).
- [ ] Load/store and `memory.*` / `table.*` instructions: when the memory/table is 64-bit, the dynamic address operand is `i64`; validate the operand type against the memory's address type rather than assuming `i32`.
- [ ] `UnmanagedMemory` / bounds-checking helpers in `Compilation/`: index with `long`, and reconcile with .NET's `int`-bounded arrays/spans (a 64-bit-declared memory is still practically bounded by available managed memory — bounds checks must trap correctly, not overflow).
- [ ] Categories: `memory64/*` and `binary-leb128:881`.
      Note the upstream `memory64/` dir now covers **table64 as well** (`table64`, `call_indirect64`, `table_copy64`, `table_copy_mixed`, …) alongside the memory files (`address64`, `load64`, `memory_grow64`, `memory_trap64`, `endianness64`, …), so this phase touches table instructions too.
- Risk: medium-large. The hard part is honest trapping semantics when a 64-bit address exceeds what a .NET buffer can represent.

### Phase 5 — Typed function references  *(foundational)*

- [ ] New value-type encoding: `(ref null? $t)` — a reference carrying a concrete type index, plus the abstract heap types (`func`, `extern`, and the 3.0 additions `any`, `eq`, `i31`, `struct`, `array`, `none`, `nofunc`, `noextern`). This needs a richer representation than today's flat `WebAssemblyValueType` enum (`WebAsssemblyValueType.cs`) and `ElementType` enum — likely a `ReferenceType` struct (nullable flag + heap type / type index).
- [ ] Non-defaultable locals: locals of non-nullable ref type require definite-assignment validation before use.
- [ ] New instructions: `call_ref`, `return_call_ref`, `ref.as_non_null`, `br_on_null`, `br_on_non_null`, typed `ref.func`, `ref.null ht`.
- [ ] Subtyping in the validator: stack-type matching becomes "is-subtype-of," not equality.
- [ ] .NET mapping: typed funcrefs are still `Delegate`-shaped at runtime; the new machinery is mostly *validation-time*. Decide how `(ref $t)` maps to a concrete delegate type for `call_ref`.
- [ ] Categories: `ref` / `typed-ref` categories; completes the `return_call_ref` slice of `tail_call`.
- Risk: large design surface (the type algebra), modest runtime surface.

### Phase 6 — Exception handling  *(medium)*

- [ ] New **Tag section** + object model `Tag` type (a tag references a function-type as its exception signature). Add `Tags` to `Module`, an `ExternalKind.Tag` for imports/exports, and reader/writer support.
- [ ] Instructions: `throw`, `throw_ref`, and the 3.0 `try_table` block (with `catch` / `catch_ref` / `catch_all` / `catch_all_ref` clauses). Implement the **new** `try_table` model, not the legacy `try/catch/delegate` exception proposal.
- [ ] Map WASM exceptions onto .NET exceptions: a thrown tag becomes a CLR exception carrying the tag identity + operands; `try_table` emits IL `try`/`catch` that filters by tag and re-pushes operands or rethrows.
- [ ] `throw_ref` / `catch_ref` need a first-class `exnref` value type.
- [ ] Harness: implement the `assert_exception` command in `SpecTestRunner` (see A3) — the exceptions fixtures are the only users.
- [ ] Categories: `exceptions/*` (`tag`, `throw`, `throw_ref`, `try_table`).
- Risk: medium — .NET EH is a good fit, but matching trap-vs-exception semantics and stack unwinding precisely takes care.

### Phase 7 — Relaxed SIMD  *(small-medium)*

- [ ] ~12 opcodes under the `0xFD` SIMD prefix (`SimdOpCode.cs`): relaxed `madd`/`nmadd` (FMA), `laneselect`, relaxed `min`/`max`, relaxed `q15mulr`, relaxed `dot` (i16x8, i32x4), relaxed `trunc` families.
- [ ] Implement the **deterministic-profile default** for each (the spec permits implementation-defined results; we pick the deterministic one, consistent with our stance below). Reuse existing `Vector128<T>` lowering patterns from the 2.0 SIMD instructions.
- [ ] Harness: relaxed-simd fixtures use the `either` expected-result form (see A1); assert the deterministic result matches one of the listed alternatives.
- [ ] Categories: `relaxed-simd/*`.
- Risk: small-medium. Watch FMA: .NET `Vector128` FMA availability is hardware-dependent; provide a deterministic fallback.

### Phase 8 — Garbage collection  *(largest; depends on Phase 5)*

- [ ] Type section overhaul: composite types (`func` | `struct` | `array`), **recursion groups**, explicit `sub`/`sub final` subtyping. This restructures `WebAssemblyType` / `FunctionType` reading and writing.
- [ ] Heap types `any`/`eq`/`i31`/`struct`/`array`/`none` and their value-type encodings (built on Phase 5's `ReferenceType`).
- [ ] Instructions: `struct.new`/`struct.new_default`/`struct.get`(`_s`/`_u`)/`struct.set`; `array.new`/`array.new_default`/`array.new_fixed`/`array.new_data`/`array.new_elem`/`array.get`(`_s`/`_u`)/`array.set`/`array.len`/`array.fill`/`array.copy`/`array.init_*`; `ref.test`/`ref.cast` (nullable + non-nullable); `ref.i31`/`i31.get_s`/`i31.get_u`; `any.convert_extern`/`extern.convert_any`.
- [ ] .NET mapping — the central design question: represent WASM structs/arrays as runtime-generated .NET types (one CLR type per WASM struct/array, leaning on the GC we already get for free), with `ref.cast`/`ref.test` lowering to `castclass`/`isinst`. `i31` packs into a tagged reference or boxed int. Field/element access becomes `ldfld`/`stfld`/`ldelem`/`stelem`.
- [ ] Categories: `gc/*` (`struct`, `array`, `i31`, `ref_test`, `ref_cast`, `type-subtyping`, …).
- Risk: high — both the type algebra and the runtime representation are large. Sequence it last and consider a feature branch (per the maintainer's "iterate complex changes in a branch" preference).

### Cross-cutting — Deterministic profile (stance, not a phase)

- [ ] Document that this library targets the **deterministic profile**: float ops already follow IEEE-754 via the CLR, relaxed-SIMD ops take their deterministic default (Phase 7), and NaN-payload edge cases remain the known `float_exprs` exceptions.
- [ ] Note the existing non-determinism caveats already captured in `SpecTests.cs` (JIT folding of `x±0`/`x*1`) so the profile claim is honest.

---

## Suggested milestones

1. **M0 — Harness** (Part A): regenerate fixtures, re-curate, everything new lands as Inconclusive/red. No behavior change yet.
2. **M1 — Quick wins**: Phase 1 (extended const) + Phase 2 (tail calls).
3. **M2 — Memory**: Phase 3 (multi-memory) + Phase 4 (memory64).
4. **M3 — References**: Phase 5 (typed refs) + Phase 6 (exceptions) + Phase 7 (relaxed SIMD).
5. **M4 — GC**: Phase 8, on a branch.

Each milestone ends with both Debug **and** Release green (CI runs both; conditional compilation makes them genuinely different).

## Open questions / risks to resolve early

- **`ResizableLimits` widening** (Phase 4): changing `Minimum`/`Maximum` from `uint` is API-breaking. Decide between widening in place (breaking, logged in `docs/BreakingChanges.md`) vs. adding parallel 64-bit members. The maintainer treats new features as non-breaking where possible — prefer additive.
- **Reference-type representation** (Phases 5/8): the flat `WebAssemblyValueType : sbyte` enum cannot encode `(ref null? $typeidx)`. A richer type is a prerequisite and ripples through validation, signatures, imports/exports, and `ToSystemType`. Design it once, up front.
- **64-bit addressing vs. .NET buffers** (Phase 4): a memory declared `i64` is still bounded by managed memory; ensure out-of-range 64-bit addresses **trap** rather than silently truncating.
- **GC runtime model** (Phase 8): per-WASM-type generated CLR types vs. a generic boxed representation — a fork in the road that determines the whole phase's shape. Prototype both before committing.
- **Tail-call verifiability** (Phase 2): confirm the IL `tail.` prefix passes the verifier for `call_indirect`/`call_ref` shapes on all target frameworks (net8/net9).
- **`ref.eq` vs boxed `i31`** (Phase 8): `i31ref`s with equal values must compare equal under `ref.eq`, but boxed ints compare by reference identity.
  The `ref.eq` lowering must special-case i31 (value comparison), or the i31 representation must intern.
- **Structural type identity across instances** (Phase 8): `type-canon` / `type-equivalence` / the linking tests require structurally-equivalent rec groups to be *the same* type across module boundaries (`call_indirect`, `ref.cast` between linked modules).
  Per-module Reflection.Emit types are nominally distinct, so a canonicalization layer (or structural comparison at cast/call sites) is needed.
- **wasm-tools/spec pin compatibility** (Part A): mostly resolved — wabt is out (cannot parse 3.0; verified 2026-07-04 against `1.0.41`), and `wasm-tools json-from-wast` `v1.252.0` converted everything sampled.
  Keep the `WasmToolsVersion` and `SpecCommit` pins moving together; mismatches still surface as per-file conversion failures in the refresh run.
