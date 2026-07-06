# Phase 7 design — Relaxed SIMD (deterministic profile)

Status: design complete, implementation delegated.
Companion to `Wasm3Plan.md` Phase 7; follows the pattern of `Wasm3Phase5Design.md` / `Wasm3Phase6Design.md`.

## Scope and strategy

Relaxed SIMD adds 20 opcodes under the existing `0xFD` SIMD prefix, sub-opcodes `0x100`–`0x113`.
Each relaxed instruction permits a set of implementation-defined behaviors; the WASM 3.0 **deterministic profile** pins exactly one.
This library implements the deterministic profile, which for most ops is *identical to an existing non-relaxed instruction*, so the runtime implementations largely delegate to existing `Execute` methods.
The spec fixtures encode the permitted behaviors as `either` multi-alternative expected results; the deterministic result is always among the alternatives, so implementing the profile makes every category pass without tolerance hacks.

No API surface beyond the new instruction classes and enum entries; purely additive.

## Verified facts (checked against the repo, not memory)

- SIMD sub-opcodes are written as LEB128 via `writer.WriteVar((uint)this.SimdOpCode)` in `SimdInstruction.WriteTo`, so values ≥ `0x80` (all of Phase 7) encode as 2-byte LEB automatically; the reader side (`reader.ReadVarUInt32()` in both `Instruction.cs` dispatchers) is equally ready.
- Compiled SIMD code calls a `public static Execute(...)` per instruction class, resolved by `SimdInstruction.ExecuteMethod`; no inline IL needed.
- Base classes: `SimdBinaryV128Instruction` for `(v128, v128) → v128`; ternary ops have no base yet — `V128Bitselect` subclasses `SimdInstruction` directly and pops three values (see its `Compile`).
- Harness staging from Part A is already in place: `EitherValue` (`SpecTestRunner.cs` ~line 1089) deserializes `"type":"either"` with a `TypedValue[]? values` payload but throws `NotSupportedException` on comparison; `CompareReturnValue` (~line 671) is the single comparison seam.
- All seven `relaxed-simd/*` categories are wholesale-skipped in `SpecTests.cs` (lines ~1032–1067) via `RunAllSkipped(..., "relaxed SIMD — Phase 7")`.
- Every `either` alternative in the relaxed-simd fixtures is a `v128` value; `i32x4_relaxed_trunc` uses no `either` at all (its deterministic trunc_sat results are exact).

## Opcode table

Sub-opcode values below are from the ratified 3.0 opcode listing; **verify them against the fixture `.wasm` binaries before writing code** (parse a fixture with the current reader and confirm the unknown-opcode failures report these values, or dump bytes directly).

