# Phase 8 design — Garbage collection (struct / array / i31 and the GC type system)

Status: design complete, implementation delegated (milestone-gated multi-agent relay).
Companion to `Wasm3Plan.md` Phase 8; follows the pattern of `Wasm3Phase5Design.md` / `Wasm3Phase6Design.md` / `Wasm3Phase7Design.md`.
Produced from codebase exploration and empirical inspection of the checked-in fixtures under `WebAssembly.Tests/Runtime/SpecTestData/{gc,type-canon,type-rec,type-equivalence}/` with `wasm-tools 1.252.0` (`dump`/`print`) and a full `SpecDiscovery` failure survey.
It keeps the additive-API philosophy: every existing encoding round-trips byte-identically, every existing public member keeps its exact behavior, and it builds directly on Phase 5's `ReferenceType` / `WebAssemblyHeapType` / `ValType` / `TypeAlgebra` machinery.

Terminology: "verified-against-fixture" means the byte pattern was confirmed with `wasm-tools dump` on a named checked-in `.wasm`; "spec-cited" means it comes from the wg-3.0 specification and was not directly byte-observed.

## 0. Verified ground truth

### 0a. Type-section structural encodings (all verified-against-fixture)

The type section is no longer a flat vector of function signatures; it is a vector of **rec groups**, each expanding to one or more **sub types**, each wrapping a **composite type**.

| Construct | Lead byte | Encoding | Verified in |
|---|---|---|---|
| Explicit rec group | `0x4E` | `4E <count:u32> <subtype>*` | `type-rec.0.wasm` (`4e 02 …`, `4e 01 …`, `4e 03 …`) |
| Implicit rec group | *(none)* | a bare `subtype` at top level is a singleton rec group of one | `type-rec.0.wasm` type 0 (`60 …` with no `4e`); `struct.0.wasm`, `array.0.wasm` |
| `sub` (non-final) | `0x50` | `50 <supertypecount:u32> <typeidx:u32>* <composite>` | `type-subtyping.0.wasm` (`50 00 5e 7f 00` = no supertype; `50 01 00 5e 7f 00` = one supertype, index 0) |
| `sub final` (explicit) | `0x4F` | `4F <supertypecount:u32> <typeidx:u32>* <composite>` | `type-subtyping.58.wasm` (`4f 01 00 60 00 00` = final, supertype 0, func) |
| `sub final` (shorthand) | *(none)* | a bare composite (`60`/`5F`/`5E`) **is** `sub final` with zero supertypes | every function/struct/array fixture (`60 00 00`, `5f 00`, `5e 78 00`) |
| Composite: func | `0x60` | `60 <params:vec(valtype)> <results:vec(valtype)>` | unchanged from pre-3.0 (`FunctionType.Function = -0x20`) |
| Composite: struct | `0x5F` | `5F <fieldcount:u32> <fieldtype>*` | `struct.0.wasm` (`5f 00`, `5f 04 78 00 …`) |
| Composite: array | `0x5E` | `5E <fieldtype>` (exactly one field type = the element) | `array.0.wasm` (`5e 78 00`, `5e 7f 00`) |
| Field type | — | `<storagetype> <mutability:byte>` where mutability `0x00`=immutable, `0x01`=mutable | `array.0.wasm` (`78 00` immutable i8; `7f 01` mutable i32) |
| Packed storage i8 | `0x78` | in a field's storage-type position | `struct.0.wasm`/`array.0.wasm` |
| Packed storage i16 | `0x77` | in a field's storage-type position | `struct.0.wasm` (`77 00`) / `array.0.wasm` |

