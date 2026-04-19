# PR 8 — SIMD (v128 / 0xFD prefix)

Goal: implement the WebAssembly SIMD proposal — a new `v128` value type and ~200 instructions under the 0xFD prefix.

## Background

The SIMD proposal adds:
- One new value type: `v128` (-0x05 / 0x7B), a 128-bit vector
- All instructions under prefix byte 0xFD (like 0xFC for misc)
- Opcodes 0x00–0xB3 (and a few higher), each a u32 LEB128 after 0xFD

## Value type

`v128` maps to `System.Runtime.Intrinsics.Vector128<byte>` on the CLR (a 128-bit struct available on .NET 5+).

## Instruction categories (~200 total)

| Range | Category | Examples |
|-------|----------|---------|
| 0x00–0x0B | Load/store | `v128.load`, `v128.store`, load splat/extend/lane |
| 0x0C | Const | `v128.const` (16 immediate bytes) |
| 0x0D–0x14 | Shuffle/swizzle | `i8x16.shuffle` (16 lane imm), `i8x16.swizzle` |
| 0x15–0x22 | Lane extract/replace | `i8x16.extract_lane_s/u`, `f64x2.replace_lane` |
| 0x23–0x3B | f32/f64 comparisons | `f32x4.eq`, `f64x2.lt` etc. |
| 0x3C–0x4F | Integer SIMD | `i8x16.add`, `i16x8.mul` etc. |
| 0x50–0x7F | Bitwise + shifts | `v128.not`, `v128.and`, `i32x4.shl` etc. |
| 0x80–0xB3 | Widening/dot/cvt | `i32x4.dot_i16x8_s`, `f32x4.convert_i32x4_s` etc. |

## CLR implementation strategy

- Add `V128 = -0x05` to `WebAssemblyValueType`; map to `Vector128<byte>`.
- Add `SimdOperationPrefix = 0xFD` to `OpCode`.
- Add `SimdOpCode` enum (u32 values 0x00–0xB3).
- Add `SimdInstruction` abstract base class (parallel to `MiscellaneousInstruction`).
- Group instructions into sub-bases by signature shape:
  - `SimdLoadInstruction` — has memory immediate (align + offset)
  - `SimdConstInstruction` — has 16 immediate bytes
  - `SimdShuffleInstruction` — has 16 lane immediates
  - `SimdLaneInstruction` — has 1 lane immediate byte
  - `SimdSimpleInstruction` — no immediates
- Emit calls to `System.Runtime.Intrinsics.X86.*` / `System.Runtime.Intrinsics.Arm.*` / `Vector128` methods as appropriate.
- For portability, use `Vector128` static methods (available cross-platform in .NET 7+) rather than hardware-specific intrinsics where possible.
- `netstandard2.0` target: SIMD requires at minimum .NET 5 for `Vector128<T>`. Add `#if NET5_0_OR_GREATER` guards; throw `PlatformNotSupportedException` on netstandard2.0.

## Phasing (this PR is large — suggest splitting)

### PR 8a — Infrastructure + load/store/const
- Value type, prefix, enum, base class
- `v128.load`, `v128.store`, `v128.const`
- Basic lane load/store ops

### PR 8b — Integer arithmetic
- `i8x16.*`, `i16x8.*`, `i32x4.*`, `i64x2.*` add/sub/mul/neg/abs/min/max

### PR 8c — Float arithmetic
- `f32x4.*`, `f64x2.*` all ops

### PR 8d — Shuffle, swizzle, extract/replace lane

### PR 8e — Widening, dot product, conversions

## Files to change (infrastructure only, for 8a)

| File | Change |
|------|--------|
| `WebAsssemblyValueType.cs` | Add `V128 = -0x05`; map to `Vector128<byte>` |
| `OpCode.cs` | Add `SimdOperationPrefix = 0xFD` |
| `SimdOpCode.cs` | New — u32 enum for all ~200 codes |
| `Instructions/SimdInstruction.cs` | New abstract base |
| `Instructions/V128Load.cs` | New |
| `Instructions/V128Store.cs` | New |
| `Instructions/V128Const.cs` | New — 16-byte immediate |
| `Instruction.cs` | Wire 0xFD prefix |
| `WebAssembly.Tests/Instructions/V128*Tests.cs` | New |

## Status

- [ ] WebAsssemblyValueType.cs — add V128
- [ ] OpCode.cs — add SimdOperationPrefix
- [ ] SimdOpCode.cs
- [ ] SimdInstruction.cs base class
- [ ] V128Load.cs
- [ ] V128Store.cs
- [ ] V128Const.cs
- [ ] Instruction.cs parser wiring
- [ ] Integer arithmetic instructions (8b)
- [ ] Float arithmetic instructions (8c)
- [ ] Shuffle / lane instructions (8d)
- [ ] Widening / conversion instructions (8e)
- [ ] Tests for each group