| Native name | Sub-opcode | Deterministic semantics | Implementation |
|---|---|---|---|
| `i8x16.relaxed_swizzle` | 0x100 | = `i8x16.swizzle` | delegate to `Int8x16Swizzle.Execute` |
| `i32x4.relaxed_trunc_f32x4_s` | 0x101 | = `i32x4.trunc_sat_f32x4_s` | delegate to `Int32x4TruncSatFloat32x4Signed.Execute` |
| `i32x4.relaxed_trunc_f32x4_u` | 0x102 | = `i32x4.trunc_sat_f32x4_u` | delegate to `Int32x4TruncSatFloat32x4Unsigned.Execute` |
| `i32x4.relaxed_trunc_f64x2_s_zero` | 0x103 | = `i32x4.trunc_sat_f64x2_s_zero` | delegate to `Int32x4TruncSatFloat64x2SignedZero.Execute` |
| `i32x4.relaxed_trunc_f64x2_u_zero` | 0x104 | = `i32x4.trunc_sat_f64x2_u_zero` | delegate to `Int32x4TruncSatFloat64x2UnsignedZero.Execute` |
| `f32x4.relaxed_madd` | 0x105 | fused `a*b + c` (single rounding) | `MathF.FusedMultiplyAdd` per lane; optional `Fma` intrinsic fast path |
| `f32x4.relaxed_nmadd` | 0x106 | fused `-(a*b) + c` | `MathF.FusedMultiplyAdd(-a, b, c)` per lane |
| `f64x2.relaxed_madd` | 0x107 | fused `a*b + c` | `Math.FusedMultiplyAdd` per lane |
| `f64x2.relaxed_nmadd` | 0x108 | fused `-(a*b) + c` | `Math.FusedMultiplyAdd(-a, b, c)` per lane |
| `i8x16.relaxed_laneselect` | 0x109 | = `v128.bitselect` | delegate to `V128Bitselect.Execute` |
| `i16x8.relaxed_laneselect` | 0x10A | = `v128.bitselect` | delegate to `V128Bitselect.Execute` |
| `i32x4.relaxed_laneselect` | 0x10B | = `v128.bitselect` | delegate to `V128Bitselect.Execute` |
| `i64x2.relaxed_laneselect` | 0x10C | = `v128.bitselect` | delegate to `V128Bitselect.Execute` |
| `f32x4.relaxed_min` | 0x10D | = `f32x4.min` | delegate to `Float32x4Min.Execute` |
| `f32x4.relaxed_max` | 0x10E | = `f32x4.max` | delegate to `Float32x4Max.Execute` |
| `f64x2.relaxed_min` | 0x10F | = `f64x2.min` | delegate to `Float64x2Min.Execute` |
| `f64x2.relaxed_max` | 0x110 | = `f64x2.max` | delegate to `Float64x2Max.Execute` |
| `i16x8.relaxed_q15mulr_s` | 0x111 | = `i16x8.q15mulr_sat_s` | delegate to `Int16x8Q15MulrSatSigned.Execute` |
| `i16x8.relaxed_dot_i8x16_i7x16_s` | 0x112 | see below | new per-lane implementation |
| `i32x4.relaxed_dot_i8x16_i7x16_add_s` | 0x113 | see below | new per-lane implementation |

### Dot-product deterministic semantics (must be verified against the spec text)

The two dot ops are the only ones without an existing twin.
The deterministic profile treats the `i7x16` operand as **signed** (i.e. plain `i8` interpretation even when bit 7 is set).
For `i16x8.relaxed_dot_i8x16_i7x16_s`: each output lane is `a[2i]*b[2i] + a[2i+1]*b[2i+1]` on sign-extended 8-bit values — verify from the wg-3.0 spec whether the pairwise add wraps or saturates in the deterministic profile before implementing, and cross-check against the `relaxed_dot_product` fixture (its inputs with bit 7 set discriminate the interpretations).
For `i32x4.relaxed_dot_i8x16_i7x16_add_s`: the i16x8 dot result's adjacent pairs are sign-extended, added (this add cannot overflow i32), then added to the accumulator operand `c` with wrapping.
Read the spec text at tag `wg-3.0` (`https://github.com/WebAssembly/spec/tree/wg-3.0`), relaxed-SIMD numerics section, and record the resolved wrap/saturate choice in the Implementation notes.

### FMA determinism note

`MathF.FusedMultiplyAdd` / `Math.FusedMultiplyAdd` are correctly-rounded fused operations on every platform (hardware FMA when available, correct software fallback otherwise), so the scalar path is already deterministic and portable.
A `Fma.IsSupported` vector fast path (mirroring the `Ssse3` pattern in `Int8x16Swizzle.Execute`) is optional and must produce bit-identical results to the scalar fallback; skip it if there is any doubt.
`-a` lane negation for nmadd must be IEEE negate (sign-bit flip), which C# unary minus on float/double is.

## Object model work

