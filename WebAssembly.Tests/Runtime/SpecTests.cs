using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Runtime;

/// <summary>
/// Runs the official specification's tests.
/// </summary>
[TestClass]
public class SpecTests
{
    private static string DataPath(params string[] parts)
        => Path.Combine(["Runtime", "SpecTestData", ..parts]);

    // ==== WASM 1.0 ====

    /// <summary>Runs the address tests.</summary>
    [TestMethod]
    public void SpecTest_address() => SpecTestRunner.Run(DataPath("address"), "address.json");

    /// <summary>Runs the align tests.</summary>
    [TestMethod]
    public void SpecTest_align() => SpecTestRunner.Run(DataPath("align"), "align.json");

    /// <summary>Runs the binary leb128 tests.</summary>
    [TestMethod]
    public void SpecTest_binary_leb128() => SpecTestRunner.Run(DataPath("binary-leb128"), "binary-leb128.json");

    /// <summary>Runs the binary tests.</summary>
    [TestMethod]
    public void SpecTest_binary() => SpecTestRunner.Run(DataPath("binary"), "binary.json");

    /// <summary>Runs the block tests.</summary>
    [TestMethod]
    public void SpecTest_block()
    {
        SpecTestRunner.Run(DataPath("block"), "block.json");
    }

    /// <summary>Runs the br tests.</summary>
    [TestMethod]
    public void SpecTest_br()
    {
        SpecTestRunner.Run(DataPath("br"), "br.json");
    }

    /// <summary>Runs the br_if tests.</summary>
    [TestMethod]
    public void SpecTest_br_if()
    {
        SpecTestRunner.Run(DataPath("br_if"), "br_if.json");
    }

    /// <summary>Runs the br_table tests.</summary>
    [TestMethod]
    public void SpecTest_br_table()
    {
        SpecTestRunner.Run(DataPath("br_table"), "br_table.json");
    }

    /// <summary>Runs the call tests.</summary>
    [TestMethod]
    public void SpecTest_call()
    {
        SpecTestRunner.Run(DataPath("call"), "call.json");
    }

    /// <summary>Runs the call_indirect tests.</summary>
    [TestMethod]
    public void SpecTest_call_indirect() => SpecTestRunner.Run(DataPath("call_indirect"), "call_indirect.json");

    /// <summary>Runs the comments tests.</summary>
    [TestMethod]
    public void SpecTest_comments() => SpecTestRunner.Run(DataPath("comments"), "comments.json");

    /// <summary>Runs the const tests.</summary>
    [TestMethod]
    public void SpecTest_const() => SpecTestRunner.Run(DataPath("const"), "const.json");

    /// <summary>Runs the conversions tests.</summary>
    [TestMethod]
    public void SpecTest_conversions() => SpecTestRunner.Run(DataPath("conversions"), "conversions.json");

    /// <summary>Runs the custom tests.</summary>
    [TestMethod]
    public void SpecTest_custom() => SpecTestRunner.Run(DataPath("custom"), "custom.json");

    /// <summary>Runs the data tests.</summary>
    [TestMethod]
    public void SpecTest_data() => SpecTestRunner.Run(DataPath("data"), "data.json");

    /// <summary>Runs the elem tests.</summary>
    [TestMethod]
    public void SpecTest_elem() => SpecTestRunner.Run(DataPath("elem"), "elem.json");

    /// <summary>Runs the endianness tests.</summary>
    [TestMethod]
    public void SpecTest_endianness() => SpecTestRunner.Run(DataPath("endianness"), "endianness.json");

    /// <summary>Runs the exports tests.</summary>
    [TestMethod]
    public void SpecTest_exports() => SpecTestRunner.Run(DataPath("exports"), "exports.json");

    /// <summary>Runs the f32 tests.</summary>
    [TestMethod]
    public void SpecTest_f32() => SpecTestRunner.Run(DataPath("f32"), "f32.json");

    /// <summary>Runs the f32_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_f32_bitwise() => SpecTestRunner.Run(DataPath("f32_bitwise"), "f32_bitwise.json");

    /// <summary>Runs the f32_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_f32_cmp() => SpecTestRunner.Run(DataPath("f32_cmp"), "f32_cmp.json");

