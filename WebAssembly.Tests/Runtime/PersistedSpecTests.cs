#if NET9_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Runtime;

/// <summary>
/// Runs the official specification's tests through the persisted WASM-to-DLL path
/// (<see cref="Compile.CreatePersistedAssembly"/>): every category is compiled to a standalone assembly,
/// saved, loaded into a collectible context, instantiated, and executed. This proves the persisted emission
/// path against the same spec suite the in-memory <see cref="SpecTests"/> class covers.
/// </summary>
/// <remarks>
/// Each method mirrors its in-memory <see cref="SpecTests"/> counterpart so a persisted-only failure is
/// immediately attributable. Per-line curation (skip/unsupported) is shared with <see cref="SpecTests"/> via
/// <see cref="SpecTests.Curation"/> — the lists are never copied. The per-category rationale for any exclusion
/// lives on the in-memory method that owns the scenario.
/// </remarks>
[TestClass]
public class PersistedSpecTests
{
    /// <summary>Runs the address tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_address() => SpecTestRunner.Run("address", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the align tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_align() => SpecTestRunner.Run("align", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the binary tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_binary() => SpecTestRunner.Run("binary", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the binary-leb128 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_binary_leb128() => SpecTestRunner.Run("binary-leb128", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the block tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_block() => SpecTestRunner.Run("block", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the br tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_br() => SpecTestRunner.Run("br", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the br_if tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_br_if() => SpecTestRunner.Run("br_if", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the br_table tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_br_table() => SpecTestRunner.Run("br_table", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the call tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_call() => SpecTestRunner.Run("call", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the call_indirect tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_call_indirect() => SpecTestRunner.Run("call_indirect", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the comments tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_comments() => SpecTestRunner.Run("comments", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the const tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_const() => SpecTestRunner.Run("const", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the conversions tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_conversions() => SpecTestRunner.Run("conversions", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the custom tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_custom() => SpecTestRunner.Run("custom", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the data tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_data() => SpecTestRunner.Run("data", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the elem tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_elem() => SpecTestRunner.Run("elem", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the endianness tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_endianness() => SpecTestRunner.Run("endianness", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the exports tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_exports() => SpecTestRunner.Run("exports", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f32 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f32() => SpecTestRunner.Run("f32", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f32_bitwise tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f32_bitwise() => SpecTestRunner.Run("f32_bitwise", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f32_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f32_cmp() => SpecTestRunner.Run("f32_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f64() => SpecTestRunner.Run("f64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f64_bitwise tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f64_bitwise() => SpecTestRunner.Run("f64_bitwise", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the f64_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_f64_cmp() => SpecTestRunner.Run("f64_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the fac tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_fac() => SpecTestRunner.Run("fac", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the float_exprs tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_float_exprs() => SpecTestRunner.Run("float_exprs", SpecTestRunner.SpecExecutionMode.Persisted, unsupported: SpecTests.Curation.FloatExprsUnsupported);