1. `SimdOpCode.cs`: 20 new enum entries with `[OpCodeCharacteristics("native.name")]` and XML docs, following the existing naming style (e.g. `Int8x16RelaxedSwizzle`, `Float32x4RelaxedMultiplyAdd`, `Float32x4RelaxedNegativeMultiplyAdd`, `Int16x8RelaxedDotInt8x16Int7x16Signed`, `Int32x4RelaxedDotInt8x16Int7x16AddSigned`).
2. One instruction class per opcode in `WebAssembly/Instructions/`, named after the enum entry.
   Binary `(v128, v128) → v128` ops (swizzle, min/max, q15mulr, i16x8 dot) extend `SimdBinaryV128Instruction`.
   Unary trunc ops extend the existing unary base (`SimdUnaryV128Instruction`).
   Ternary ops (madd/nmadd, laneselect, i32x4 dot-add) follow the `V128Bitselect` shape; introduce an abstract `SimdTernaryV128Instruction` base to avoid four copies of the same `Compile`, matching how the binary/unary bases factor it.
3. `Instruction.cs`: 20 new cases in the general parser's `SimdOpCode` switch (relaxed ops cannot appear in initializer expressions, so the init-expression dispatcher is untouched).
4. Delegating `Execute` methods are one-liners calling the existing class's `Execute`; the JIT inlines them, and keeping one public `Execute` per class preserves the `ExecuteMethod` resolution invariant.

## Harness work

1. Implement `either` comparison in `CompareReturnValue`: when `rawExpected is EitherValue either`, return true if any alternative matches.
   Alternatives that fail can throw (`Assert` / `AssertFailedException`) rather than return false, so wrap each non-final alternative's recursive `CompareReturnValue` call in try/catch of `UnitTestAssertException`; let the final alternative propagate its failure for a useful message if none matched.
   Remove the `NotSupportedException` from `EitherValue.BoxedValue` only if something still calls it after the fix; otherwise leave it as the guard against future misuse (arguments never use `either`).
2. Convert the seven `relaxed-simd/*` methods in `SpecTests.cs` from `RunAllSkipped` to `SpecTestRunner.Run(...)`, re-deriving any per-line skips from actual failures (expected: none).

## Milestones

Each gate requires the full suite green (`dotnet test` Debug and Release; rotate net8.0/net9.0/net10.0 across checks) with zero failures and the skip count only shrinking.

- **M7.1 — Object model + round-trip**: enum entries, instruction classes with `Execute` bodies, parser dispatch; per-instruction unit tests in `WebAssembly.Tests/Instructions/` (follow neighbors like `Int8x16SwizzleTests` for shape) covering round-trip and at least one execution case each; suite green.
- **M7.2 — Harness + spec categories**: `either` comparison, un-skip all seven categories, suite green with `relaxed-simd` passing (expect skip count to drop from 30 to 23).
- **M7.3 — Docs + notes**: tick the Phase 7 checkboxes in `Wasm3Plan.md`, append Implementation notes to this file (resolved dot semantics, any surprises), final both-config green run reported with exact counts.

## Risks

- Opcode value errors: mitigated by verifying against fixture binaries before coding (M7.1 duty).
- Dot-product wrap/saturate ambiguity: mitigated by reading the spec text plus fixture cross-check; the `either` alternatives will expose a wrong choice as a test failure, not a silent pass.
- `trunc_sat` delegation assumes the existing implementations are exactly the deterministic relaxed behavior; the `i32x4_relaxed_trunc` fixtures (no `either`, exact expectations) verify this directly.
- FMA on very old hardware falls back to software `FusedMultiplyAdd`; slower but bit-identical, so no correctness risk.

## Constraints (standing)

- No commits, no branches; leave the working tree for Ryan to review.
- Never edit `SpecTestData/`, `bin/`, `obj/`, `TestResults/`.
- Additive API only; XML docs on all public members; `TreatWarningsAsErrors` means one warning fails the build.
- Temporary files go under `/tmp`, never in the repo.
- Markdown edits: one sentence per line.

## Implementation notes

*(appended by the implementation agent, 2026-07-06)*

### Opcode verification