    /// <summary>Runs the f64 tests.</summary>
    [TestMethod]
    public void SpecTest_f64() => SpecTestRunner.Run(DataPath("f64"), "f64.json");

    /// <summary>Runs the f64_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_f64_bitwise() => SpecTestRunner.Run(DataPath("f64_bitwise"), "f64_bitwise.json");

    /// <summary>Runs the f64_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_f64_cmp() => SpecTestRunner.Run(DataPath("f64_cmp"), "f64_cmp.json");

    /// <summary>Runs the fac tests.</summary>
    [TestMethod]
    public void SpecTest_fac() => SpecTestRunner.Run(DataPath("fac"), "fac.json");

    /// <summary>Runs the float_exprs tests.</summary>
    [TestMethod]
    public void SpecTest_float_exprs() => SpecTestRunner.Run(DataPath("float_exprs"), "float_exprs.json");

    /// <summary>Runs the float_literals tests.</summary>
    [TestMethod]
    public void SpecTest_float_literals() => SpecTestRunner.Run(DataPath("float_literals"), "float_literals.json");

    /// <summary>Runs the float_memory tests.</summary>
    [TestMethod]
    public void SpecTest_float_memory() => SpecTestRunner.Run(DataPath("float_memory"), "float_memory.json");

    /// <summary>Runs the float_misc tests.</summary>
    [TestMethod]
    public void SpecTest_float_misc() => SpecTestRunner.Run(DataPath("float_misc"), "float_misc.json");

    /// <summary>Runs the forward tests.</summary>
    [TestMethod]
    public void SpecTest_forward() => SpecTestRunner.Run(DataPath("forward"), "forward.json");

    /// <summary>Runs the func tests.</summary>
    [TestMethod]
    public void SpecTest_func() => SpecTestRunner.Run(DataPath("func"), "func.json");

    /// <summary>Runs the func_ptrs tests.</summary>
    [TestMethod]
    public void SpecTest_func_ptrs() => SpecTestRunner.Run(DataPath("func_ptrs"), "func_ptrs.json");

    /// <summary>Runs the global tests.</summary>
    [TestMethod]
    public void SpecTest_global() => SpecTestRunner.Run(DataPath("global"), "global.json");

    /// <summary>Runs the i32 tests.</summary>
    [TestMethod]
    public void SpecTest_i32() => SpecTestRunner.Run(DataPath("i32"), "i32.json");

    /// <summary>Runs the i64 tests.</summary>
    [TestMethod]
    public void SpecTest_i64() => SpecTestRunner.Run(DataPath("i64"), "i64.json");

    /// <summary>Runs the if tests.</summary>
    [TestMethod]
    public void SpecTest_if()
    {
        SpecTestRunner.Run(DataPath("if"), "if.json");
    }

    /// <summary>Runs the imports tests.</summary>
    [TestMethod]
    public void SpecTest_imports() => SpecTestRunner.Run(DataPath("imports"), "imports.json");

    /// <summary>Runs the inline module tests.</summary>
    [TestMethod]
    public void SpecTest_inline_module() => SpecTestRunner.Run(DataPath("inline-module"), "inline-module.json");

    /// <summary>Runs the int_exprs tests.</summary>
    [TestMethod]
    public void SpecTest_int_exprs() => SpecTestRunner.Run(DataPath("int_exprs"), "int_exprs.json");

    /// <summary>Runs the int_literals tests.</summary>
    [TestMethod]
    public void SpecTest_int_literals() => SpecTestRunner.Run(DataPath("int_literals"), "int_literals.json");

    /// <summary>Runs the labels tests.</summary>
    [TestMethod]
    [Ignore("StackTooSmallException")]
    public void SpecTest_labels() => SpecTestRunner.Run(DataPath("labels"), "labels.json");

    /// <summary>Runs the left to right tests.</summary>
    [TestMethod]
    public void SpecTest_left_to_right() => SpecTestRunner.Run(DataPath("left-to-right"), "left-to-right.json");

    /// <summary>Runs the linking tests.</summary>
    [TestMethod]
    public void SpecTest_linking() => SpecTestRunner.Run(DataPath("linking"), "linking.json");