Notes proven empirically:
The shorthand composite `0x60`/`0x5F`/`0x5E` decodes to exactly `SubType { is_final: true, supertype_idx: None }` (wasm-tools labels them so).
A `sub final` **may still carry a supertype** (`type-subtyping.58.wasm`'s `4F 01 00 …`), so "final" and "has supertype" are independent — final means no further subtypes may be declared, not that it is a root.
Ratified 3.0 permits **at most one supertype** per sub type (the `supertypecount` is `0` or `1` in every fixture), which is what makes the single-inheritance CLR mapping in §3 sound.
`shared`, `descriptor_idx`, `describes_idx` are all `None`/`false` in every ratified fixture — those are post-3.0 proposal fields and must be rejected if seen.
Storage/value types inside fields reuse the Phase 5 reference encodings verbatim (`struct.0.wasm` field 8 = `64 00` = `(ref 0)`, field 9 = `63 01 00` = `(ref null 1)`; `6e 00` = anyref, `70 00` = funcref, `6c 00` = i31ref).

### 0b. The 0xFB GC opcode space (all verified-against-fixture)

`0xFB` is a new prefix, dispatched exactly like `0xFC` (`MiscellaneousOpCode`) and `0xFD` (`SimdOpCode`): the sub-opcode after the prefix is a LEB128 `u32` (`Instruction.cs:341` shows the `0xFC` precedent reading `ReadVarUInt32`).
Extracted by dumping every `gc/*/*.wasm` fixture:

| Sub-op | Name | Immediates | Verified in |
|---|---|---|---|
| `0x00` | `struct.new` | typeidx | `struct.10.wasm` (`fb 00 00`) |
| `0x01` | `struct.new_default` | typeidx | (spec-cited; same shape) |
| `0x02` | `struct.get` | typeidx, fieldidx | `struct.10.wasm` |
| `0x03` | `struct.get_s` | typeidx, fieldidx | `struct.10.wasm` (`fb 03 00 00`) |
| `0x04` | `struct.get_u` | typeidx, fieldidx | `struct.10.wasm` (`fb 04 00 00`) |
| `0x05` | `struct.set` | typeidx, fieldidx | `struct.10.wasm` (`fb 05 00 01`) |
| `0x06` | `array.new` | typeidx | array fixtures |
| `0x07` | `array.new_default` | typeidx | array fixtures |
| `0x08` | `array.new_fixed` | typeidx, size:u32 | `array.*` (`fb 08 00 02`) |
| `0x09` | `array.new_data` | typeidx, dataidx | `array_new_data.0.wasm` (`fb 09 00 00`) |
| `0x0A` | `array.new_elem` | typeidx, elemidx | `array_new_elem.0.wasm` (`fb 0a 00 00`) |
| `0x0B` | `array.get` | typeidx | `array.*` (`fb 0b 00`) |
| `0x0C` | `array.get_s` | typeidx | `array.*` (`fb 0c 00`) |
| `0x0D` | `array.get_u` | typeidx | `array.*` (`fb 0d 00`) |
| `0x0E` | `array.set` | typeidx | `array.*` (`fb 0e 00`) |
| `0x0F` | `array.len` | *(none)* | `array.*` (`fb 0f`) |
| `0x10` | `array.fill` | typeidx | array_fill fixtures |
| `0x11` | `array.copy` | dsttypeidx, srctypeidx | `array_copy.0.wasm` (`fb 11 00 01`) |
| `0x12` | `array.init_data` | typeidx, dataidx | `array_init_data.0.wasm` (`fb 12 00 00`) |
| `0x13` | `array.init_elem` | typeidx, elemidx | `array_init_elem.0.wasm` (`fb 13 00 00`) |
| `0x14` | `ref.test (ref ht)` | heaptype | `ref_test.*` (`fb 14 …`) — the **non-null** variant |
| `0x15` | `ref.test (ref null ht)` | heaptype | `ref_test.1.wasm` (`fb 15 00`) — the **nullable** variant |
| `0x16` | `ref.cast (ref ht)` | heaptype | `ref_cast.*` — **non-null** |
| `0x17` | `ref.cast (ref null ht)` | heaptype | `ref_cast.0.wasm` (`fb 17 6e`) — **nullable** |
| `0x18` | `br_on_cast` | castflags:byte, labelidx, ht_from, ht_to | `br_on_cast.0.wasm` (`fb 18 03 01 6e 6b`) |
| `0x19` | `br_on_cast_fail` | castflags:byte, labelidx, ht_from, ht_to | `br_on_cast_fail.0.wasm` (`fb 19 01 00 …`) |
| `0x1A` | `any.convert_extern` | *(none)* | `extern.0.wasm` (`fb 1a`) |
| `0x1B` | `extern.convert_any` | *(none)* | `extern.0.wasm` (`fb 1b`) |
| `0x1C` | `ref.i31` | *(none)* | `i31.0.wasm` (`fb 1c`) |
| `0x1D` | `i31.get_s` | *(none)* | `i31.*` |
| `0x1E` | `i31.get_u` | *(none)* | `i31.0.wasm` (`fb 1e`) |

`ref.eq` is **not** 0xFB-prefixed: it is the single byte `0xD3` (verified in `ref_eq.0.wasm`: `d3`), slotting into the existing D-range beside `RefNull=0xD0` (`OpCode.cs:1139`), `RefIsNull=0xD1` (`OpCode.cs:1145`), `RefFunc=0xD2` (`OpCode.cs:1151`), `RefAsNonNull=0xD4` (`OpCode.cs:1157`).

**castflags** (verified): bit 0 = source (from) reference is nullable, bit 1 = target (to) reference is nullable.
`fb 18 03 01 6e 6b` = both-nullable, label 1, from `any` (`0x6e`) to `struct` (`0x6b`) = `br_on_cast 1 anyref structref`.
`fb 18 01 00 …` = from-nullable/to-non-null, label 0 (from `anyref` to `(ref i31)`).
Both heap types are full s33 heap types (concrete index or abstract), read exactly like `ReferenceType.Parse`'s heap-type tail.

### 0c. GC values in constant expressions (verified-against-fixture)

The `SpecDiscovery` run shows `i31.0.wasm` line 1 failing with `Opcode "251" is not permitted in intializer expressions` (251 = `0xFB`), and the same at global/element offsets across many fixtures.
So the **initializer-expression parser** (`Instruction.cs:62` `ParseInitializerExpression`) must accept the GC constant instructions: `struct.new`, `struct.new_default`, `array.new`, `array.new_default`, `array.new_fixed`, `ref.i31`, `any.convert_extern`, `extern.convert_any` (plus `ref.null`/`ref.func`/`global.get`, already accepted).
These appear in global initializers and element-segment item expressions.

### 0d. Host boundary and harness payloads (verified-against-fixture)

**No GC fixture imports a struct or array** (grepped every `gc/*/*.wasm` import section — none reference struct/array/`(ref …)` composite types as imports); host-provided GC values do not exist.
The only reference values crossing the host boundary are **externref host values**, which the existing harness represents as opaque string payloads (`SpecTestRunner.cs:1059-1069`: `(ref.extern 1)` → the string `"1"`).
`gc/extern` exercises `any.convert_extern` / `extern.convert_any` round-trips of those host values (`extern.0.wasm` exports `(func (param externref) (result anyref))` etc.).

Expected-value payloads in the JSON (from `gc/*/*.json`):
Almost all expected returns are plain `i32`/`f32` numerics (487 `i32`, 16 `f32`) — the GC fixtures overwhelmingly extract a scalar via `struct.get`/`array.get`/`i31.get_*` and compare that.
Bare `{"type":"structref"}` (2×), `{"type":"arrayref"}` (5×), `{"type":"eqref"}` (4×), `{"type":"i31ref"}` (1×) carry **no value** — they assert "any non-null reference of this kind", exactly like the funcref precedent (`SpecTestRunner.cs:676`).
`{"type":"anyref","value":"N"}` and `{"type":"externref","value":"N"}` carry a host-ref string payload (e.g. an internalized/externalized `(ref.extern N)`); `"value":"null"` means null.
i31 range facts (verified from `i31.json`): `get_u(-1)` → `2147483647`, `get_u(1073741824)` → `1073741824`, `get_u(2147483647)` → `2147483647`, confirming `ref.i31` masks the input to 31 bits (`payload = input & 0x7FFFFFFF`), `get_u` returns the payload, `get_s` sign-extends bit 30.
New invalid texts: `"field is immutable"` (`struct.8.wasm`, struct.set on an immutable field), plus the existing `"unknown type"`, `"type mismatch"`.

### 0e. What the discovery survey proves about scope

A fresh `SpecDiscovery` (258 categories) gives the exact failure driver per category.
Every `gc/*` and `type-*` failure is currently gated behind one of two `ModuleLoadException`s thrown at parse: `GC composite types … are not yet supported` (`WebAssemblyType.cs:124`, `Signature.cs:46`) for struct/array/rec, or `Opcode "251" is not permitted in intializer expressions`.
Once those two gates open, most failures are cascades (`… has no method source`) that clear automatically, leaving the genuine instruction/validation work.
`gc/binary-gc` is **already green** (`SpecTests.cs:1102`, `Run` with no skip) — it is malformed-binary asserts needing no GC semantics.
Critically, `return_call_ref` and `instance` both report **0 failures** in discovery — their skip lists are entirely **stale** (see §6).

## 1. Type-section object model (additive)

`WebAssemblyType` is today function-only: `Form` plus `Parameters`/`Returns` narrow-list storage with the Phase 5 positional ref sidecars (`WebAssemblyType.cs:12-146`), and `Module.Types` is `IList<WebAssemblyType>`.
The design keeps that class as the **func-composite representation** unchanged and adds the struct/array/rec/sub machinery beside it.

### 1a. Composite payload

Introduce a new public shape carried additively on `WebAssemblyType`:

- `public FunctionType Form` gains three legal values by extending `FunctionType` (currently only `Function = -0x20`, `FunctionType.cs`): add `Struct = -0x21` (0x5F) and `Array = -0x22` (0x5E), matching the s7 encoding bytes (the same encode-as-LEB-s7 rule Phase 5 documented for `ReferenceType.WriteTo`).
- New public class `WebAssembly/StructField.cs` — `public sealed class StructField`: `public WebAssemblyValueType StorageType`, an optional `ReferenceType? StorageRefType` sidecar (long-form refs, mirroring the parameter sidecar pattern), `public PackedType Packed` (`enum PackedType : byte { None, I8, I16 }` — since i8/i16 are not `WebAssemblyValueType` members), and `public bool IsMutable`.
- On `WebAssemblyType`: `public IList<StructField>? Fields` (non-null exactly when `Form` is `Struct`; for `Array` it holds a single element field).

An array's element is modeled as a one-element `Fields` list, so `StructField` is the single field-type abstraction the reader/writer share.
Rationale for reusing `WebAssemblyType` rather than a new class hierarchy: `Module.Types` is `IList<WebAssemblyType>` and cannot change element type without breaking every existing consumer; `WebAssemblyType` therefore becomes the union carrier (Form-discriminated), keeping the collection type stable.
A singleton func type with no `sub`/`rec` wrapping encodes **exactly as today** — `WebAssemblyType.WriteTo` (`WebAssemblyType.cs:188`) is untouched on that path, so all pre-3.0 modules round-trip byte-identically.

### 1b. Sub types and rec groups

Two additive public members on `WebAssemblyType`:

- `public bool IsFinal` (default `true`; the shorthand composites are final).
- `public IList<uint>? SuperTypes` (null/empty = no declared supertype; ratified 3.0 permits at most one entry but the list models the vector faithfully).

Rec groups are modeled at the `Module` level, not per-type, because a rec group's boundary is structurally significant (it scopes type indices and defines canonical identity):

- New public class `WebAssembly/RecursionGroup.cs` — `public sealed class RecursionGroup`: `public IList<WebAssemblyType> Types`.
- `Module` gains `public IList<RecursionGroup>? RecursionGroups` as an **optional parallel view**.

The primary storage stays `Module.Types` (the flat, index-addressable list every existing consumer uses); `RecursionGroups`, when present, records the grouping so writes can reproduce `0x4E` boundaries.
The additive-and-lossless reader/writer contract:
When a module has no explicit rec groups and no struct/array/sub types, `RecursionGroups` is null and everything writes through the untouched pre-3.0 path (byte-identical).
When rec groups or GC types are present, `Module.ReadFromBinary` populates both `Types` (flat) and `RecursionGroups` (grouped), and `WriteToBinary` drives emission from `RecursionGroups` when non-null, else synthesizes singleton implicit groups from `Types` (a bare final func = shorthand, exactly today's bytes).
A round-trip test suite over every `gc/*`, `type-rec/*`, `type-canon/*`, `type-subtyping/*` `.wasm` asserts byte-identical re-emission (the Phase 6 model-level precedent).

### 1c. Parse paths — the two-camp reality

Phase 5's implementation notes established that the type section is parsed **twice**: the object model (`WebAssemblyType(Reader)`, `WebAssemblyType.cs:120`) and the compiler (`Signature(Reader,…)`, `Signature.cs:41`), independently.
Both must learn the rec-group/sub/composite grammar.
A shared internal reader helper `WebAssembly/CompositeTypeReader.cs` decodes one sub type (finality, supertypes, composite kind, fields) and is called from both `WebAssemblyType(Reader)` and the compiler's type-section loop, so the grammar lands once.
The compiler side (§2/§3) additionally canonicalizes and emits; the object-model side only records.

## 2. Canonical type identity (iso-recursive canonicalization)

### 2a. Why it is needed and where it lives

`ref.test`/`ref.cast` between two concrete indices, duplicate-type merging (`type-canon`), cross-index equivalence in `call_indirect`/globals (`type-equivalence`), and the rec-group scoping asserts (`type-rec`) all require deciding when two type indices denote the **same** canonical type.
Because **no GC value crosses a module boundary** (§0d), canonicalization is needed only **within a single compiled module** and only at **compile time** — there is no persistent cross-instance type registry, unlike a full host runtime.
This is a decisive simplification over the general GC implementation: canonical identity is an internal compile-time equivalence relation over the module's own type indices, computed once during the type-section pass, consumed by the validator (`TypeAlgebra`) and the CLR emitter (§3), and never serialized.

### 2b. The algorithm

Iso-recursive canonicalization proceeds rec-group by rec-group, in declaration order (a rec group may reference only earlier groups plus its own members — the scoping rule that makes `type-rec.22` / `type-equivalence.77` invalid forward references):

1. **Unroll within a rec group.** Each member's structure is a term over: composite kind (func/struct/array), finality, supertype references, field storage types + mutability, and value/reference types whose concrete indices are either (a) an index into an *earlier, already-canonicalized* group — replaced by that group member's canonical id, or (b) an index into the *current* group — replaced by a **De Bruijn-style local position** `(rec-relative member index)` so the whole group's canonical form is closed and position-independent.
2. **Structural key.** Render each group as a canonical structural string/tree (the Phase 5 `WasmTypeDescriptor` renderer already does exactly this for func types with a depth-based back-reference cycle guard — extend it to struct/array/field/mutability/finality/supertype tokens). Two groups are identical iff their rendered forms are byte-equal.
3. **Merge.** Maintain a `Dictionary<string, int> canonicalByForm` across the module: a group whose form was seen before maps all its members to the earlier group's canonical member ids (this is what `type-canon` asserts — a re-declared identical rec group is the *same* type).
4. **Tie-knotting for self-reference.** Single-type self-referential groups (`type-rec.0` type 0 = `(func (param (ref 0)) (result (ref 0)))`) terminate because the self-reference is a local De Bruijn position, not a recursive descent; `TypeAlgebra.StructurallyEqual` already implements the bisimulation-with-assumption closure (`TypeAlgebra.cs:73-104`) for the func-only case and is generalized here to struct/array fields.

The output is an internal `CanonicalTypeMap`: `int[] canonicalId` indexed by declared type index (equal ids = interchangeable types), plus per-canonical-id the composite descriptor and resolved single supertype (as a canonical id).
This lives on `CompilationContext` beside `Types`.

### 2c. Object model vs. compile time

The object model does **not** canonicalize — it preserves the declared indices and grouping for lossless round-trip.
Canonicalization is purely a compiler concern (`CompilationContext`), consistent with Phase 5 keeping the public model in encoding form and the validator in `ValType` form.

## 3. CLR runtime representation — the central design question

### 3a. Decision: one emitted CLR class per canonical struct/array type, leaning on the .NET GC

For each **canonical** struct type, emit one CLR class into the same `ModuleBuilder` as the exports type; for each canonical array type, emit one CLR class wrapping a backing element array.
Declared subtype chains become CLR base-class chains, so `ref.cast`/`ref.test` to a **concrete** index lower to `castclass`/`isinst` and the .NET GC manages lifetime for free.
Structural duplicates (within or across rec groups) share one CLR class because they share one canonical id (§2) — this is exactly what makes `ref.cast` between two structurally-equal indices succeed with a single `castclass`.

### 3b. The runtime base hierarchy (new public library types)

The emitted assembly may reference only **public** library members, and generated subclasses must inherit from library base types, so the following are additive public types in `WebAssembly.Runtime` (precedent: `ExceptionTag`/`WebAssemblyException` are public infrastructure):

- `public abstract class WasmGCObject` — common root; implements the abstract-lattice markers below.
- `public abstract class WasmStructObject : WasmGCObject` — base for every emitted struct class.
- `public abstract class WasmArrayObject : WasmGCObject` — base for every emitted array class; exposes `public abstract int Length { get; }` for `array.len`.
- `public sealed class WasmI31 : WasmGCObject` — holds `public readonly int Value` (the 31-bit payload); the i31ref representation.
- Marker interface `public interface IWasmEq {}` — implemented by `WasmGCObject` so struct/array/i31 are all `eq` (see 3f for why `any` needs no marker).
- `public static class WasmGC` with `public static bool RefEq(object? a, object? b)` — the `ref.eq` helper (3g).

`ValType.ToSystemType()` (`ValType.cs:106`) is extended: a concrete struct index → its emitted class (or `WasmStructObject` at slot granularity — see 3c); array index → its emitted class (or `WasmArrayObject`); abstract `struct`/`array`/`eq`/`any`/`i31`/`none` → `object` on the stack (uniform), with the concrete CLR class applied only at use sites.

### 3c. Slot uniformity — concrete CLR types at use sites only

Following the Phase 5 discipline (delegate type applied by `castclass` only at call sites, `ValType.cs:112` comment), **stack slots, locals, and fields that hold GC references are typed `object`** (or `Delegate` for the func hierarchy), and the specific emitted class is applied by `castclass` precisely where a member is touched:

- `struct.get`/`struct.set`: `castclass EmittedStruct; ldfld/stfld field_i`.
- `array.get`/`array.set`/`array.len`: `castclass EmittedArray; ldfld items; …`.

This keeps spills, ferry locals, multi-value plumbing, and the try_table spill machinery type-uniform (all `object`), avoids covariance headaches, and means the emitted struct/array field types can themselves be `object` for reference fields.
Emitted struct fields for **scalar** members keep their exact CLR type (`int`/`long`/`float`/`double`/`Vector128<byte>`, and packed `byte` for i8 / `ushort` for i16) so mutation and default values are correct.

### 3d. Packed fields (i8/i16), mutability, defaults

A packed i8 field is a CLR `byte` field; i16 is `ushort`.
`struct.set`/`array.set` on a packed field stores with `conv.u1`/`conv.u2` (truncation = the WASM wrap).
`struct.get_u`/`array.get_u`: `ldfld` a `byte`/`ushort` loads zero-extended to i32 — the unsigned read for free.
`struct.get_s`/`array.get_s`: `ldfld` then `conv.i1`/`conv.i2` to sign-extend bit 7/15.
Non-packed fields use plain `struct.get` (no sign suffix; validation rejects `_s`/`_u` on non-packed and rejects plain `get` on packed).
Immutable fields: `struct.set`/`array.set` on an immutable field is a **validation** error `"field is immutable"` (`struct.8.wasm`) — a `ModuleLoadException` (new runner invalid text).
`struct.new_default`/`array.new_default`: numeric fields default to 0, `Vector128` to zero, **nullable** reference fields to `null`; a field of **non-nullable** reference type is not defaultable, so `*_default` is a validation error when any field is non-nullable-ref (spec-cited; the emitter never has to produce a non-null default).

### 3e. Arrays — backing storage and the passive-segment operations

Each emitted array class holds one field `T[] Items` where `T` is the element storage CLR type (`byte` for i8, `ushort` for i16, `int`/`long`/`float`/`double`/`Vector128<byte>` for scalars, `object`/`Delegate` for references).
Mutability of the element is part of canonical identity (an immutable `(array i32)` and a mutable `(array (mut i32))` are **different** canonical types → different CLR classes → `ref.cast` between them fails), which the raw-`int[]` alternative could not express — this is why arrays get a wrapper class rather than being lowered to bare CLR arrays.

- `array.new` (typeidx): pop count + init value, allocate `new T[count]`, fill with the value, `newobj EmittedArray`.
- `array.new_default`: allocate `new T[count]` (CLR zero-inits; matches WASM defaults for defaultable elements).
- `array.new_fixed` (typeidx, size): pop `size` values, store into a fresh `T[size]`.
- `array.new_data` (typeidx, dataidx) / `array.init_data` (typeidx, dataidx): element must be numeric/packed; reinterpret bytes from the **passive data segment** byte[] field (the bulk-memory passive-data machinery already exists — `Compile.cs:2062` `StorePassiveDataSegment`, referenced by `memory.init`) into the element array with the element's byte width; out-of-range traps.
- `array.new_elem` (typeidx, elemidx) / `array.init_elem` (typeidx, elemidx): element must be a reference; copy from the **passive element segment** array field (`Compile.cs:1942-1944`, `PassiveElementSegments`).
- `array.get(_s/_u)`/`array.set`: `castclass EmittedArray; ldfld Items; ldc index; ldelem/stelem` with bounds-checked index (a CLR `IndexOutOfRangeException` is the WASM trap — the Phase 6 trap taxonomy already routes it as a trap through `catch_all`, `Wasm3Phase6Design.md` §2).
- `array.len`: `castclass WasmArrayObject; callvirt get_Length`.
- `array.fill` (typeidx): loop store.
- `array.copy` (dsttypeidx, srctypeidx): forward/backward-safe element copy between two array instances (overlap handled like `memory.copy`); element assignability checked at validation.

### 3f. Abstract-type `ref.test`/`ref.cast` — what `isinst` targets

The abstract GC hierarchy maps to `isinst`/`castclass` targets or intrinsic checks:

- `ref.test/cast (ref null? struct)` → `isinst/castclass WasmStructObject`.
- `… array` → `WasmArrayObject`.
- `… i31` → `WasmI31`.
- `… eq` → `isinst IWasmEq` (struct/array/i31 all implement it via `WasmGCObject`; an **internalized extern** — an opaque host `object` — correctly fails `eq`).
- `… $concreteStructIndex` / `$concreteArrayIndex` → `castclass` the specific emitted class (canonical id resolved).
- `… any` → **not** an `isinst`: at an `any`-typed operand the validator guarantees the value is already an internal reference or an internalized extern (func/host-extern are in disjoint hierarchies), so `ref.test (ref any)` reduces to a **non-null check** and `ref.test (ref null any)` is a constant `true`; `ref.cast (ref any)` is a null-guard (`ThrowNullReference` on null, reusing the Phase 5 helper), `ref.cast (ref null any)` is a no-op. This is sound precisely because internalized externs (opaque `object`, no marker) must still count as `any` — a marker interface would wrongly exclude them.
- Bottom types `none`/`nofunc`/`noextern`/`noexn`: no non-null value inhabits them, so `ref.test (ref none)` is constant `false`, `ref.test (ref null none)` is a null check, `ref.cast (ref none)` always traps for non-null, `ref.cast (ref null none)` traps unless null.

Nullable-vs-non-null cast variants: the `_nullable` opcodes (0x15/0x17) permit null to pass the test/cast; the non-null opcodes (0x14/0x16) treat null as failure (`ref.test` → 0, `ref.cast` → trap `NullReferenceException`/`InvalidCastException`).
The castflags null bits on `br_on_cast` compose the same way (see §4).
Null-vs-explicit-check tradeoff follows the Phase 5/6 established pattern: where a CLR primitive already traps (a null `castclass` for a non-null cast throws `InvalidCastException`, indexing throws `IndexOutOfRangeException`), lean on it; where the spec wants a check the CLR would not naturally make (non-null cast of a null through a nullable target), emit an explicit `brtrue`/`ThrowNullReference`.

### 3g. i31 and ref.eq

`ref.i31`: pop i32, mask to 31 bits (`payload = input & 0x7FFFFFFF`, verified §0d), `newobj WasmI31(int)`.
`i31.get_u`: `castclass WasmI31; ldfld Value` (payload is already 0..2³¹−1).
`i31.get_s`: load `Value`, sign-extend bit 30 (`shl 1; shr 1` on i32, or the xor/sub idiom).
`ref.eq` cannot be a bare reference-identity `ceq` because two distinct `WasmI31` boxes with equal payload must compare equal (the plan's flagged open question).
Lowering: `call WasmGC.RefEq(object, object)`, defined as `ReferenceEquals(a,b) || (a is WasmI31 x && b is WasmI31 y && x.Value == y.Value)`.
This keeps i31 identity value-based without interning 2³¹ objects, and because `any.convert_extern`/`extern.convert_any` are identity no-ops (3h), an i31 survives any round-trip as the same `WasmI31` object — satisfying "i31 must survive any.convert_extern round-trips."
A small cache for frequently-used small i31 payloads is an optional allocation optimization, not a correctness requirement (RefEq handles equality regardless).

### 3h. extern ↔ any conversions

`any.convert_extern` (`(ref null extern)` → `(ref null any)`) and `extern.convert_any` (`(ref null any)` → `(ref null extern)`) are **IL no-ops** (identity): both externref and anyref are `object` at runtime, and passing the same object through preserves the identity the fixtures require (`extern.convert_any(any.convert_extern(x)) == x`, `ref.eq` on the wrapped value).
Only the static `ValType` changes; no code is emitted (beyond nothing in unreachable code, per the Phase 5 emit-nothing discipline).

### 3i. Host boundary and .NET-caller visibility

Because no fixture imports GC values, there is no host-supplied-struct path to design.
An exported function returning `(ref struct)` etc. surfaces to a .NET caller as the emitted CLR class instance typed `object` (the export's CLR return type from `ToSystemType`), i.e. a `WasmStructObject`/`WasmArrayObject`/`WasmI31` — opaque but type-testable.
The harness (§7) compares such returns structurally (non-null-of-kind) rather than by identity, matching the JSON's bare `structref`/`arrayref`/`eqref`/`i31ref` expectations; internalized externs surface as the original host payload object (the string), matching `anyref`/`externref` value payloads.
This is documented as the expected shape; no additive public export-descriptor work is required beyond what Phase 5's `WasmTypeDescriptor` already emits (extended in §2b's renderer to cover struct/array for completeness, though linking never exercises it for GC).

### 3j. Emission ordering and the persisted path

Emitted struct/array `TypeBuilder`s are defined during the type-section pass (canonical ids known then), base types wired to super classes or the library bases, fields declared; Reflection.Emit permits IL to reference not-yet-`CreateType()`'d builders, so the Code section compiles against them and all GC `TypeBuilder`s are baked before instantiation.
All library base-type references route through `configuration.NeutralizeType` for the `CreatePersistedAssembly` path (the Phase 6 `ExceptionTag` precedent); an M8.7 persisted-assembly test covers a struct/array/i31-using module.

## 4. Instructions & IL mapping (the five-touch pattern)

New enum: `WebAssembly/GCOpCode.cs` (`enum GCOpCode : uint`, values 0x00–0x1E per §0b), dispatched under a new `0xFB` prefix in **both** parsers in `Instruction.cs` (the general parser and — for the constant subset — `ParseInitializerExpression`), mirroring the `0xFC`/`0xFD` handling at `Instruction.cs:339-359`.
`ref.eq` gets `OpCode.RefEq = 0xd3` and a single-byte dispatch beside the other D-range ops.
One instruction class per opcode under `Instructions/` (e.g. `StructNew.cs`, `ArrayNewFixed.cs`, `RefTest.cs`, `RefCast.cs`, `BranchOnCast.cs`, `RefI31.cs`, `I31GetSigned.cs`, `AnyConvertExtern.cs`, `RefEqual.cs`), each with `OpCode`/`GCOpCode`, `WriteTo`, `Compile`+`ValidateStack`, `Equals`, reader ctor, and a per-instruction test file.

`br_on_cast` / `br_on_cast_fail` are the trickiest IL (like Phase 5's `br_on_null`):
Validation: read castflags/labelidx/ht_from/ht_to; the operand must be `<:` `(ref null? ht_from)`; the branch carries `(ref null? ht_to)` (nullability from castflags) prepended by the label's other operands; `br_on_cast` branches when the value **matches** the target and falls through with the *residual* type (`ht_from` minus `ht_to`); `br_on_cast_fail` is the mirror.
IL composes the Phase 6 `BranchHelper.EmitBranchTransfer` choke point (so it works inside `try_table` regions) with a `dup; <test to_type per 3f>; brtrue/brfalse` guard around the transfer, stashing the (re-typed) value into the label's ferry locals exactly as `br_on_null` does (`Wasm3Phase5Design.md` §3d), and emitting nothing in unreachable code.

## 5. Validation

`TypeAlgebra` (`TypeAlgebra.cs`) already models the abstract GC lattice (`any > eq > {i31,struct,array} > none`, plus func/extern/exn hierarchies) and structural equality of concrete func indices via bisimulation.
Phase 8 extends it:

- **Concrete indexed subtyping with declared `sub`.** `HeapSubtype` (`TypeAlgebra.cs:31`) currently treats two concrete indices as subtypes only when structurally equal (`StructurallyEqual`). Add declared subtyping: `$t <: $u` iff `canonicalId($t) == canonicalId($u)` **or** `$u` is reachable by walking `$t`'s canonical supertype chain. Structural equality remains the base case (undeclared equivalence), now generalized over struct/array composites (fields, mutability, element type), not just func.
- **Concrete-to-abstract.** A concrete struct index `<: struct <: eq <: any`; array index `<: array <: eq <: any`; func index `<: func` (already present). Bottom types are subtypes of every matching concrete type.
- **Field/element subtyping for casts and array.copy.** Immutable field/element subtyping is covariant; mutable is invariant — enforced when validating `array.copy` element assignability and struct/array declared subtyping (spec-cited; the `type-subtyping` fixtures exercise both).
- **`br_on_cast`/`br_on_cast_fail` label typing** per §4.
- **Rec-group scoping.** A type reference may target an earlier group or the current group only; a forward reference across singleton groups is `"unknown type"` — this is what turns `type-rec.22` and `type-equivalence.77` from accidental loads into the required `assert_invalid` (discovery shows line 22 currently loads and fails the `Assert.ThrowsExactly`).
- Non-defaultable-local interactions are already handled by Phase 5's definite-assignment pass; GC adds no new local kinds beyond non-nullable refs, which that pass already covers.

`CompilationContext.Types` stays `Signature[]` for func types; a parallel `CanonicalTypeMap` (§2b) holds struct/array descriptors, canonical ids, and resolved supertypes, consulted by `IsSubtype` (`CompilationContext.cs:447`) and the emitters.

## 6. Loose ends from earlier phases

- **The 40 `return_call_ref` skip lines** (`SpecTests.cs:688`): **stale — they already pass.** The fresh `SpecDiscovery` reports `return_call_ref (0 failure(s))`, including the 1,000,000-deep `count` recursion and every `assert_invalid`; these lines were never re-derived after Phase 5 completed. Remove the entire skip list. (`return_call_ref.0.wasm` uses only plain func types and typed-ref globals, all Phase 5 machinery.)
- **`exceptions/tag` [30, 40, 49]** (`SpecTests.cs:1017`): tags whose signature type lives in a `(rec …)` group; discovery confirms 3 failures, all `GC composite types` at parse. They flip the moment M8.1's rec-group parsing lands — fold into M8.1's flip set.
- **`type-equivalence` skips [30,49,77,136,161,233,238,246,257,268,279,290,308]** and **`type-rec` skips [3,…,197,22]**: discovery shows 13 and 18 failures respectively, all either GC-composite parse gates (→ M8.1) or the forward-reference `assert_invalid` (rec-group scoping validation → M8.1/M8.2). `type-canon` (2 failures) is the same. These flip across M8.1–M8.2.
- **The `instance` category** (`SpecTests.cs:1189`, "Part B"): **stale — already passes.** Discovery reports `instance (0 failure(s))`; the `module_definition`/`module_instance`/`register` command shapes were implemented for real in harness A3 (`Wasm3Plan.md` A3), and `instance.*` fixtures use tags/tables/globals/funcref/refnull — no GC. Remove the `RunAllSkipped` during M8.1 curation; it belongs to no phase, it is a stale label.

## 7. Harness (`SpecTestRunner`)

All harness changes are test-only (no public API).
The GC value-type stubs (`SpecTestRunner.cs:1087-1108`) currently throw on any concrete payload (`ReferenceLikeValue.BoxedValue`).
Required changes, driven by the actual JSON payloads (§0d):

- **`AnyRefValue` and `ExternRefValue`**: box a **host string payload** identically (an internalized/externalized `(ref.extern N)` surfaces as the string `"N"`); `"null"`/absent → null. `AnyRefValue` must stop deriving the throwing `ReferenceLikeValue` and instead mirror `ExternRefValue` (`SpecTestRunner.cs:1062`).
- **Bare `structref`/`arrayref`/`eqref`/`i31ref`** (no value): compare like `FuncRefValue` — a new arm in `CompareReturnValue` (`SpecTestRunner.cs:671`) accepting `actual is WasmStructObject` / `WasmArrayObject` / `IWasmEq` / `WasmI31` respectively (non-null-of-kind); `"null"` payload → `actual is null`.
- **Bare `anyref`** (no value) → any non-null reference.
- No new trap texts beyond the existing set; new **invalid** text `"field is immutable"` maps to `ModuleLoadException` (beside the existing `"type mismatch"`/`"unknown type"` arms).

## 8. Milestones (relay pattern — each ends fully green, Debug+Release, net8/9/10; skip counts only shrink)

Baseline: 1012 passed / 23 skipped / 0 failed on net9.0 both configs; the 23 skips are the Phase 8 categories plus the two stale ones.

**M8.1 — Type-section object model, rec/sub/composite parsing, byte-identical round-trip, scoping validation, stale-skip cleanup.**
`FunctionType.Struct/Array`, `StructField`, `PackedType`, `WebAssemblyType.Fields/IsFinal/SuperTypes`, `RecursionGroup`, `Module.RecursionGroups`, shared `CompositeTypeReader` used by both parse paths; write path reproducing rec-group boundaries and shorthand composites byte-identically; rec-group scoping bounds check (forward-ref → "unknown type").
Flip: `exceptions/tag` [30,40,49], the rec-parse-gated lines of `type-rec`/`type-equivalence`/`type-canon`, and **remove the stale `return_call_ref` and `instance` skips** (verify empirically before removing — if either is not green, stop and report).
Gate: round-trip suite over all `gc/*`+`type-*` `.wasm`; GC-instruction categories still skipped.

**M8.2 — Canonicalization + validator lattice for concrete/declared subtyping.**
`CanonicalTypeMap`, the iso-recursive algorithm (§2b) reusing/extending the `WasmTypeDescriptor` renderer, `TypeAlgebra` extension for concrete indexed subtyping + declared `sub` + field/element covariance/invariance + concrete-to-abstract.
Flip: remaining `type-canon`, `type-rec`, `type-equivalence`, and the **validation-only** lines of `type-subtyping`.
Gate: no runtime GC values yet; those categories stay skipped.

**M8.3 — CLR representation core + structs.**
Public base types (`WasmGCObject`/`WasmStructObject`/`WasmArrayObject`/`WasmI31`/`IWasmEq`/`WasmGC.RefEq`), `ValType.ToSystemType` for struct/array indices, per-canonical-type `TypeBuilder` emission + baking, `struct.new`/`new_default`/`get`/`get_s`/`get_u`/`set` with packed/mutability/default handling.
Flip: `gc/struct`.
Gate: full suite green.

**M8.4 — Arrays.**
Emitted array classes + all `array.*` (`new`/`new_default`/`new_fixed`/`new_data`/`new_elem`/`get(_s/_u)`/`set`/`len`/`fill`/`copy`/`init_data`/`init_elem`), reusing passive data/element segment fields.
Flip: `gc/array`, `array_copy`, `array_fill`, `array_new_data`, `array_new_elem`, `array_init_data`, `array_init_elem`.

**M8.5 — i31 and ref.eq.**
`ref.i31`/`i31.get_s`/`i31.get_u`, `WasmI31`, `ref.eq` via `WasmGC.RefEq`, and the GC constant-expression parser extension (needed for i31/struct/array in globals/elem — discovery's "251 in initializer" lines).
Flip: `gc/i31`, `gc/ref_eq`.

**M8.6 — Casts and conversions.**
`ref.test`/`ref.cast` (both null variants, abstract + concrete targets per §3f), `br_on_cast`/`br_on_cast_fail` (castflags, `EmitBranchTransfer` composition), `any.convert_extern`/`extern.convert_any` (identity).
Flip: `gc/ref_test`, `gc/ref_cast`, `gc/br_on_cast`, `gc/br_on_cast_fail`, `gc/extern`.

**M8.7 — type-subtyping runtime, harness, persisted path, close-out.**
Harness value-comparison changes (§7), `gc/type-subtyping` runtime asserts, persisted-assembly test over a struct/array/i31 module (NeutralizeType coverage), full skip-set re-derivation via `SpecDiscovery`, `Wasm3Plan.md` Phase 8 checkboxes, Debug+Release net8/9/10.

## 9. Risks and open questions

1. **Canonicalization correctness under mutual recursion** (`type-rec`/`type-canon`). *Resolution*: reuse the proven `TypeAlgebra` bisimulation-with-assumption closure, generalized to struct/array; render each rec group to a closed De Bruijn form and merge by string equality; the fixtures are the oracle. *Stop-and-report* if a fixture needs cross-module canonical identity (none found — no GC imports).
2. **`any` test/cast for internalized externs** — a marker interface would wrongly exclude opaque host `object`s. *Resolution (committed)*: `ref.test (ref any)` = non-null check, sound because validation guarantees any-typed operands are internal-or-internalized (§3f); this is the one place the CLR's type system cannot express the WASM abstract type, and the reduction to a null check is the conforming workaround, not a degradation.
3. **i31 identity vs. boxed-int reference equality.** *Resolution (committed)*: value-comparing `WasmGC.RefEq` special-case; no interning; survives convert round-trips because conversions are identity no-ops.
4. **Array mutability/immutability as type identity** — raw CLR arrays cannot distinguish `(array i32)` from `(array (mut i32))`. *Resolution (committed)*: wrapper class per canonical array type, mutability folded into canonical id, so `ref.cast` between them fails as required.
5. **Emitted-type baking order and Reflection.Emit self-reference** — a struct field of its own (or a mutually-recursive) type. *Resolution*: define all GC `TypeBuilder`s before compiling any body; IL may reference un-baked builders; bake all before instantiation. *Risk*: low (the exports type already does this).
6. **`br_on_cast` IL inside `try_table` regions** — must route through the Phase 6 `EmitBranchTransfer` choke point. *Mitigation*: compose it exactly as `br_on_null` does; dedicated arity + in-region tests.
7. **Packed-field sign semantics** — off-by-one in `conv.i1`/`conv.u1` extension. *Mitigation*: the `struct`/`array` fixtures pin exact `_s`/`_u` results; unit tests over i8/i16 boundary values.
8. **No candidate `unsupported` lines identified.** Every surveyed behavior has a conforming CLR lowering — struct/array via emitted classes, i31 via value-comparing helper, abstract tests via `isinst`/null-checks, conversions via identity. If implementation surfaces a genuinely unreachable assert, flag it as a precise per-line `unsupported` rather than approximating, per the Phase 5/6 precedent.

## 10. Runtime function-type identity (M8.8 addendum, written after M8.6)

The last semantic blocker, shared by `gc/type-subtyping` (whole category), `type-rec` [183, 192] (`call_indirect` canonical-mismatch traps), and `type-rec` [149, 157] (function-import `assert_unlinkable`).

### The gap

Funcref values are arity-shaped CLR `Delegate`s, and `call_indirect`'s runtime check is `castclass` to the arity delegate type (`CallIndirect.cs:153`) — coarser than WASM's canonical identity.
Two canonically-distinct func types conflate whenever their CLR shapes agree (differently-shaped rec groups, or `(ref $a)` vs `(ref $b)` parameters that both lower to `object`).
`ref.test`/`ref.cast` to a **concrete func index** has the same problem: `isinst Delegate` proves only the hierarchy, not the identity.

### The mechanism: a process-global side table keyed by delegate instance

Rejected alternative: one emitted CLR delegate type per canonical func type would make `castclass` exact, but it changes the representation of every funcref crossing the host boundary (`FunctionImport` wraps standard delegates like `Func<int,int>`) — a breaking change, so it is out.

Committed design: `WasmGC` gains a `ConditionalWeakTable<Delegate, …>`-based registry mapping a funcref delegate instance to its canonical type identity, registered at every site where a WASM-typed delegate is born, and consulted by the canonical checks.

- **Identity token**: the registered value is the type's closed canonical descriptor *ancestry* — its own module-independent descriptor (`CanonicalTypeMap.Descriptor`, §2b) plus each declared-supertype descriptor up the chain, so subtype checks are membership tests.
  Intern each descriptor string to a token object via a static `WasmGC` dictionary so runtime comparisons are reference-equality, not string comparison.
- **Registration sites**: `ref.func` emission, element-segment initialization (active, passive via `array.new_elem`/`table.init`, declarative), funcref global initializers, `FunctionImport` wiring (the import's declared type index supplies the descriptor — this is also exactly what `type-rec` 149/157's `assert_unlinkable` needs at link time), and export-to-import relinking (`FromCompiledExports`).
- **Checks**: `call_indirect` guards its existing `castclass` with a `WasmGC` canonical check ("the callee's registered exact type is, or declares as an ancestor, the expected type"); `ref.test`/`ref.cast`/`br_on_cast*` to a concrete func index route through the same helper instead of `isinst Delegate`.
- **Unregistered delegates** (host-created and never wired through an import declaration): fall back to today's CLR-shape acceptance rather than trapping, preserving existing host-embedding behavior; spec modules never produce unregistered funcrefs.

This makes `call_indirect` *stricter* in exactly the places the spec requires (it currently wrongly succeeds across same-CLR-shape canonical mismatches) and no looser anywhere.
Cross-module identity is free because descriptors are module-independent strings (already proven by tag import matching, §2b/M8.2).

### Scope

`M8.8` covers: the registry + interning in `WasmGC`, registration at the sites above, the `call_indirect` guard, concrete-func-index cast/test routing, function-import canonical matching (`ImportException` on mismatch), and flipping `gc/type-subtyping` plus the four `type-rec` lines.

## Constraints (standing)

- No commits, no branches; leave the working tree for Ryan to review at each milestone.
- Never edit `SpecTestData/`, `bin/`, `obj/`, `TestResults/`.
- Additive API only; XML docs on all public members; `TreatWarningsAsErrors` means one warning fails the build.
- Internal types use public readonly fields rather than trivial properties.
- Temporary files go under `/tmp`, never in the repo.
- Markdown edits: one sentence per line.

## Implementation notes

*(appended by the implementation agents as milestones complete)*

### Implementation notes (M8.1, agent 1)

M8.1 landed complete and fully green; M8.2 was not started (see the budget/handoff note at the end).

**What landed (object model + both parse paths + writer):**

- `FunctionType` gained `Struct = -0x21` / `Array = -0x22` (§1a).
- New public types: `StructField` (`StorageType`, `StorageRefType` sidecar, `Packed`, `IsMutable`; `IEquatable`), `PackedType { None, I8, I16 }`, `RecursionGroup` (`Types` plus `IsExplicit` — see the encoding note below), and `WebAssemblyType.Fields`/`IsFinal`/`SuperTypes`.
- `Module.RecursionGroups` (`IList<RecursionGroup>?`) — optional parallel view; non-null only when the module uses a feature the flat pre-3.0 encoding cannot reproduce byte-for-byte (any explicit `0x4E`, any struct/array, any non-final type, or any declared supertype). Pure function-type modules keep `RecursionGroups == null` and write through the untouched pre-3.0 path, so every pre-3.0 module still round-trips byte-identically.
- Shared internal `CompositeTypeReader` decodes the rec/sub/finality/supertype wrapper (the sub-type layer); the composite payload (func params/returns, struct/array fields, packed/ref field storage) is decoded by a new internal `WebAssemblyType(Reader, sbyte compositeForm, bool isFinal, IList<uint>? superTypes)` ctor. Both the object-model loop (`Module.ReadFromBinary`) and the compiler's type-section loop (`Compile.cs`) call `CompositeTypeReader.ReadSubType`, so the grammar is decoded once. The old `WebAssemblyType(Reader)` ctor and `Signature(Reader,…)` ctor were removed; `Signature` now has `Signature(WebAssemblyType, uint, CompilerConfiguration)`.
- All §0a encodings verified against the fixture binaries before coding (a perl dumper over every `gc/**`+`type-*` type section): the **only** sub-type forms present in the ratified fixtures are shorthand func/struct/array, explicit `0x4E` (including one empty group in `type-rec.0`), `0x50` with 0 or 1 supertypes, and `0x4F` with exactly **1** supertype. There is **no `0x4F 00` (explicit-final, zero-supertype)** form in any fixture.

**Byte-identical writer rule (important for M8.3+):** a sub type is emitted shorthand (bare composite) iff `IsFinal && SuperTypes` empty; else `0x4F`(final)/`0x50`(non-final) + supertypes + composite. Because no fixture has the redundant `0x4F 00` form, this clean rule reproduces every fixture exactly. `RecursionGroup.IsExplicit` was **added beyond the design's listed members** because a singleton rec group is genuinely ambiguous on the wire — `type-rec.0` contains both bare singletons and explicit `0x4E 01` singletons — and byte-identity requires recording which. The writer emits `0x4E` iff `group.IsExplicit || group.Types.Count != 1` (multi/empty groups are always explicit). The reader sets `IsExplicit` from the `0x4E` marker. If a future non-fixture module uses `0x4F 00`, it would re-emit as shorthand (semantically identical, not byte-identical); no ratified fixture exercises this.

**Rec-group reference scoping (validation, §5 last bullet):** implemented in the compiler type-section loop only (assert_invalid uses `Compile.FromBinary`, not `Module.ReadFromBinary`). Each flat type index records its group's exclusive end boundary; a concrete-index reference (in func params/returns or struct/array field storage) must be `< groupEnd` — a forward reference into a later group throws `ModuleLoadException("unknown type …")`. This is what turns `type-rec.22` and `type-equivalence.77` into the required rejections. Supertype-index scoping is **not** validated yet (deferred to M8.2's declared-subtyping work; no M8.1-flippable fixture needs it).

**Compiler tolerance for declared-but-unused GC types:** `context.Types` stays `Signature[]`. A struct/array type gets a placeholder `Signature` with empty params/returns and a new `public readonly bool IsFunction` (false for GC composites, true for func) — set but not yet consumed; M8.3 should read `IsFunction` to route struct/array indices to their emitted CLR classes. A module that only *declares* struct/array types compiles; any instruction that actually touches a GC value still fails with the existing clear `ModuleLoadException` (opcode 251 / ref.eq 211 / "251 in initializer"), keeping those categories skipped.

**Tests:**

- New `WebAssembly.Tests/GCTypeModelTests.cs`: `[DynamicData]` over every `.wasm` under `gc/` (excluding malformed `binary-gc`), `type-rec/`, `type-canon/`, `type-equivalence/` — 210 cases. Parseable fixtures assert byte-identical round-trip; fixtures whose bodies use unimplemented GC instructions are required to fail with exactly the pending signature (opcode 251/211/"251 in initializer"), so the round-trip assertion self-engages once M8.3+ adds the instruction classes. All 210 green (0 skipped).
- `ReferenceTypeTests.ReferenceType_CompositeTypeForm_Rejected` was renamed `…_StructParsed` and inverted: the struct-form module it used to assert *rejected* now parses and round-trips (its old "Phase 8" rejection assertion was the sole non-GC test failure after the change).

**Curation (SpecTests.cs), verified via a fresh `SpecDiscovery` run (all counts are post-M8.1):**

- `return_call_ref` — stale skip list (40 lines) **removed**; discovery reports 0 failures. Verified empirically.
- `instance` — stale `RunAllSkipped` **removed** → `Run`; discovery reports 0 failures. Verified empirically.
- `type-canon` — `RunAllSkipped` → `Run` (no skip); 0 failures (rec parsing + scoping fully close it in M8.1, ahead of the design's M8.2 expectation).
- `type-equivalence` — skip list **removed** → `Run` (no skip); 0 failures (line 77's forward reference now rejected by scoping).
- `exceptions/tag` — skip `[30,40,49]` → `[49,60]`. 30/40 (rec-group tag types) now parse and pass. 49 **and 60** are `assert_unlinkable` importing a rec-group tag; the import mismatch (`ImportException`) needs canonical structural identity to be observed → M8.2. (60 was passing pre-M8.1 only because the imported module failed to *parse*; now it parses but the linking check is not yet canonical-aware. Net skips for this category still dropped 3→2.)
- `type-rec` — skip `[3,39,45,71,78,137,141,143,149,157,167,174,176,183,185,192,197,22]` (18) → `[52,60,94,104,115,125,149,157,183,192,205,217]` (12). Line 22 now rejected by scoping. The remaining 12 are: `52/60/94/104/115/125/205/217` = `assert_invalid "type mismatch"` (need declared/canonical subtyping validation to reject — **M8.2**); `149/157` = `assert_unlinkable "incompatible import type"` (canonical structural import matching — **M8.2**); `183/192` = expect a runtime `InvalidCastException` from a rec-group-typed `call_indirect`/cast (canonical identity + possibly GC runtime — **M8.2/M8.3+**). These were accidentally-green pre-M8.1 (the modules failed to parse, satisfying `assert_invalid` for the wrong reason); they are correctly-parsing-now and pending real validation.

**Gate (0 failed, 19 skipped, down from 23):** net9.0 Debug 1226 passed / net9.0 Release 1226 / net8.0 Debug 1222 / net10.0 Release 1226 — all 19 skipped. (net8 has 4 fewer total: the net9+-only persisted-assembly tests.) The 19 remaining skipped categories are the 16 GC-instruction `gc/*` categories (except the green `gc/binary-gc`) plus `exceptions/tag` and `type-rec`.

**Handoff — where M8.2 should start:** `type-canon` and `type-equivalence` are already fully green, so M8.2's job there is *done by M8.1's scoping* — do **not** expect to flip them (but do guard against regressing them when canonicalization lands). The concrete M8.2 targets are the 12 `type-rec` lines above, the 2 `exceptions/tag` lines, and the validation-only `gc/type-subtyping` lines. Build the internal `CanonicalTypeMap` on `CompilationContext` (beside `Types`), extend the Phase 5 `WasmTypeDescriptor` renderer to struct/array/field/mutability/finality/supertype tokens, and generalize `TypeAlgebra.StructurallyEqual`/`HeapSubtype` to concrete indexed subtyping (canonical-id equality OR declared-supertype chain walk) with immutable-covariant / mutable-invariant field/element rules. Note `183/192` want a *runtime* `InvalidCastException`, which may need M8.3's emitted CLR classes — confirm with discovery whether M8.2 validation alone flips them before committing them to M8.2. `Signature.IsFunction` and the struct/array placeholder signatures are the hooks M8.3 will use to attach emitted types.

### Implementation notes (M8.2, agent 2)

M8.2 landed complete and fully green; M8.3 was not started (a deliberate handoff — see the end of this note).

**What landed (canonicalization + validator lattice + declared-subtype legality + canonical tag matching):**

- New internal `WebAssembly/Runtime/Compilation/CanonicalTypeMap.cs` — the iso-recursive canonicalization.
  It is built once during the type-section pass (`Compile.cs` `Section.Type`) and stored on `CompilationContext.CanonicalTypes` (a new nullable field beside `Types`).
  Shape (all public readonly fields, per the internal-types convention): `int[] CanonicalId` (equal ids = interchangeable types), `FunctionType[] Kind` (func/struct/array per declared index, for concrete-to-abstract routing), `int[] Supertype` (resolved single declared-supertype index or −1), and `string[] Descriptor` (a closed, module-independent structural rendering per declared index).
  Methods: `SameCanonical(a,b)`, `IsDeclaredSubtype(sub,super)` (same canonical id OR `super` on `sub`'s canonical supertype chain).
- The algorithm proceeds rec-group by rec-group in declaration order.
  Each group is rendered to a closed key: intra-group concrete references become De Bruijn local positions (`L{rel}`), and cross-group references expand to the *already-computed* closed descriptor of the target group plus the target's member position (`«…#pos»`), so the whole rendering is module-independent and self-references terminate.
  Groups whose descriptors are byte-equal merge to the same canonical ids; a member at position `m` of a merged group takes `baseId + m`, so two members at different positions of one group stay distinct (this is what makes `type-rec` lines 183/192's premise real and what distinguishes `exceptions/tag` line 49's position-1-vs-0 case).
  Because the merge key *is* the module-independent descriptor, `Descriptor[i]` doubles as the cross-module import-matching string.
- `TypeAlgebra.IsSubtype` gained an optional `CanonicalTypeMap? canonical` parameter (defaulted; `CompilationContext.IsSubtype` now threads `this.CanonicalTypes`).
  When present, two concrete indices compare by `IsDeclaredSubtype` (canonical id equality OR supertype-chain walk) instead of the raw `StructurallyEqual`; concrete-to-abstract now routes by `Kind` (a struct index `<: struct/eq/any`, an array index `<: array/eq/any`, a func index `<: func`), and bottom-to-concrete routes by kind (`none <: struct/array` index, `nofunc <: func` index).
  `StructurallyEqual` is retained only as the null-map fallback (effectively unreachable now, since every module with concrete refs has a type section and hence a map).
- Declared-subtype legality validation (new, in `Compile.cs` `Section.Type`): for every sub type with a declared supertype, the supertype must be in range and *non-final* (else `"sub type … is final"`), and the sub composite must be a valid subtype of the super composite (`IsValidCompositeSubtype`/`IsValidFieldSubtype`): function params contravariant / results covariant with equal arity; struct width+depth (a subtype may only append trailing fields, shared fields refined); array element refined; field/element refinement covariant when immutable, invariant when mutable, with packedness and mutability required to match exactly.
  Messages contain the fixtures' `"sub type"` / `"sub type 1 does not match super type"` wording.
  Verified against `gc/type-subtyping` fixtures directly (throwaway harness, not committed): `type-subtyping.55` → rejected "super type 0 is final", `type-subtyping.67` → rejected "sub type 1 does not match super type 0", and modules 0–9 (which *have* valid declared supertypes) all still load — so no over-rejection.
- Canonical tag-import matching: a new `Compile.RenderTagDescriptor(context, signature, signatures)` returns `CanonicalTypeMap.Descriptor[tagTypeIndex]` (falling back to the old `WasmTypeDescriptor.RenderTagSignature` only when no map exists).
  All three tag-descriptor sites (tag-section definition, tag import, tag re-export) route through it, so exporter and importer compare rec-group-aware descriptors; host tags (null descriptor) are still accepted.
  This is what flips `exceptions/tag` 49/60 (importing a tag whose type is a different rec-group member than the exporter's).

**Curation (SpecTests.cs), all re-derived from actual results:**

- `exceptions/tag` — skip `[49,60]` **removed** → `Run` (no skip); the canonical tag descriptor rejects both. Category green.
- `type-rec` — skip `[52,60,94,104,115,125,149,157,183,192,205,217]` (12) → `[149,157,183,192]` (4).
  The eight `"type mismatch"` `assert_invalid` lines (52/60/94/104/115/125/205/217 — each a `ref.func`/global whose produced `(ref $f)` is a canonically-distinct rec-group member from the declared global type) are now correctly rejected by the canonical-id subtype check on the global initializer.
  The 4 remaining are documented in-place with their precise blockers: **149/157** (`assert_unlinkable`) need a *function* import to carry a closed descriptor and be matched canonically, but function imports today do only an arity-based CLR delegate cast — there is no function-export/import descriptor infrastructure (unlike tags/globals/tables), so adding it is a later-milestone change; **183/192** (`assert_trap`) need `call_indirect` to trap when a table function's canonical type differs, but the runtime table representation is an arity-keyed `Delegate` that cannot distinguish two structurally-identical func types in differently-shaped rec groups — that needs per-canonical-type runtime tags (GC runtime representation, **M8.3+**).
  Because 183/192 keep `type-rec` Inconclusive regardless, flipping 149/157 alone would not reduce the skip count, so the risky function-descriptor work was deliberately deferred.
- `gc/type-subtyping` — left `RunAllSkipped`.
  It is dominated by runtime GC asserts (17 `assert_return`, 29 `invoke`, 12 `assert_trap` that instantiate struct/array modules), which need M8.3+; its validation-only `"sub type"`/`"type mismatch"` lines are now *implemented and verified* (above) but cannot be cleanly isolated into a Run+skip without a large fragile skip list, and the category would stay Inconclusive anyway.
  It flips wholesale when the GC runtime lands (M8.3 for structs, M8.4 for arrays, M8.7 close-out).

**Gate (0 failed, 18 skipped, down from 19):** net9.0 Debug 1227 passed / net9.0 Release 1227 / net8.0 Debug 1223 / net10.0 Release 1227 — all 18 skipped. (net8 has 4 fewer total: the net9+-only persisted-assembly tests.) `GCTypeModelTests` (210 cases) stayed green throughout; `type-canon`/`type-equivalence` did not regress.

**Handoff — where M8.3 should start (agent 3):** the M8.2 hooks are in place — `CanonicalTypeMap` (with `CanonicalId`/`Kind`/`Descriptor`) on `CompilationContext.CanonicalTypes`, and `Signature.IsFunction` + the struct/array placeholder signatures from M8.1.
M8.3 is the CLR representation core + structs (design §3, §4): the public base hierarchy (`WasmGCObject`/`WasmStructObject`/`WasmArrayObject`/`WasmI31`/`IWasmEq`/`WasmGC.RefEq`), per-canonical-type `TypeBuilder` emission wired to those bases, `ValType.ToSystemType` for concrete struct/array indices, and the `struct.*` instruction subset with the `0xFB` prefix dispatch (plus the initializer-expression constant subset — `struct.new` appears in `gc/struct` global initializers).
Emit **one CLR class per canonical id** (use `CanonicalTypeMap.CanonicalId` to share a class across structurally-equal indices; declared supertype chains via `CanonicalTypeMap.Supertype` become CLR base-class chains).
Add the harness `"field is immutable"` invalid-text mapping (§0d/§7) when wiring `gc/struct`.
Note for the array milestone (M8.4): the canonical map already folds array element mutability into identity (an immutable `(array i32)` and mutable `(array (mut i32))` render differently → different canonical ids → different CLR classes), so `ref.cast` between them will fail for free once the classes are emitted.

### Implementation notes (M8.3, agent 3)

M8.3 landed complete and fully green; M8.4 was not started (a deliberate budget-gated handoff — see the end of this note).

**What landed (CLR representation core + all six struct instructions):**

- New public library types in `WebAssembly.Runtime` (additive, XML-documented): `IWasmEq` (marker), `WasmGCObject` (abstract root, implements `IWasmEq`, protected ctor), `WasmStructObject`/`WasmArrayObject` (abstract bases; `WasmArrayObject` exposes `public abstract int Length`), `WasmI31` (sealed, `public int Value`, `ctor(int)`), and `static WasmGC.RefEq(object?, object?)` (`ReferenceEquals || (both WasmI31 with equal Value)`).
  These are in namespace `WebAssembly.Runtime`, so `PersistedCompilerConfiguration.NeutralizeType` already resolves them for the persisted path (the `ExceptionTag` precedent).
- New internal `WebAssembly/Runtime/Compilation/GCTypeEmitter.cs` — emits **one CLR class per canonical struct/array id** into the exports `ModuleBuilder`. Built during the type-section pass (`Compile.cs` `Section.Type`, right after `CanonicalTypeMap.Build`) and stored on the new `CompilationContext.GCTypes`; baked (`CreateAllTypes`, base-classes-first) immediately before `exportsBuilder.CreateTypeInfo()`.
  - Base class = the emitted class of the declared supertype's canonical id when present, else `WasmStructObject`/`WasmArrayObject` (routed through `configuration.NeutralizeType`). Chains are defined base-first (`DefineType` accepts an unbaked `TypeBuilder` parent) and the recursive baker creates a base before its derived class.
  - Each class gets a **manually-emitted parameterless constructor** (`ldarg.0; call baseCtor; ret`) rather than `DefineDefaultConstructor`, because the latter throws when the base is an unbaked `TypeBuilder`. This is the key Reflection.Emit gotcha for the chain design — agents 4+ must keep using the tracked `ConstructorBuilder` for emitted bases and `GetConstructor(Instance|Public|NonPublic, …, EmptyTypes)` for the library base.
  - Struct fields: scalars keep their exact CLR type; packed i8→`byte`, i16→`ushort`; references→`object`/`Delegate` (via `ValType.ToSystemType(map.Kind)`). Fields are declared **public**; a subtype declares only its appended fields and reuses the ancestor's `FieldBuilder`s for inherited slots (`GCField[]` absolute layout). No gc/struct fixture actually uses struct subtyping, so the chain path is built-but-unexercised by gc/struct (it is exercised for correctness by whatever array/struct types the green `type-*` categories declare, which now bake real classes).
  - **Array emission is already scaffolded** (`DefineArray`, the `Length` getter override, `GetArray`, `RequireArray`, element metadata) as an M8.4 head-start; the array *instructions* are not wired yet.
- **`ValType.ToSystemType` is now kind-aware** (design §3b/§3c). Added an overload `ToSystemType(FunctionType[]? indexedKinds)`: a concrete struct/array index → `object`, a concrete func index → `Delegate` (the parameterless overload keeps the pre-3.0 Delegate-for-all-indices behavior). This was **mandatory**, not optional — a func type with a `(ref $struct)` parameter/result (e.g. `struct.7` type 2 `((ref 0))->f32`) must lower that slot to `object`, else the emitted method signature is `Delegate` and calls fail with an IL type mismatch. Threaded through: the `Signature` ctors (new `indexedKinds` overloads), `CompilationContext.SystemType(ValType)` helper (used by the ferry/result/return/local sites in `CompilationContext`, `Return`, `TailCallHelper`, `SelectWithType`, `TryTable`, `Throw`), the local-declaration site and the defined-global CLR-type sites in `Compile.cs`. Imported-global GC slots were left as-is (no GC imports exist per §0d).
- **Immutable-global caching fix (a real correctness bug surfaced by gc/struct):** an immutable global was lowered as a getter that *re-runs* its constant expression on every `global.get`. That is unsound for a GC allocation (`struct.new`/`array.new`/`ref.i31`) — each `global.get` produced a **fresh** object, so `struct.set` mutations never persisted (struct.10's `set_get_packed_*`). Fix (`Compile.cs`, defined-globals loop): an immutable global whose content lowers to `object` (a GC/extern reference slot) is now cached once in an init-only field at instantiation and the getter reads the field — exactly the mutable-global path minus the setter. Numeric/funcref immutable globals keep the re-run path (stable, byte-identical). Agent 4 gets array globals for free from this.
- Instruction plumbing: `OpCode.GCPrefix = 0xFB` (added to the enum; **`ref.eq` 0xD3 was intentionally NOT added yet** — that is M8.5, and adding it early would have changed `((OpCode)211).ToString()` from `"211"` to a name and broken the `GCTypeModelTests` pending-signature detector). New `WebAssembly/GCOpCode.cs` (`enum GCOpCode : byte`, all values 0x00–0x1E with `[OpCodeCharacteristics]` native names + a `ToNativeName` extension, mirroring `MiscellaneousOpCode`), new abstract `GCInstruction` base (OpCode=GCPrefix, abstract `GCOpCode`, `WriteTo` = prefix + varuint(subop) + `WriteImmediates`, `ToString` = native name), and the `0xFB` dispatch in **both** `Instruction.Parse` and `Instruction.ParseInitializerExpression`.
- Six struct instruction classes: `StructNew`, `StructNewDefault`, `StructGet`, `StructGetSigned`, `StructGetUnsigned`, `StructSet` (get/set share an abstract `StructFieldInstruction` base carrying `Type`+`Field`). IL discipline:
  - `struct.new`: spill operands (reverse) into locals, `newobj` parameterless ctor, then `dup;ldloc;stfld` per field (packed → `conv.u1`/`conv.u2`); pushes `(ref $t)` non-null.
  - `struct.new_default`: `newobj` parameterless ctor; rejects a non-nullable-ref (non-defaultable) field at validation.
  - `struct.get`: `castclass EmittedClass; ldfld`; rejects packed fields (require `_s`/`_u`).
  - `struct.get_s`: `castclass; ldfld; conv.i1/conv.i2`; `struct.get_u`: `castclass; ldfld` (byte/ushort `ldfld` is already zero-extended); both reject non-packed.
  - `struct.set`: pops value then ref, spills value (packed conv), `castclass; ldloc; stfld`; rejects immutable fields with a `ModuleLoadException` (message contains `"field is immutable"`). Null-ref traps are the natural CLR `NullReferenceException` from `ldfld`/`stfld` after a `castclass` of null.

**Unimplemented-GC pending signatures (critical for `GCTypeModelTests`, 210 cases, which parse via `Module.ReadFromBinary`):** the `0xFB` dispatch defaults for non-struct sub-opcodes deliberately keep the exact substrings the test's `IsPendingGCInstruction` matches — the general parser throws `Don't know how to parse opcode "251" (GC …)` (contains `opcode "251"`) and the initializer parser throws `Opcode "251" is not permitted in intializer expressions (GC …)` (contains `"251" is not permitted in intializer expressions`). Do NOT reword these without re-checking that detector. When agent 4 wires array/i31/etc. those fixtures flip from "pending → skipped" to "parses → round-trips byte-identically", so array/i31 `WriteTo` must be byte-exact.

**Harness (`SpecTestRunner`, test-only):** added `"field is immutable"` to the assert_invalid→`ModuleLoadException` arm; added `"null structure reference"`/`"null array reference"` to the assert_trap→`NullReferenceException` arm; and added a `CompareReturnValue` arm for bare `structref`/`arrayref`/`eqref`/`i31ref` (non-null-of-kind via `WasmStructObject`/`WasmArrayObject`/`WasmI31`/`IWasmEq`, `"null"`→`actual is null`). Meta-test upkeep: `OpCodeTests` gained the `"gc"→"GCPrefix"` replacement; `InstructionTests.Instruction_NameMatchesOpcode` now excludes `GCInstruction` and a new `Instruction_NameMatchesGCOpcode` mirrors the SIMD/misc variants (GC enum names equal the class names). `.editorconfig` gained `dotnet_code_quality.CA1711.allowed_suffixes = New` (the `struct.new`/`array.new` family).

**Curation:** `gc/struct` flipped `RunAllSkipped` → `Run` (no skips — it fully passes, including the packed sign/zero-extension, the null-ref traps, the immutable-field and unknown-type/type-mismatch asserts, and the bare-`structref` return). No per-line skips were needed.

**Gate (0 failed, 17 skipped, down from 18):** net9.0 Debug 1244 passed / net9.0 Release 1244 / net8.0 Debug 1240 / net10.0 Release 1244 — all 17 skipped. (net8 has 4 fewer total: the net9+-only persisted tests.) `GCTypeModelTests` 210/210 stayed green throughout.

**Handoff — where M8.4 (agent 4) should start:** the array head-start is already in `GCTypeEmitter` — `EnsureDefined` handles `FunctionType.Array` (base `WasmArrayObject`, a public `☘ Items` `T[]` field, an override of `get_Length` returning `Items.Length`, element CLR type via `ElementClrType`), plus `GetArray`/`RequireArray`/`GetClass`/`Has`. Wire the thirteen `array.*` instruction classes (§0b 0x06–0x13) into both `Instruction.cs` dispatch switches (general + initializer for the const subset `array.new`/`array.new_default`/`array.new_fixed`), following the six struct classes as templates:
- `array.new`/`new_default`/`new_fixed`: allocate `new T[count]` (packed conv on store), `newobj` the parameterless ctor, `stfld Items`. `array.new_default` rejects non-defaultable elements exactly like `struct.new_default`.
- `array.new_data`/`init_data` and `array.new_elem`/`init_elem`: reuse the existing passive-segment fields — `context.DataSegments[dataidx]` (a `byte[]` field; see `Instructions/MemoryInit.cs` for the pattern) and `context.ElementSegments[elemidx]` / `context.PassiveElementSegments` (see `Compile.cs` element section). Out-of-range is an `IndexOutOfRangeException` trap.
- `array.get(_s/_u)`/`array.set`/`array.len`/`array.fill`/`array.copy`: `castclass EmittedArray; ldfld Items; …` (packed sign/zero-extension identical to the struct field rules; `array.len` = `castclass WasmArrayObject; callvirt get_Length`). `array.copy` needs overlap-safe direction (mirror `memory.copy`); element assignability is validated via the canonical map's field/element subtyping (M8.2's `IsValidFieldSubtype` rules).
- Harness: the bare `arrayref` comparison and `"null array reference"` trap are **already wired** (added in M8.3). Flip `gc/array`, `gc/array_copy`, `gc/array_fill`, `gc/array_new_data`, `gc/array_new_elem`, `gc/array_init_data`, `gc/array_init_elem` to `Run`, re-deriving any per-line skips honestly (a line needing `ref.cast`/`br_on_cast` belongs to M8.6 — skip it with a precise comment rather than forcing).
- M8.5 (i31/ref.eq) and M8.6 (casts) build directly on the base hierarchy: `ref.i31` = `and 0x7FFFFFFF; newobj WasmI31`; `i31.get_u` = `castclass WasmI31; ldfld/callvirt Value`; `ref.eq` = add `OpCode.RefEq = 0xd3` (now safe, since M8.3 left it out) + `call WasmGC.RefEq`; concrete `ref.cast`/`ref.test` = `castclass`/`isinst` against `GCTypeEmitter.GetClass(typeidx)`, abstract targets against `WasmStructObject`/`WasmArrayObject`/`WasmI31`/`IWasmEq` per §3f.

### Implementation notes (M8.4 + M8.5, agent 4)

Both milestones landed complete and fully green across the matrix.
M8.5 was taken on after the M8.4 gate because two M8.4 targets (`gc/array`'s array.8 and all of `gc/array_new_elem`) hold i31 element segments and could not flip without it.

**M8.4 — arrays (design §3e).**

- Thirteen instruction classes under `Instructions/`, plus a shared abstract `ArrayInstruction` base (single `Type` index; used by `ArrayNew`/`ArrayNewDefault`/`ArrayGet`/`ArrayGetSigned`/`ArrayGetUnsigned`/`ArraySet`/`ArrayFill`).
  The two-immediate ops (`ArrayNewFixed` typeidx+size, `ArrayNewData`/`ArrayInitData` typeidx+dataidx, `ArrayNewElem`/`ArrayInitElem` typeidx+elemidx, `ArrayCopy` dsttypeidx+srctypeidx) and the zero-immediate `ArrayLen` derive directly from `GCInstruction`.
  All immediate orders were verified against fixture bytes before coding (`array.copy` = `fb 11 <dst> <src>`, `array.new_fixed` = `fb 08 <t> <size>`, `array.new_data` = `fb 09 <t> <data>`, etc.).
- Wired into both `Instruction.cs` dispatch switches: all thirteen in the general parser, and `array.new`/`array.new_default`/`array.new_fixed` in `ParseInitializerExpression` (the constant subset, §0c).
- IL follows the six struct classes. `castclass EmittedArray; ldfld Items; ldelem/stelem <elemClr>` for get/set; packed i8→`byte`/i16→`ushort` with `ldelem`-zero-extended reads, `conv.i1/i2` for `_s`, `conv.u1/u2` truncation on stores; `array.len` = `castclass WasmArrayObject; callvirt get_Length`.
  The natural CLR `IndexOutOfRangeException` from `ldelem`/`stelem` is the "out of bounds array access" trap; a null array reference NREs from the `ldfld Items`.
- The passive-segment and bulk operations are lowered to six new **public** `WasmGC` runtime helpers (called directly like `UnmanagedMemory.InitFromSegment`, no `NeutralizeType` on the call site — the persisted path is M8.7): `NewArray<T>`/`FillArray<T>` (array.new/fill), `NewArrayFromData<T>`/`InitArrayFromData<T>` (data-segment reinterpret via `MemoryMarshal.AsBytes`, generic over the element storage type through `MakeGenericMethod`), `NewArrayFromElement`/`InitArrayFromElement` (element-segment copy via `Array.Copy`, non-generic over `Array` since references are `object[]`/`Delegate[]`), and `ArrayCopy` (overlap-safe via `Array.Copy`).
  The array helpers deliberately throw `IndexOutOfRangeException` for a uniform array trap (a scoped `#pragma warning disable CA2201` documents the intentional use of the runtime-reserved type — it matches what `ldelem`/`stelem` throw).
- Data-segment helpers accept `byte[]?`: a **zero-length passive data segment is never assigned** (the field stays null — `Compile.StorePassiveDataSegment` returns early on empty data), so an out-of-range read of an empty/dropped segment must trap (`MemoryAccessOutOfRangeException`) rather than NRE. This was the one non-obvious runtime bug; a `/tmp` micro-harness confirmed the reinterpretation and the null-field trap early.
- Validation: `array.set`/`fill`/`init_*` reject immutable arrays ("array is immutable"); `array.new_data`/`init_data` reject reference elements ("array type is not numeric or vector"); `array.copy` checks element assignability (packedness exact, numeric exact, references covariant via `context.IsSubtype`) and rejects an immutable destination ("array types do not match" / "array is immutable"); `array.new_default` rejects a non-nullable-reference element.
- **Latent M8.3 bug fixed:** the emitted GC classes were `TypeAttributes.Sealed`, which faults `CreateType` with "the parent type is sealed" the instant a declared subtype chain appears (unexercised by `gc/struct`, but `gc/ref_eq` declares one). Removed `Sealed` from `GCClassAttributes`.

**M8.5 — i31 + ref.eq (design §3g).**

- `RefI31` (`and 0x7FFFFFFF; newobj WasmI31`), `I31GetSigned` (`castclass WasmI31; callvirt get_Value; shl 1; shr 1` to sign-extend bit 30), `I31GetUnsigned` (`castclass; callvirt get_Value`).
  `ref.i31` is also wired into `ParseInitializerExpression`.
  A null i31 NREs from the `castclass`/`callvirt` ("null i31 reference", added to the harness).
- `RefEqual` (a `SimpleInstruction`, single byte `0xD3`) lowering to `call WasmGC.RefEq`.
  **Naming:** the design's example `RefEqual`/`RefEq` ambiguity resolves to **`RefEqual`** — `OpCodeTests.OpCode_NameMatchesCharacteristics` maps `eq`→`Equal` (as `i32.eq`→`Int32Equal`), so both `OpCode.RefEqual` and the instruction class must be `RefEqual` (the runtime helper `WasmGC.RefEq` keeps its short name).
  This is the interaction agent 3 flagged: adding the enum member changed `((OpCode)0xD3).ToString()` from `"211"`, which the meta-test caught — fixed honestly by matching the established convention, no other test relied on the numeric string.
- **Element-segment GC constants:** `ValidateElementInitExpr` and `EmitElementRefValue` (`Compile.cs`) were generalized from "exactly one reference-producing instruction" to inspect the instruction before `end`, adding the `[<i32 const>, ref.i31, end]` shape (the i32 operand may be an `i32.const` **or** a `global.get` of an imported i32 global — `i31.3.wasm` uses the latter in a table initializer). This is what lets i31 flow into globals, tables, and passive element segments.

**Curation (SpecTests.cs), re-derived from actual results.**

- Fully green (`Run`, no skips): `gc/array_copy`, `gc/array_fill`, `gc/array_init_data`, `gc/array_init_elem`, `gc/array_new_data`, `gc/array_new_elem`, `gc/ref_eq`.
- `gc/array` — `Run` with a skip list for **module array.8** (line 219 + its asserts 276–290): its passive element segments are initialized with **whole-array constants** built by `array.new`/`array.new_fixed` in the element initializer expressions. The element-init path (`EmitElementRefValue`) only handles single-value reference constants (`ref.func`/`ref.null`/`global.get`/`ref.i31`); emitting a nested array-allocation constant needs the general constant-expression compiler wired into the element path — a later-milestone extension, not just i31. Everything else in `gc/array` runs (all get/set/get_s/get_u/new/new_default/new_fixed/len, packed sign/zero-extension, OOB and null traps, the immutable/constant-expression/unknown-type invalids).
- `gc/i31` — `Run` with a skip list for **modules i31.5 (line 150) and i31.6 (line 168)** and their asserts: both downcast an anyref back to i31ref with `ref.cast` (0x17), which is M8.6. Everything else runs (ref.i31, i31.get_s/u, i31 in globals and table/element initializers including via an imported global).

**Gate (0 failed, 10 skipped, down from 17):** net9.0 Debug 1271 passed / net9.0 Release 1271 / net10.0 Debug 1271 / net8.0 Release 1267 — all 10 skipped. (net8 has 4 fewer total: the net9+-only persisted tests.) `GCTypeModelTests` 210/210 stayed green throughout (the array/i31/ref_eq fixtures now parse and round-trip byte-identically instead of matching the pending signature).
The 10 remaining skips: nine spec categories — `gc/ref_test`, `gc/ref_cast`, `gc/br_on_cast`, `gc/br_on_cast_fail`, `gc/extern` (M8.6); `gc/type-subtyping` (M8.7 runtime asserts); `gc/array` and `gc/i31` (the two partial skips above); `type-rec` (4 lines pending from M8.2, unchanged) — plus the always-skipped `DiscoverAllFailures` diagnostic helper (not a spec category).
Seven categories flipped fully green this milestone, so spec-category skips dropped 16→9.

**Handoff — where M8.6 (agent 5) should start (casts and conversions, design §3f/§4).**

- Implement `ref.test`/`ref.cast` (both null variants 0x14–0x17), `br_on_cast`/`br_on_cast_fail` (0x18/0x19, castflags per §0b), and `any.convert_extern`/`extern.convert_any` (0x1A/0x1B, IL no-ops per §3h). The `GCOpCode` enum members already exist; wire them into the general `Instruction.cs` dispatch (`br_on_cast` needs the block-relative label plumbing like `br_on_null`, routed through the Phase 6 `EmitBranchTransfer` choke point).
- Concrete targets: `castclass`/`isinst` against `GCTypeEmitter.GetClass(typeidx)` (now that classes are unsealed, chains work). Abstract targets: `WasmStructObject`/`WasmArrayObject`/`WasmI31`/`IWasmEq`, with `any`/bottom handled as the null-check/constant reductions in §3f.
- Flipping `ref.cast` **also closes the two partial skips this milestone left**: remove the `gc/i31` skip list (i31.5/i31.6 only need `ref.cast`) and re-check `gc/array` — but note array.8's residual blocker is **array constants in element segments**, not casts, so array.8 needs the element-init generalization below, independent of M8.6. Confirm with a run before removing that skip.
- **The element-init generalization (array constants in element segments)** is the one genuinely deferred feature: `EmitElementRefValue`/`ValidateElementInitExpr` handle only single-value reference constants plus the `[<i32>, ref.i31, end]` shape. Supporting `[…, array.new_fixed, end]` / `[…, array.new, end]` (and `struct.new`) means running an arbitrary constant expression into the element/table-init IL. This is the last thing gating `gc/array` fully; it and `gc/type-subtyping` are the natural M8.7 close-out items alongside the persisted-assembly test and the `Wasm3Plan.md` checkboxes.
- `WasmGC` gained six public array helpers and the emitted classes are now unsealed — both are additive and the persisted path (`NeutralizeType`) already resolves `WasmGC`/`WasmI31`/`WasmArrayObject`; the generic-method calls (`MakeGenericMethod`) and the direct `WasmGC` static calls are the only spots M8.7's persisted test should double-check.

### Implementation notes (M8.6 + partial M8.7, agent 5)

M8.6 landed complete and fully green across the matrix.
Two M8.7 items landed (the persisted-assembly test and the doc/checkbox updates); the two remaining GC spec-category skips (`gc/array` array.8 and `gc/type-subtyping`) are deferred with precise reasons below.

**M8.6 — casts and conversions (design §3f/§3h/§4).**

- New instruction classes: `RefTest`/`RefCast` (each a subclass of a new abstract `ReferenceTestInstruction` carrying a nullability flag plus an s33 heap-type immediate — a concrete index or an abstract heap type); `BranchOnCast`/`BranchOnCastFail` (subclasses of a new abstract `BranchCastInstruction` carrying the cast-flags byte, label index, and the from/to heap types); `AnyConvertExtern`/`ExternConvertAny`.
  The nullable/non-null opcode pair is one class with a `Nullable` flag read from the opcode (`GCOpCode => Nullable ? RefTestNull : RefTest`), so the parameterless-ctor default is the non-null variant and `Instruction_NameMatchesGCOpcode` stays satisfied; `WriteTo` re-emits the exact original opcode from the flag.
- All immediate encodings were verified against the fixture bytes before coding (`ref_test.0` `fb 14 {6a..70}` covering array/struct/i31/eq/any/extern/func; `ref_test.1` `fb 15 {71,72,73}` bottom types + `fb 14 00` concrete; `br_on_cast.0` `fb 18 01 00 6e 6a` = flags-1/label-0/any→array and `fb 18 03 01 6e 6b` = flags-3/label-1/any→struct).
- Shared lowering lives in `Instructions/CastHelper.cs`: `Classify(heapType?, typeIndex?)` maps a target to one of three runtime shapes — `IsInst` (concrete index → `GCTypeEmitter.GetClass`, or `Delegate` for a concrete function index; `struct`→`WasmStructObject`, `array`→`WasmArrayObject`, `i31`→`WasmI31`, `eq`→`IWasmEq`, `func`→`Delegate`), `Top` (`any`/`extern`/`exn` — every non-null value matches), and `Bottom` (`none`/`nofunc`/`noextern`/`noexn` — nothing matches). `EmitTest` produces an i32 (isinst+`ldnull`+`cgt.un`, with an explicit null→1 branch for the nullable variant; `Top`/`Bottom` reduce to non-null-check / const / is-null per §3f). `EmitCast` leaves the (checked) reference: `castclass` for `IsInst`, a null guard for the non-null `Top`, and traps for `Bottom`.
- **Trap-text correction (design risk 2 territory):** the fixtures pin `ref.cast` of **null** with a non-null target to `"cast failure"` (`InvalidCastException`), *not* `"null reference"` — the "null reference" traps come from `ref.as_non_null`, never from `ref.cast`. So the non-null-cast null guard throws `InvalidCastException`, not `NullReferenceException` (verified against `ref_cast.0`'s `ref_cast_i31` where every null selector expects "cast failure"). The design's "cast non-null = ThrowNullReference" wording is superseded by the fixture. A tiny additive `WasmGC.FailCast(object) : object` (always throws `InvalidCastException`) backstops the bottom-non-null cast while keeping the IL stack balanced.
- `br_on_cast`/`br_on_cast_fail` compose the Phase 6 `BranchHelper.EmitBranchTransfer` exactly as `br_on_non_null` does: pop the operand, `dup` + `CastHelper.EmitTest(to)` + `brfalse`/`brtrue` a skip label, transfer (carrying the re-typed reference as the label's last operand), then fall through with the residual (`ref null?_from` heap `from`, nullable = `from ∧ ¬to`). Validation enforces `to <: from`, the operand `<: (ref null? from)`, and `carried <: label-last`; `WriteTo` round-trips the castflags byte + label + both heap types byte-identically (confirmed by `GCTypeModelTests`, which now parses+round-trips the cast/`br_on_cast`/`extern` fixtures instead of matching the pending signature).
- `any.convert_extern`/`extern.convert_any` are IL no-ops (ValType-only; nullability preserved), wired into both the general parser and — since they are const (§0c) — `ParseInitializerExpression`.

**A latent branch-machinery gap fixed (shared, low-risk).** Branching to a target whose single result is a *long-form/indexed* reference (e.g. a function returning `(ref $t)`) hit `BlockType.TryToValueType` returning false (the `FunctionOuterBlock` stores only a `BlockType`, which cannot encode an index), so the single-value branch path silently fell through to the void path. `br_on_cast.2`/`.5` (branching to a single-`(ref …)`-return function) are the first fixtures to exercise it. Added `BranchHelper.TrySingleResultType`, which recovers the type from `context.CheckedSignature.RawReturnTypes` for a function boundary, and routed the reachable single-value paths (`EmitReachableBranch`, `EmitBranchTransfer`) and `br_on_cast`'s label-type check through it. Blocks with a synthesized single-ref `BlockSignature` were already fine (they go through the multi-value `branchTypes` path).

**A latent End-validation gap fixed (shared).** `End` for a multi-value / synthesized-`BlockSignature` block only *count*-checked its results — it never subtype-validated them (only the inline single-`BlockType` path called `ValidateStack`). A `br_on_cast` fall-through leaving a nullable residual where the block declares a non-null result (`br_on_cast.5`) therefore slipped through. Added `BranchHelper.ValidateBranchTypes` to `End`'s reachable multi-value path. Verified no regression across `block`/`if`/`loop`/`func`/`type*`/`br*` and the full suite.

**Harness (test-only).** Added `"cast failure"`/`"cast"` → `InvalidCastException` (the wasm-tools text differs between suites); made `AnyRefValue` box its host string payload like `ExternRefValue` (an internalized/externalized extern surfaces as its opaque string) and added `AnyRefValue`/bare-`ExternRefValue` comparison arms (bare = any non-null reference, `"null"` = null, payload = string compare) — this is what `gc/extern`'s `internalize`/`externalize` round-trips assert.

**Curation.** Flipped `gc/ref_test`, `gc/ref_cast`, `gc/br_on_cast`, `gc/br_on_cast_fail`, `gc/extern` to `Run` (no skips); removed the `gc/i31` skip list entirely (i31.5/i31.6's `ref.cast` downcasts now work). Unit tests added for all six new instruction classes (`Instruction_HasTestClass`).

**M8.6 gate — 0 failed, 4 skipped (down from 10):** net9.0 Debug 1283 passed / net9.0 Release 1283 / net8.0 Release 1279 / net10.0 Debug 1283 — all 4 skipped. (net8 has 4 fewer total: the net9+-only persisted tests.) The 4 remaining skips are three spec categories (`gc/array` array.8, `gc/type-subtyping`, `type-rec` 4 lines) plus the always-skipped `DiscoverAllFailures` diagnostic helper. Seven spec categories flipped fully green this milestone, so spec-category skips dropped from 9 to 6 (of which two are wholesale-deferred, below).

**M8.7 — what landed.**

- Persisted-assembly coverage: two new `PersistedAssemblyTests` (`PersistedAssembly_StructCast_RoundTrip`, `PersistedAssembly_I31_RoundTrip`, net9+ only) compile a struct-allocate + abstract/concrete `ref.cast` + `struct.get` module and an `ref.i31` + `ref.cast` + `i31.get_u` module to a DLL, load it, and invoke — exercising the emitted GC class, its manual parameterless ctor, and `WasmStructObject`/`WasmI31` under the persisted `NeutralizeType` mapping. Both pass.
- `Wasm3Plan.md` Phase 8 checkboxes checked with per-milestone attribution and a residual-deferrals note.

**M8.7 — what remains (for a potential agent 6), with precise blockers.**

- **`gc/type-subtyping` (wholesale-deferred, `RunAllSkipped`).** Investigated fully: every one of its runtime asserts exercises *function-type identity* — `ref.test`/`ref.cast` to a concrete **function** index and `call_indirect` canonical-type mismatch (disassembled `type-subtyping.17`–`.22`; the modules are all `func`/`struct`-of-func-fields with no struct/array *value* casts). At run time every funcref is an arity-shaped `Delegate`, so two structurally-distinct function types are indistinguishable; distinguishing them needs per-canonical-type runtime tags on funcrefs — the same invasive runtime-representation change blocking `type-rec` 183/192. The struct/array/i31 subtyping the cast machinery *does* handle is fully exercised by the other green `gc/*` categories, so isolating this category into a Run+skip would be a large fragile skip list that stays Inconclusive anyway.
- **`gc/array` array.8 (per-line skip retained).** Its passive element segments are initialized with whole-array constants (`array.new_fixed`/`array.new` in the element-item init expressions). `EmitElementRefValue`/`ValidateElementInitExpr` handle only single-value reference constants plus `[<i32>, ref.i31, end]`; supporting nested GC allocations needs the general constant-expression compiler routed into the element-init IL (the active-table-offset path at `Compile.cs` already does this via `context.Reset` + `instruction.Compile`, so the mechanism exists — the fiddly part is doing it per-element without disturbing the interleaved array-build IL, which is why it was left rather than risked late). This is self-contained and *would* reduce the skip count; a good first task for agent 6.
- **`type-rec` 149/157/183/192** unchanged from M8.2 (function-import closed descriptors + runtime canonical `call_indirect` — both the func-identity family above).

**Surprises.** (1) `ref.cast` of null traps as `"cast failure"`/`InvalidCastException`, not `"null reference"` — the design and my first cut had it as `NullReferenceException`; the fixture is the oracle. (2) Two latent shared gaps (single-long-form-ref-return branch target; multi-value block-result subtype validation) surfaced only under `br_on_cast` and needed careful, regression-checked fixes to the Phase-5/6 branch/end machinery rather than anything cast-specific.

### Implementation notes (M8.8 + close-out, agent 6)

M8.8 landed complete and fully green across the whole matrix, and Phase 8 is closed out.
The two blockers agent 5 handed off (gc/array `array.8` and runtime function-type identity) are both resolved; the suite's only remaining skip is the always-ignored `DiscoverAllFailures` diagnostic.

**Task 1 — array constants in element segments (closed `gc/array` `array.8`).**

- `EmitElementRefValue` (`Compile.cs`) now falls through to a new `EmitConstantElementExpr` helper for any per-element initializer that is not one of the simple single-value reference shapes (`ref.func`/`ref.null`/`global.get`/`ref.i31`).
  The helper runs the general constant-expression compiler exactly as the active-table-offset and cached-global paths do — `context.Reset(il, Signature.Empty, …)`, `ConstantExpression = true`, compile each instruction, stop at `end` to leave the produced reference on the IL stack — so nested GC allocations (`array.new`/`array.new_fixed`/`struct.new`) build their element item in place.
  `ValidateElementInitExpr` gained the matching produced-type cases (`(ref $t)` for each GC allocation).
- **The real bug behind `array.8`** was not the const-expr routing but `CompilationContext.IsFunctionReference`: it used the *parameterless* `ValType.ToSystemType()`, which reports **every** concrete type index as `Delegate` (the pre-3.0 default), so an element segment of concrete array-reference type got a `Delegate[]` backing field and the array items faulted with `ArrayTypeMismatchException` at instantiation.
  Fixed by making `IsFunctionReference` kind-aware (`ToSystemType(indexedKinds)` via `context.CanonicalTypes?.Kind`) and threading `context.CanonicalTypes?.Kind` through its three `Compile.cs` call sites and `IsFuncTable`.
  This is a general correctness fix for any concrete-struct/array-reference table or element segment, not just `array.8`.

**Task 2 — runtime function-type identity (design §10).**

The mechanism is a process-global registry in `WasmGC` (all additive public API): a `ConditionalWeakTable<Delegate, object[]>` mapping each WASM-typed funcref delegate to its canonical type **ancestry** (own closed descriptor + each declared-supertype descriptor up the chain), with descriptor strings interned to token objects via a static `ConcurrentDictionary<string, object>` so runtime checks are O(ancestry) reference-equality membership tests.
Subtyping is membership: `A <: E` exactly when `E`'s descriptor token appears in `A`'s ancestry.
The ancestry is carried as a single newline-joined string (`CanonicalTypeMap.Ancestry(typeIndex)`), emitted as one `ldstr` and split/interned at registration.

- **New `WasmGC` members**: `InternFunctionType`, `RegisterFunctionReference`, `FunctionReferenceMatches`, `CheckIndirectCallType` (call_indirect guard), `IsFunctionReferenceOfType` (ref.test), `CastFunctionReference` (ref.cast).
- **New `CanonicalTypeMap.Ancestry(uint)`** — walks `Supertype` collecting `Descriptor` (module-independent, so cross-module identity is free, same as tag import matching).
- **Registration sites**: `EmitFunctionReferencesInitialization` registers every `FunctionReferences[i]` (internal *and* imported — imported functions get a registered wrapper delegate) with the ancestry of `functionSignatures[i].TypeIndex`. Because `ref.func`, element segments (func-index and init-expr forms), funcref globals, and table population all load from `FunctionReferences`, this one runtime site covers all of them. `RuntimeImport.FromCompiledExports` registers each re-imported export delegate from its `NativeExportAttribute.Descriptor` (function exports now carry the ancestry there).
- **Checks**: `CallIndirect` guards its existing `castclass` with `WasmGC.CheckIndirectCallType` (`dup; ldstr descriptor; call`), trapping `InvalidCastException("indirect call type mismatch")` on a registered-and-mismatched callee (null/unregistered fall through to the castclass, unchanged). `CastHelper` gained a `TargetKind.FuncType` for a concrete function index, routing `ref.test`/`ref.cast`/`br_on_cast*` through `IsFunctionReferenceOfType`/`CastFunctionReference` instead of `isinst/castclass Delegate`. `ImportException.ValidateFunction` (emitted at the function-import wiring) throws `ImportException` when the supplied delegate's registered ancestry does not contain the declared import type descriptor — the `assert_unlinkable "incompatible import type"` path.
- **Unregistered (host) delegates** always fall back to CLR-shape acceptance (`FunctionReferenceMatches` returns true when the delegate is not in the registry), so host embeddings are unaffected; spec modules never produce unregistered funcrefs.
- Minor plumbing: added a `CompilationContext.Emit(ILOpCode, string)` overload for `ldstr`.

**Curation.** `gc/array` → `Run` (no skips); `type-rec` → `Run` (149/157/183/192 removed); `gc/type-subtyping` → `Run` (from `RunAllSkipped`). Harness (`SpecTestRunner`) gained one `assert_invalid` arm mapping `"sub type"` / `"sub type N does not match super type M"` to `ModuleLoadException` (the declared-subtype legality errors from M8.2 that only `type-subtyping` exercises at whole-category scope); `"indirect call type mismatch"` and `"incompatible import type"` were already mapped (`InvalidCastException` / `ImportException`).

**Close-out.** No new `unsupported` lines were needed — every surveyed behavior has a conforming CLR lowering (design risk 8 held). A full-suite run confirms the only skipped test across all categories is `DiscoverAllFailures`; `float_exprs`' seven JIT-folding `no_fold` lines remain the sole `unsupported` (intentional-avoidance) lines, and keep their category green. The `Wasm3Plan.md` Phase 8 checkboxes and the cross-cutting Deterministic-profile checkboxes are ticked with the stance documented.

**Gate.** All six cells green, zero failures: net9.0 Debug 1288 passed / 1 skipped, and the equivalent across net8.0/net10.0 × Debug/Release (net8 has fewer total — the net9+-only persisted-assembly tests). `GCTypeModelTests` (byte-identical round-trip) stayed green throughout.

**Surprises.** (1) `array.8` looked like a const-expr-routing gap but was really the `IsFunctionReference` kind-blindness above — the routing was necessary but not sufficient. (2) One registration site (`FunctionReferences`) covers ref.func / element segments / globals / tables because they all load the same delegate instances; only the cross-module export path (`FromCompiledExports`) and function imports needed their own registration/check. (3) `call_indirect`'s mismatch trap reuses `InvalidCastException`, which the harness already maps for "indirect call type mismatch" — no new trap taxonomy.