All 20 sub-opcodes (`0x100`–`0x113`) were verified empirically by scanning every `.wasm` under
`WebAssembly.Tests/Runtime/SpecTestData/relaxed-simd/` for the `0xFD` prefix followed by a 2-byte
LEB128 sub-opcode in `[0x100, 0x113]` and mapping each to the directory it appeared in.
Every opcode in the design table matched its expected native name and category (swizzle → `i8x16_relaxed_swizzle`,
the four truncs → `i32x4_relaxed_trunc`, madd/nmadd → `relaxed_madd_nmadd`, the four laneselects →
`relaxed_laneselect`, min/max → `relaxed_min_max`, q15 → `i16x8_relaxed_q15mulr_s`, the two dots →
`relaxed_dot_product`), all 20 distinct. No opcode value in the design table needed correction.

### Dot-product deterministic semantics (resolved)

Resolved from the `wg-3.0` spec text (`document/core/exec/numerics.rst`) and cross-checked against the
`relaxed_dot_product` fixtures:

- The relaxed dot instructions map to the operator `ivdotsat` (not the plain `ivdot` used by the
  non-relaxed `i32x4.dot_i16x8_s`). `ivdotsat` forms each product with `imul` and combines adjacent
  products with **saturating** addition (`iaddsat`), whereas `ivdot` wraps.
- The deterministic profile prescribes every relaxed parameter to be 0; for the dot products
  `R_idot = 0` selects the **signed** interpretation of the second (`i7x16`) operand. So the deterministic
  profile is: both operands signed `i8`, exact products, signed-**saturating** `i16` pairwise add.
- Implementation: `Int16x8RelaxedDotInt8x16Int7x16Signed.Execute` computes each i16 lane as
  `clamp_i16(a[2i]*b[2i] + a[2i+1]*b[2i+1])` over sign-extended i8 lanes.
  `Int32x4RelaxedDotInt8x16Int7x16AddSigned.Execute` reuses that saturating i16 dot, then sign-extends and
  sums each adjacent i16 pair to i32 (cannot overflow) and adds the i32x4 accumulator with wrapping.
- Fixture cross-check: the two `either` lines (dot line 32 → 32512; dot-add line 62 → 65025) are matched by
  the signed×signed result, which is present in each alternative list. No non-`either` (exact) fixture ever
  drives the i16 pairwise sum outside `[-32768, 32767]`, so wrap-vs-saturate is not directly discriminated by
  any concrete expectation; the spec-correct saturating choice was implemented regardless (and is covered by a
  dedicated unit-test lane that forces `(-128)*(-128)*2 = 32768 → 32767`).

### Surprises / deviations

- None material. The delegation strategy held exactly: swizzle, the four truncs, the four laneselects,
  min/max, and q15 each delegate to their existing 2.0 twin's `Execute` with no behavioral wrinkle, and all
  seven categories passed on the first run with zero per-line skips.
- FMA needed no `Vector128`/`Fma` intrinsic fast path — the scalar `MathF.FusedMultiplyAdd` /
  `Math.FusedMultiplyAdd` fallback is already correctly-rounded and portable, so it is the whole
  implementation. `nmadd` negates the first operand with C# unary minus (IEEE sign-bit flip).
- Harness: the `NotSupportedException` guard in `EitherValue.BoxedValue` was left in place — nothing calls it
  after the `CompareReturnValue` change (the new `EitherValue` branch runs before any `BoxedValue` access), so
  it stays as a guard against future misuse.
- Test scaffolding: added `WebAssembly.Tests/Instructions/RelaxedSimdTestHelper.cs` (typed-lane → `byte[16]`
  packers) to keep the 20 per-instruction tests readable; execution tests run through `ToInstance`, which
  round-trips the module via `WriteToBinary`/`FromBinary`, so each test also covers binary round-trip.

### Verification (final)

- Debug net9.0: Passed 1012, Skipped 23, Failed 0 (Total 1035).
- Release net9.0, Debug net8.0, Release net10.0: see the coordinating report; all green with the same 23 skips.