    /// <summary>Runs the load tests.</summary>
    [TestMethod]
    public void SpecTest_load() => SpecTestRunner.Run(DataPath("load"), "load.json");

    /// <summary>Runs the local_get tests.</summary>
    [TestMethod]
    public void SpecTest_local_get() => SpecTestRunner.Run(DataPath("local_get"), "local_get.json");

    /// <summary>Runs the local_set tests.</summary>
    [TestMethod]
    public void SpecTest_local_set() => SpecTestRunner.Run(DataPath("local_set"), "local_set.json");

    /// <summary>Runs the local_tee tests.</summary>
    [TestMethod]
    public void SpecTest_local_tee() => SpecTestRunner.Run(DataPath("local_tee"), "local_tee.json");

    /// <summary>Runs the loop tests.</summary>
    [TestMethod]
    public void SpecTest_loop()
    {
        SpecTestRunner.Run(DataPath("loop"), "loop.json");
    }

    /// <summary>Runs the memory tests.</summary>
    [TestMethod]
    public void SpecTest_memory() => SpecTestRunner.Run(DataPath("memory"), "memory.json");

    /// <summary>Runs the memory_grow tests.</summary>
    [TestMethod]
    public void SpecTest_memory_grow() => SpecTestRunner.Run(DataPath("memory_grow"), "memory_grow.json");

    /// <summary>Runs the memory_redundancy tests.</summary>
    [TestMethod]
    public void SpecTest_memory_redundancy() => SpecTestRunner.Run(DataPath("memory_redundancy"), "memory_redundancy.json");

    /// <summary>Runs the memory_size tests.</summary>
    [TestMethod]
    public void SpecTest_memory_size() => SpecTestRunner.Run(DataPath("memory_size"), "memory_size.json");

    /// <summary>Runs the memory_trap tests.</summary>
    [TestMethod]
    public void SpecTest_memory_trap() => SpecTestRunner.Run(DataPath("memory_trap"), "memory_trap.json");

    /// <summary>Runs the names tests.</summary>
    [TestMethod]
    public void SpecTest_names() => SpecTestRunner.Run(DataPath("names"), "names.json");

    /// <summary>Runs the nop tests.</summary>
    [TestMethod]
    public void SpecTest_nop() => SpecTestRunner.Run(DataPath("nop"), "nop.json");

    /// <summary>Runs the obsolete keywords tests.</summary>
    [TestMethod]
    public void SpecTest_obsolete_keywords() => SpecTestRunner.Run(DataPath("obsolete-keywords"), "obsolete-keywords.json");

    /// <summary>Runs the return tests.</summary>
    [TestMethod]
    public void SpecTest_return() => SpecTestRunner.Run(DataPath("return"), "return.json");

    /// <summary>Runs the select tests.</summary>
    [TestMethod]
    public void SpecTest_select() => SpecTestRunner.Run(DataPath("select"), "select.json");

    /// <summary>Runs the skip stack guard page tests.</summary>
    [TestMethod]
    [Ignore("Causes CLR malfunction.")]
    public void SpecTest_skip_stack_guard_page() => SpecTestRunner.Run(DataPath("skip-stack-guard-page"), "skip-stack-guard-page.json");

    /// <summary>Runs the stack tests.</summary>
    [TestMethod]
    public void SpecTest_stack() => SpecTestRunner.Run(DataPath("stack"), "stack.json");

    /// <summary>Runs the start tests.</summary>
    [TestMethod]
    public void SpecTest_start() => SpecTestRunner.Run(DataPath("start"), "start.json");

    /// <summary>Runs the store tests.</summary>
    [TestMethod]
    public void SpecTest_store() => SpecTestRunner.Run(DataPath("store"), "store.json");

    /// <summary>Runs the switch tests.</summary>
    [TestMethod]
    public void SpecTest_switch() => SpecTestRunner.Run(DataPath("switch"), "switch.json");

    /// <summary>Runs the table tests.</summary>
    [TestMethod]
    public void SpecTest_table() => SpecTestRunner.Run(DataPath("table"), "table.json");