    /// <summary>Runs the float_literals tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_float_literals() => SpecTestRunner.Run("float_literals", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the float_memory tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_float_memory() => SpecTestRunner.Run("float_memory", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the float_misc tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_float_misc() => SpecTestRunner.Run("float_misc", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the forward tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_forward() => SpecTestRunner.Run("forward", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the func tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_func() => SpecTestRunner.Run("func", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the func_ptrs tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_func_ptrs() => SpecTestRunner.Run("func_ptrs", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the global tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_global() => SpecTestRunner.Run("global", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the i32 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_i32() => SpecTestRunner.Run("i32", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the i64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_i64() => SpecTestRunner.Run("i64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the id tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_id() => SpecTestRunner.Run("id", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the if tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_if() => SpecTestRunner.Run("if", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the imports tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_imports() => SpecTestRunner.Run("imports", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the inline-module tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_inline_module() => SpecTestRunner.Run("inline-module", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the int_exprs tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_int_exprs() => SpecTestRunner.Run("int_exprs", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the int_literals tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_int_literals() => SpecTestRunner.Run("int_literals", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the labels tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_labels() => SpecTestRunner.Run("labels", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the left-to-right tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_left_to_right() => SpecTestRunner.Run("left-to-right", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the linking tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_linking() => SpecTestRunner.Run("linking", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the load tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_load() => SpecTestRunner.Run("load", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the local_get tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_local_get() => SpecTestRunner.Run("local_get", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the local_set tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_local_set() => SpecTestRunner.Run("local_set", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the local_tee tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_local_tee() => SpecTestRunner.Run("local_tee", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the loop tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_loop() => SpecTestRunner.Run("loop", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory() => SpecTestRunner.Run("memory", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory_grow tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_grow() => SpecTestRunner.Run("memory_grow", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory_redundancy tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_redundancy() => SpecTestRunner.Run("memory_redundancy", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory_size tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_size() => SpecTestRunner.Run("memory_size", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory_trap tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_trap() => SpecTestRunner.Run("memory_trap", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the names tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_names() => SpecTestRunner.Run("names", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the nop tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_nop() => SpecTestRunner.Run("nop", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the obsolete-keywords tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_obsolete_keywords() => SpecTestRunner.Run("obsolete-keywords", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the return tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_return() => SpecTestRunner.Run("return", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the select tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_select() => SpecTestRunner.Run("select", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the skip-stack-guard-page tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_skip_stack_guard_page() => SpecTestRunner.Run("skip-stack-guard-page", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the stack tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_stack() => SpecTestRunner.Run("stack", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the start tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_start() => SpecTestRunner.Run("start", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the store tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_store() => SpecTestRunner.Run("store", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the switch tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_switch() => SpecTestRunner.Run("switch", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the table tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table() => SpecTestRunner.Run("table", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the token tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_token() => SpecTestRunner.Run("token", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the traps tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_traps() => SpecTestRunner.Run("traps", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the type tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_type() => SpecTestRunner.Run("type", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the unreachable tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_unreachable() => SpecTestRunner.Run("unreachable", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the unreached-invalid tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_unreached_invalid() => SpecTestRunner.Run("unreached-invalid", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the unreached-valid tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_unreached_valid() => SpecTestRunner.Run("unreached-valid", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the unwind tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_unwind() => SpecTestRunner.Run("unwind", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the utf8-custom-section-id tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_utf8_custom_section_id() => SpecTestRunner.Run("utf8-custom-section-id", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the utf8-import-field tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_utf8_import_field() => SpecTestRunner.Run("utf8-import-field", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the utf8-import-module tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_utf8_import_module() => SpecTestRunner.Run("utf8-import-module", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the utf8-invalid-encoding tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_utf8_invalid_encoding() => SpecTestRunner.Run("utf8-invalid-encoding", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/bulk tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_bulk() => SpecTestRunner.Run("bulk-memory/bulk", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/memory_copy tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_copy() => SpecTestRunner.Run("bulk-memory/memory_copy", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/memory_fill tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_fill() => SpecTestRunner.Run("bulk-memory/memory_fill", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/memory_init tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory_init() => SpecTestRunner.Run("bulk-memory/memory_init", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/table_copy tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_copy() => SpecTestRunner.Run("bulk-memory/table_copy", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/table_fill tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_fill() => SpecTestRunner.Run("bulk-memory/table_fill", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/table_init tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_init() => SpecTestRunner.Run("bulk-memory/table_init", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the bulk-memory/table-sub tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_sub() => SpecTestRunner.Run("bulk-memory/table-sub", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the ref_func tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_ref_func() => SpecTestRunner.Run("ref_func", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the ref_is_null tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_ref_is_null() => SpecTestRunner.Run("ref_is_null", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the ref_null tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_ref_null() => SpecTestRunner.Run("ref_null", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the table_get tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_get() => SpecTestRunner.Run("table_get", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the table_grow tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_grow() => SpecTestRunner.Run("table_grow", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the table_set tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_set() => SpecTestRunner.Run("table_set", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the table_size tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_table_size() => SpecTestRunner.Run("table_size", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_address tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_address() => SpecTestRunner.Run("simd/simd_address", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_align tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_align() => SpecTestRunner.Run("simd/simd_align", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_bit_shift tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_bit_shift() => SpecTestRunner.Run("simd/simd_bit_shift", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_bitwise tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_bitwise() => SpecTestRunner.Run("simd/simd_bitwise", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_boolean tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_boolean() => SpecTestRunner.Run("simd/simd_boolean", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_const tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_const() => SpecTestRunner.Run("simd/simd_const", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_conversions tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_conversions() => SpecTestRunner.Run("simd/simd_conversions", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f32x4 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f32x4() => SpecTestRunner.Run("simd/simd_f32x4", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f32x4_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f32x4_arith() => SpecTestRunner.Run("simd/simd_f32x4_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f32x4_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f32x4_cmp() => SpecTestRunner.Run("simd/simd_f32x4_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f32x4_pmin_pmax tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f32x4_pmin_pmax() => SpecTestRunner.Run("simd/simd_f32x4_pmin_pmax", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f32x4_rounding tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f32x4_rounding() => SpecTestRunner.Run("simd/simd_f32x4_rounding", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f64x2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f64x2() => SpecTestRunner.Run("simd/simd_f64x2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f64x2_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f64x2_arith() => SpecTestRunner.Run("simd/simd_f64x2_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f64x2_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f64x2_cmp() => SpecTestRunner.Run("simd/simd_f64x2_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f64x2_pmin_pmax tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f64x2_pmin_pmax() => SpecTestRunner.Run("simd/simd_f64x2_pmin_pmax", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_f64x2_rounding tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_f64x2_rounding() => SpecTestRunner.Run("simd/simd_f64x2_rounding", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_arith() => SpecTestRunner.Run("simd/simd_i16x8_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_arith2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_arith2() => SpecTestRunner.Run("simd/simd_i16x8_arith2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_cmp() => SpecTestRunner.Run("simd/simd_i16x8_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_extadd_pairwise_i8x16 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_extadd_pairwise_i8x16() => SpecTestRunner.Run("simd/simd_i16x8_extadd_pairwise_i8x16", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_extmul_i8x16 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_extmul_i8x16() => SpecTestRunner.Run("simd/simd_i16x8_extmul_i8x16", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_q15mulr_sat_s tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_q15mulr_sat_s() => SpecTestRunner.Run("simd/simd_i16x8_q15mulr_sat_s", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i16x8_sat_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i16x8_sat_arith() => SpecTestRunner.Run("simd/simd_i16x8_sat_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_arith() => SpecTestRunner.Run("simd/simd_i32x4_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_arith2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_arith2() => SpecTestRunner.Run("simd/simd_i32x4_arith2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_cmp() => SpecTestRunner.Run("simd/simd_i32x4_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_dot_i16x8 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_dot_i16x8() => SpecTestRunner.Run("simd/simd_i32x4_dot_i16x8", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_extadd_pairwise_i16x8 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_extadd_pairwise_i16x8() => SpecTestRunner.Run("simd/simd_i32x4_extadd_pairwise_i16x8", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_extmul_i16x8 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_extmul_i16x8() => SpecTestRunner.Run("simd/simd_i32x4_extmul_i16x8", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_trunc_sat_f32x4 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_trunc_sat_f32x4() => SpecTestRunner.Run("simd/simd_i32x4_trunc_sat_f32x4", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i32x4_trunc_sat_f64x2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i32x4_trunc_sat_f64x2() => SpecTestRunner.Run("simd/simd_i32x4_trunc_sat_f64x2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i64x2_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i64x2_arith() => SpecTestRunner.Run("simd/simd_i64x2_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i64x2_arith2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i64x2_arith2() => SpecTestRunner.Run("simd/simd_i64x2_arith2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i64x2_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i64x2_cmp() => SpecTestRunner.Run("simd/simd_i64x2_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i64x2_extmul_i32x4 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i64x2_extmul_i32x4() => SpecTestRunner.Run("simd/simd_i64x2_extmul_i32x4", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i8x16_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i8x16_arith() => SpecTestRunner.Run("simd/simd_i8x16_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i8x16_arith2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i8x16_arith2() => SpecTestRunner.Run("simd/simd_i8x16_arith2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i8x16_cmp tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i8x16_cmp() => SpecTestRunner.Run("simd/simd_i8x16_cmp", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_i8x16_sat_arith tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_i8x16_sat_arith() => SpecTestRunner.Run("simd/simd_i8x16_sat_arith", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_int_to_int_extend tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_int_to_int_extend() => SpecTestRunner.Run("simd/simd_int_to_int_extend", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_lane() => SpecTestRunner.Run("simd/simd_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_linking tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_linking() => SpecTestRunner.Run("simd/simd_linking", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load() => SpecTestRunner.Run("simd/simd_load", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load16_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load16_lane() => SpecTestRunner.Run("simd/simd_load16_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load32_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load32_lane() => SpecTestRunner.Run("simd/simd_load32_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load64_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load64_lane() => SpecTestRunner.Run("simd/simd_load64_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load8_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load8_lane() => SpecTestRunner.Run("simd/simd_load8_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load_extend tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load_extend() => SpecTestRunner.Run("simd/simd_load_extend", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load_splat tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load_splat() => SpecTestRunner.Run("simd/simd_load_splat", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_load_zero tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_load_zero() => SpecTestRunner.Run("simd/simd_load_zero", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_memory-multi tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_memory_multi() => SpecTestRunner.Run("simd/simd_memory-multi", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_select tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_select() => SpecTestRunner.Run("simd/simd_select", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_splat tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_splat() => SpecTestRunner.Run("simd/simd_splat", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_store tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_store() => SpecTestRunner.Run("simd/simd_store", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_store16_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_store16_lane() => SpecTestRunner.Run("simd/simd_store16_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_store32_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_store32_lane() => SpecTestRunner.Run("simd/simd_store32_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_store64_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_store64_lane() => SpecTestRunner.Run("simd/simd_store64_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the simd/simd_store8_lane tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_simd_store8_lane() => SpecTestRunner.Run("simd/simd_store8_lane", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the return_call tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_return_call() => SpecTestRunner.Run("return_call", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the return_call_indirect tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_return_call_indirect() => SpecTestRunner.Run("return_call_indirect", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the return_call_ref tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_return_call_ref() => SpecTestRunner.Run("return_call_ref", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/address0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_address0() => SpecTestRunner.Run("multi-memory/address0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/address1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_address1() => SpecTestRunner.Run("multi-memory/address1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/align0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_align0() => SpecTestRunner.Run("multi-memory/align0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/binary0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_binary0() => SpecTestRunner.Run("multi-memory/binary0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/data0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_data0() => SpecTestRunner.Run("multi-memory/data0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/data1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_data1() => SpecTestRunner.Run("multi-memory/data1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/data_drop0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_data_drop0() => SpecTestRunner.Run("multi-memory/data_drop0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/exports0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_exports0() => SpecTestRunner.Run("multi-memory/exports0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/float_exprs0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_float_exprs0() => SpecTestRunner.Run("multi-memory/float_exprs0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/float_exprs1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_float_exprs1() => SpecTestRunner.Run("multi-memory/float_exprs1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/float_memory0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_float_memory0() => SpecTestRunner.Run("multi-memory/float_memory0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/imports0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_imports0() => SpecTestRunner.Run("multi-memory/imports0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/imports1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_imports1() => SpecTestRunner.Run("multi-memory/imports1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/imports2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_imports2() => SpecTestRunner.Run("multi-memory/imports2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/imports3 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_imports3() => SpecTestRunner.Run("multi-memory/imports3", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/imports4 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_imports4() => SpecTestRunner.Run("multi-memory/imports4", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/linking0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_linking0() => SpecTestRunner.Run("multi-memory/linking0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/linking1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_linking1() => SpecTestRunner.Run("multi-memory/linking1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/linking2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_linking2() => SpecTestRunner.Run("multi-memory/linking2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/linking3 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_linking3() => SpecTestRunner.Run("multi-memory/linking3", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/load0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_load0() => SpecTestRunner.Run("multi-memory/load0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/load1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_load1() => SpecTestRunner.Run("multi-memory/load1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/load2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_load2() => SpecTestRunner.Run("multi-memory/load2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_copy0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_copy0() => SpecTestRunner.Run("multi-memory/memory_copy0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_copy1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_copy1() => SpecTestRunner.Run("multi-memory/memory_copy1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_fill0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_fill0() => SpecTestRunner.Run("multi-memory/memory_fill0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_grow tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_grow() => SpecTestRunner.Run("multi-memory/memory_grow", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_init0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_init0() => SpecTestRunner.Run("multi-memory/memory_init0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory-multi tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_multi() => SpecTestRunner.Run("multi-memory/memory-multi", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_size0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_size0() => SpecTestRunner.Run("multi-memory/memory_size0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_size1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_size1() => SpecTestRunner.Run("multi-memory/memory_size1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_size2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_size2() => SpecTestRunner.Run("multi-memory/memory_size2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_size3 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_size3() => SpecTestRunner.Run("multi-memory/memory_size3", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_size_import tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_size_import() => SpecTestRunner.Run("multi-memory/memory_size_import", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_trap0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_trap0() => SpecTestRunner.Run("multi-memory/memory_trap0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/memory_trap1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_memory_trap1() => SpecTestRunner.Run("multi-memory/memory_trap1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/start0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_start0() => SpecTestRunner.Run("multi-memory/start0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/store0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_store0() => SpecTestRunner.Run("multi-memory/store0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/store1 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_store1() => SpecTestRunner.Run("multi-memory/store1", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/store2 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_store2() => SpecTestRunner.Run("multi-memory/store2", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the multi-memory/traps0 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_multimemory_traps0() => SpecTestRunner.Run("multi-memory/traps0", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/address64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_address64() => SpecTestRunner.Run("memory64/address64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/align64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_align64() => SpecTestRunner.Run("memory64/align64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/binary_leb128_64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_binary_leb128_64() => SpecTestRunner.Run("memory64/binary_leb128_64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/bulk64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_bulk64() => SpecTestRunner.Run("memory64/bulk64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/call_indirect64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_call_indirect64() => SpecTestRunner.Run("memory64/call_indirect64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/endianness64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_endianness64() => SpecTestRunner.Run("memory64/endianness64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/float_memory64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_float_memory64() => SpecTestRunner.Run("memory64/float_memory64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/load64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_load64() => SpecTestRunner.Run("memory64/load64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory64() => SpecTestRunner.Run("memory64/memory64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory64-imports tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory64_imports() => SpecTestRunner.Run("memory64/memory64-imports", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_copy64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_copy64() => SpecTestRunner.Run("memory64/memory_copy64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_fill64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_fill64() => SpecTestRunner.Run("memory64/memory_fill64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_grow64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_grow64() => SpecTestRunner.Run("memory64/memory_grow64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_init64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_init64() => SpecTestRunner.Run("memory64/memory_init64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_redundancy64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_redundancy64() => SpecTestRunner.Run("memory64/memory_redundancy64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/memory_trap64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_memory_trap64() => SpecTestRunner.Run("memory64/memory_trap64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table64() => SpecTestRunner.Run("memory64/table64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_copy64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_copy64() => SpecTestRunner.Run("memory64/table_copy64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_copy_mixed tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_copy_mixed() => SpecTestRunner.Run("memory64/table_copy_mixed", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_fill64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_fill64() => SpecTestRunner.Run("memory64/table_fill64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_get64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_get64() => SpecTestRunner.Run("memory64/table_get64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_grow64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_grow64() => SpecTestRunner.Run("memory64/table_grow64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_init64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_init64() => SpecTestRunner.Run("memory64/table_init64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_set64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_set64() => SpecTestRunner.Run("memory64/table_set64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the memory64/table_size64 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_memory64_table_size64() => SpecTestRunner.Run("memory64/table_size64", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the br_on_non_null tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_br_on_non_null() => SpecTestRunner.Run("br_on_non_null", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the br_on_null tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_br_on_null() => SpecTestRunner.Run("br_on_null", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the call_ref tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_call_ref() => SpecTestRunner.Run("call_ref", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the local_init tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_local_init() => SpecTestRunner.Run("local_init", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the ref tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_ref() => SpecTestRunner.Run("ref", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the ref_as_non_null tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_ref_as_non_null() => SpecTestRunner.Run("ref_as_non_null", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the exceptions/tag tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_exceptions_tag() => SpecTestRunner.Run("exceptions/tag", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the exceptions/throw tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_exceptions_throw() => SpecTestRunner.Run("exceptions/throw", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the exceptions/throw_ref tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_exceptions_throw_ref() => SpecTestRunner.Run("exceptions/throw_ref", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the exceptions/try_table tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_exceptions_try_table() => SpecTestRunner.Run("exceptions/try_table", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/i16x8_relaxed_q15mulr_s tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_i16x8_relaxed_q15mulr_s() => SpecTestRunner.Run("relaxed-simd/i16x8_relaxed_q15mulr_s", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/i32x4_relaxed_trunc tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_i32x4_relaxed_trunc() => SpecTestRunner.Run("relaxed-simd/i32x4_relaxed_trunc", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/i8x16_relaxed_swizzle tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_i8x16_relaxed_swizzle() => SpecTestRunner.Run("relaxed-simd/i8x16_relaxed_swizzle", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/relaxed_dot_product tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_relaxed_dot_product() => SpecTestRunner.Run("relaxed-simd/relaxed_dot_product", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/relaxed_laneselect tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_relaxed_laneselect() => SpecTestRunner.Run("relaxed-simd/relaxed_laneselect", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/relaxed_madd_nmadd tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_relaxed_madd_nmadd() => SpecTestRunner.Run("relaxed-simd/relaxed_madd_nmadd", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the relaxed-simd/relaxed_min_max tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_relaxedsimd_relaxed_min_max() => SpecTestRunner.Run("relaxed-simd/relaxed_min_max", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array() => SpecTestRunner.Run("gc/array", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_copy tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_copy() => SpecTestRunner.Run("gc/array_copy", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_fill tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_fill() => SpecTestRunner.Run("gc/array_fill", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_init_data tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_init_data() => SpecTestRunner.Run("gc/array_init_data", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_init_elem tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_init_elem() => SpecTestRunner.Run("gc/array_init_elem", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_new_data tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_new_data() => SpecTestRunner.Run("gc/array_new_data", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/array_new_elem tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_array_new_elem() => SpecTestRunner.Run("gc/array_new_elem", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/binary-gc tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_binary_gc() => SpecTestRunner.Run("gc/binary-gc", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/br_on_cast tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_br_on_cast() => SpecTestRunner.Run("gc/br_on_cast", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/br_on_cast_fail tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_br_on_cast_fail() => SpecTestRunner.Run("gc/br_on_cast_fail", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/extern tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_extern() => SpecTestRunner.Run("gc/extern", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/i31 tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_i31() => SpecTestRunner.Run("gc/i31", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/ref_cast tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_ref_cast() => SpecTestRunner.Run("gc/ref_cast", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/ref_eq tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_ref_eq() => SpecTestRunner.Run("gc/ref_eq", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/ref_test tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_ref_test() => SpecTestRunner.Run("gc/ref_test", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/struct tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_struct() => SpecTestRunner.Run("gc/struct", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the gc/type-subtyping tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_gc_type_subtyping() => SpecTestRunner.Run("gc/type-subtyping", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the type-canon tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_type_canon() => SpecTestRunner.Run("type-canon", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the type-equivalence tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_type_equivalence() => SpecTestRunner.Run("type-equivalence", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the type-rec tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_type_rec() => SpecTestRunner.Run("type-rec", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the annotations tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_annotations() => SpecTestRunner.Run("annotations", SpecTestRunner.SpecExecutionMode.Persisted);

    /// <summary>Runs the instance tests via the persisted path.</summary>
    [TestMethod]
    public void PersistedSpecTest_instance() => SpecTestRunner.Run("instance", SpecTestRunner.SpecExecutionMode.Persisted);

}
#endif