    /// <summary>Runs the token tests.</summary>
    [TestMethod]
    public void SpecTest_token() => SpecTestRunner.Run(DataPath("token"), "token.json");

    /// <summary>Runs the traps tests.</summary>
    [TestMethod]
    public void SpecTest_traps() => SpecTestRunner.Run(DataPath("traps"), "traps.json");

    /// <summary>Runs the type tests.</summary>
    [TestMethod]
    public void SpecTest_type() => SpecTestRunner.Run(DataPath("type"), "type.json");

    /// <summary>Runs the unreachable tests.</summary>
    [TestMethod]
    public void SpecTest_unreachable() => SpecTestRunner.Run(DataPath("unreachable"), "unreachable.json");

    /// <summary>Runs the unreached invalid tests.</summary>
    [TestMethod]
    public void SpecTest_unreached_invalid() => SpecTestRunner.Run(DataPath("unreached-invalid"), "unreached-invalid.json");

    /// <summary>Runs the unreached valid tests.</summary>
    [TestMethod]
    public void SpecTest_unreached_valid() => SpecTestRunner.Run(DataPath("unreached-valid"), "unreached-valid.json");

    /// <summary>Runs the unwind tests.</summary>
    [TestMethod]
    [Ignore("The JIT compiler encountered invalid IL code or an internal limitation.")]
    public void SpecTest_unwind() => SpecTestRunner.Run(DataPath("unwind"), "unwind.json");

    /// <summary>Runs the utf8 custom section id tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_custom_section_id() => SpecTestRunner.Run(DataPath("utf8-custom-section-id"), "utf8-custom-section-id.json");

    /// <summary>Runs the utf8 import field tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_import_field() => SpecTestRunner.Run(DataPath("utf8-import-field"), "utf8-import-field.json");

    /// <summary>Runs the utf8 import module tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_import_module() => SpecTestRunner.Run(DataPath("utf8-import-module"), "utf8-import-module.json");

    /// <summary>Runs the utf8 invalid encoding tests.</summary>
    [TestMethod]
    public void SpecTest_utf8_invalid_encoding() => SpecTestRunner.Run(DataPath("utf8-invalid-encoding"), "utf8-invalid-encoding.json");


    // ==== WASM 2.0 — Bulk memory operations ====

    /// <summary>Runs the bulk tests.</summary>
    [TestMethod]
    public void SpecTest_bulk() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "bulk"), "bulk.json");

    /// <summary>Runs the memory_copy tests.</summary>
    [TestMethod]
    public void SpecTest_memory_copy() => SpecTestRunner.Run(DataPath("memory_copy"), "memory_copy.json");

    /// <summary>Runs the memory_fill tests.</summary>
    [TestMethod]
    public void SpecTest_memory_fill() => SpecTestRunner.Run(DataPath("memory_fill"), "memory_fill.json");

    /// <summary>Runs the memory_init tests.</summary>
    [TestMethod]
    public void SpecTest_memory_init() => SpecTestRunner.Run(DataPath("memory_init"), "memory_init.json");

    /// <summary>Runs the table_copy tests.</summary>
    [TestMethod]
    public void SpecTest_table_copy() => SpecTestRunner.Run(DataPath("table_copy"), "table_copy.json");

    /// <summary>Runs the table_fill tests.</summary>
    [TestMethod]
    public void SpecTest_table_fill() => SpecTestRunner.Run(DataPath("table_fill"), "table_fill.json");

    /// <summary>Runs the table_init tests.</summary>
    [TestMethod]
    public void SpecTest_table_init() => SpecTestRunner.Run(DataPath("table_init"), "table_init.json");


    // ==== WASM 2.0 — Reference types & multi-table ====

    /// <summary>Runs the ref_func tests.</summary>
    [TestMethod]
    public void SpecTest_ref_func() => SpecTestRunner.Run(DataPath("ref_func"), "ref_func.json");

    /// <summary>Runs the ref_is_null tests.</summary>
    [TestMethod]
    public void SpecTest_ref_is_null() => SpecTestRunner.Run(DataPath("ref_is_null"), "ref_is_null.json");

    /// <summary>Runs the ref_null tests.</summary>
    [TestMethod]
    public void SpecTest_ref_null() => SpecTestRunner.Run(DataPath("ref_null"), "ref_null.json");

    /// <summary>Runs the table sub tests.</summary>
    [TestMethod]
    public void SpecTest_table_sub() => SpecTestRunner.Run(DataPath("table-sub"), "table-sub.json");

    /// <summary>Runs the table_get tests.</summary>
    [TestMethod]
    public void SpecTest_table_get() => SpecTestRunner.Run(DataPath("table_get"), "table_get.json");

    /// <summary>Runs the table_grow tests.</summary>
    [TestMethod]
    public void SpecTest_table_grow() => SpecTestRunner.Run(DataPath("table_grow"), "table_grow.json");

    /// <summary>Runs the table_set tests.</summary>
    [TestMethod]
    public void SpecTest_table_set() => SpecTestRunner.Run(DataPath("table_set"), "table_set.json");

    /// <summary>Runs the table_size tests.</summary>
    [TestMethod]
    public void SpecTest_table_size() => SpecTestRunner.Run(DataPath("table_size"), "table_size.json");


    // ==== WASM 2.0 — SIMD (fixed-width 128-bit) ====

    /// <summary>Runs the simd_address tests.</summary>
    [TestMethod]
    public void SpecTest_simd_address() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_address"), "simd_address.json");

    /// <summary>Runs the simd_align tests.</summary>
    [TestMethod]
    public void SpecTest_simd_align() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_align"), "simd_align.json");

    /// <summary>Runs the simd_bit_shift tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bit_shift() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_bit_shift"), "simd_bit_shift.json");

    /// <summary>Runs the simd_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bitwise() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_bitwise"), "simd_bitwise.json");

    /// <summary>Runs the simd_boolean tests.</summary>
    [TestMethod]
    public void SpecTest_simd_boolean() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_boolean"), "simd_boolean.json");

    /// <summary>Runs the simd_const tests.</summary>
    [TestMethod]
    public void SpecTest_simd_const() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_const"), "simd_const.json");

    /// <summary>Runs the simd_conversions tests.</summary>
    [TestMethod]
    public void SpecTest_simd_conversions() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_conversions"), "simd_conversions.json");

    /// <summary>Runs the simd_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4"), "simd_f32x4.json");

    /// <summary>Runs the simd_f32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_arith"), "simd_f32x4_arith.json");

    /// <summary>Runs the simd_f32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_cmp"), "simd_f32x4_cmp.json");

    /// <summary>Runs the simd_f32x4_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_pmin_pmax() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_pmin_pmax"), "simd_f32x4_pmin_pmax.json");

    /// <summary>Runs the simd_f32x4_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_rounding() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f32x4_rounding"), "simd_f32x4_rounding.json");

    /// <summary>Runs the simd_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2"), "simd_f64x2.json");

    /// <summary>Runs the simd_f64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_arith"), "simd_f64x2_arith.json");

    /// <summary>Runs the simd_f64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_cmp"), "simd_f64x2_cmp.json");

    /// <summary>Runs the simd_f64x2_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_pmin_pmax() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_pmin_pmax"), "simd_f64x2_pmin_pmax.json");

    /// <summary>Runs the simd_f64x2_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_rounding() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_f64x2_rounding"), "simd_f64x2_rounding.json");

    /// <summary>Runs the simd_i16x8_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_arith"), "simd_i16x8_arith.json");

    /// <summary>Runs the simd_i16x8_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_arith2"), "simd_i16x8_arith2.json");

    /// <summary>Runs the simd_i16x8_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_cmp"), "simd_i16x8_cmp.json");

    /// <summary>Runs the simd_i16x8_extadd_pairwise_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extadd_pairwise_i8x16() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_extadd_pairwise_i8x16"), "simd_i16x8_extadd_pairwise_i8x16.json");

    /// <summary>Runs the simd_i16x8_extmul_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extmul_i8x16() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_extmul_i8x16"), "simd_i16x8_extmul_i8x16.json");

    /// <summary>Runs the simd_i16x8_q15mulr_sat_s tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_q15mulr_sat_s() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_q15mulr_sat_s"), "simd_i16x8_q15mulr_sat_s.json");

    /// <summary>Runs the simd_i16x8_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_sat_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i16x8_sat_arith"), "simd_i16x8_sat_arith.json");

    /// <summary>Runs the simd_i32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_arith"), "simd_i32x4_arith.json");

    /// <summary>Runs the simd_i32x4_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_arith2"), "simd_i32x4_arith2.json");

    /// <summary>Runs the simd_i32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_cmp"), "simd_i32x4_cmp.json");

    /// <summary>Runs the simd_i32x4_dot_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_dot_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_dot_i16x8"), "simd_i32x4_dot_i16x8.json");

    /// <summary>Runs the simd_i32x4_extadd_pairwise_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extadd_pairwise_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_extadd_pairwise_i16x8"), "simd_i32x4_extadd_pairwise_i16x8.json");

    /// <summary>Runs the simd_i32x4_extmul_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extmul_i16x8() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_extmul_i16x8"), "simd_i32x4_extmul_i16x8.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_trunc_sat_f32x4"), "simd_i32x4_trunc_sat_f32x4.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f64x2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i32x4_trunc_sat_f64x2"), "simd_i32x4_trunc_sat_f64x2.json");

    /// <summary>Runs the simd_i64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_arith"), "simd_i64x2_arith.json");

    /// <summary>Runs the simd_i64x2_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_arith2"), "simd_i64x2_arith2.json");

    /// <summary>Runs the simd_i64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_cmp"), "simd_i64x2_cmp.json");

    /// <summary>Runs the simd_i64x2_extmul_i32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_extmul_i32x4() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i64x2_extmul_i32x4"), "simd_i64x2_extmul_i32x4.json");

    /// <summary>Runs the simd_i8x16_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_arith"), "simd_i8x16_arith.json");

    /// <summary>Runs the simd_i8x16_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith2() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_arith2"), "simd_i8x16_arith2.json");

    /// <summary>Runs the simd_i8x16_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_cmp() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_cmp"), "simd_i8x16_cmp.json");

    /// <summary>Runs the simd_i8x16_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_sat_arith() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_i8x16_sat_arith"), "simd_i8x16_sat_arith.json");

    /// <summary>Runs the simd_int_to_int_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_int_to_int_extend() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_int_to_int_extend"), "simd_int_to_int_extend.json");

    /// <summary>Runs the simd_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_lane"), "simd_lane.json");

    /// <summary>Runs the simd_linking tests.</summary>
    [TestMethod]
    [Ignore("simd_linking data only in simd/ subdirectory")]
    public void SpecTest_simd_linking() => SpecTestRunner.Run(DataPath("simd", "simd_linking"), "simd_linking.json");

    /// <summary>Runs the simd_load tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load"), "simd_load.json");

    /// <summary>Runs the simd_load8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load8_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load8_lane"), "simd_load8_lane.json");

    /// <summary>Runs the simd_load16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load16_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load16_lane"), "simd_load16_lane.json");

    /// <summary>Runs the simd_load32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load32_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load32_lane"), "simd_load32_lane.json");

    /// <summary>Runs the simd_load64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load64_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load64_lane"), "simd_load64_lane.json");

    /// <summary>Runs the simd_load_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_extend() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_extend"), "simd_load_extend.json");

    /// <summary>Runs the simd_load_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_splat() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_splat"), "simd_load_splat.json");

    /// <summary>Runs the simd_load_zero tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_zero() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_load_zero"), "simd_load_zero.json");

    /// <summary>Runs the simd_select tests.</summary>
    [TestMethod]
    public void SpecTest_simd_select() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_select"), "simd_select.json");

    /// <summary>Runs the simd_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_splat() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_splat"), "simd_splat.json");

    /// <summary>Runs the simd_store tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store"), "simd_store.json");

    /// <summary>Runs the simd_store8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store8_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store8_lane"), "simd_store8_lane.json");

    /// <summary>Runs the simd_store16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store16_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store16_lane"), "simd_store16_lane.json");

    /// <summary>Runs the simd_store32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store32_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store32_lane"), "simd_store32_lane.json");

    /// <summary>Runs the simd_store64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store64_lane() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "simd_store64_lane"), "simd_store64_lane.json");
}
