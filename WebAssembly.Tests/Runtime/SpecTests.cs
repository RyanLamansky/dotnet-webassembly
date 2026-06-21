using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
    public void SpecTest_binary_leb128()
    {
        // 881: StackTypeInvalidException: Int32Load requires the top stack item to be Int32, found Int64.
        // 998: ModuleLoadException: Don't know how to parse miscellaneous opcode "128".
        var skips = new HashSet<uint>
        {
            881, 998
        };
        SpecTestRunner.Run(DataPath("binary-leb128"), "binary-leb128.json", skips.Contains);
    }

    /// <summary>Runs the binary tests.</summary>
    [TestMethod]
    public void SpecTest_binary()
    {
        // 1172: ModuleLoadException: Operation is not valid due to the current state of the object.
        // 1178: ModuleLoadException: Code section found but functionSignatures is null.
        // 1538: ModuleLoadException: Stream ended unexpectedly.
        var skips = new HashSet<uint>
        {
            1172, 1178, 1538
        };
        SpecTestRunner.Run(DataPath("binary"), "binary.json", skips.Contains);
    }

    /// <summary>Runs the block tests.</summary>
    [TestMethod]
    public void SpecTest_block() => SpecTestRunner.Run(DataPath("block"), "block.json");

    /// <summary>Runs the br tests.</summary>
    [TestMethod]
    public void SpecTest_br() => SpecTestRunner.Run(DataPath("br"), "br.json");

    /// <summary>Runs the br_if tests.</summary>
    [TestMethod]
    public void SpecTest_br_if() => SpecTestRunner.Run(DataPath("br_if"), "br_if.json");

    /// <summary>Runs the br_table tests.</summary>
    [TestMethod]
    public void SpecTest_br_table() => SpecTestRunner.Run(DataPath("br_table"), "br_table.json");

    /// <summary>Runs the call tests.</summary>
    [TestMethod]
    // Call-stack-exhaustion lines are auto-skipped by the runner (uncatchable StackOverflowException).
    public void SpecTest_call() => SpecTestRunner.Run(DataPath("call"), "call.json");

    /// <summary>Runs the call_indirect tests.</summary>
    [TestMethod]
    // Call-stack-exhaustion lines are auto-skipped by the runner (uncatchable StackOverflowException).
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
    public void SpecTest_custom()
    {
        // 1x ModuleLoadException: At offset N: WebAssemblyValueType N not recognized. (Parameter 'valueType')
        var skips = new HashSet<uint>
        {
            14
        };
        SpecTestRunner.Run(DataPath("custom"), "custom.json", skips.Contains);
    }

    /// <summary>Runs the data tests.</summary>
    [TestMethod]
    public void SpecTest_data()
    {
        // 2x AssertFailedException: "unknown global" — data offset referencing an unknown global not rejected (85, 89).
        // 2x AssertFailedException: empty-content active data segment out of bounds should trap but doesn't
        //    — the zero-length segment skips the instantiation bounds check (203, 210).
        // 4x AssertFailedException: "unknown memory 1" assert_invalid not yet rejected (307, 331, 343, 365).
        // 2x AssertFailedException: "constant expression required" — non-constant data offset not rejected (466, 492).
        // 2x AssertFailedException: "unknown global 0/1" assert_invalid not yet rejected (475, 483).
        var skips = new HashSet<uint>
        {
            85, 89, 203, 210, 307, 331, 343, 365, 466, 475, 483, 492
        };
        SpecTestRunner.Run(DataPath("data"), "data.json", skips.Contains);
    }

    /// <summary>Runs the elem tests.</summary>
    [TestMethod]
    public void SpecTest_elem()
    {
        // 2x ModuleLoadException: Element segment offset must be a single Int32 constant followed by end (120, 127).
        // 12x AssertFailedException: "out of bounds table access" assert_trap has no test procedure set up.
        // 2x AssertFailedException: "unknown global 0/1" assert_invalid has no test procedure set up (467, 475).
        // 2x ImportException: Missing import for module1::shared-table (589, 602).
        // 3x AssertFailedException: Object reference not set to an instance of an object (598, 611, 692).
        // 3x AssertFailedException: Not equal i32 (active-element value mismatch) (599, 612, 613).
        // 1x ModuleLoadException: Can't export a table without defining or importing one (646).
        // 9x KeyNotFoundException: The given key '$m' was not present in the dictionary.
        // 1x ImportException: Missing import for exporter::table (664).
        var skips = new HashSet<uint>
        {
            120, 127, 239, 248, 257, 266, 273, 281, 290, 298, 307, 315, 324, 332, 467, 475, 589, 598, 599,
            602, 611, 612, 613, 646, 653, 655, 656, 658, 659, 661, 662, 664, 668, 669, 692
        };
        SpecTestRunner.Run(DataPath("elem"), "elem.json", skips.Contains);
    }

    /// <summary>Runs the endianness tests.</summary>
    [TestMethod]
    public void SpecTest_endianness() => SpecTestRunner.Run(DataPath("endianness"), "endianness.json");

    /// <summary>Runs the exports tests.</summary>
    [TestMethod]
    public void SpecTest_exports()
    {
        // 1x ModuleLoadException: Exported table must be of index 0, found 1 — multi-table export not supported (133).
        var skips = new HashSet<uint>
        {
            133
        };
        SpecTestRunner.Run(DataPath("exports"), "exports.json", skips.Contains);
    }

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
    // The call-stack-exhaustion line is auto-skipped by the runner (uncatchable StackOverflowException).
    public void SpecTest_fac() => SpecTestRunner.Run(DataPath("fac"), "fac.json");

    /// <summary>Runs the float_exprs tests.</summary>
    [TestMethod]
    public void SpecTest_float_exprs()
    {
        // 7x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            2349, 2351, 2353, 2355, 2357, 2359, 2361
        };
        SpecTestRunner.Run(DataPath("float_exprs"), "float_exprs.json", skips.Contains);
    }

    /// <summary>Runs the float_literals tests.</summary>
    [TestMethod]
    public void SpecTest_float_literals()
    {
        // 4x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            125, 127, 128, 129
        };
        SpecTestRunner.Run(DataPath("float_literals"), "float_literals.json", skips.Contains);
    }

    /// <summary>Runs the float_memory tests.</summary>
    [TestMethod]
    public void SpecTest_float_memory()
    {
        // 2x AssertFailedException: Not equal iN: A and B (NaN payload mismatch)
        var skips = new HashSet<uint>
        {
            21, 73
        };
        SpecTestRunner.Run(DataPath("float_memory"), "float_memory.json", skips.Contains);
    }

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
    public void SpecTest_global()
    {
        // 2x AssertFailedException: Common Language Runtime detected an invalid program (202, 203).
        // 2x AssertFailedException: mutable-global validation should trap but doesn't (352, 356).
        // 1x AssertFailedException: expected ModuleLoadException not thrown (371).
        var skips = new HashSet<uint>
        {
            202, 203, 352, 356, 371
        };
        SpecTestRunner.Run(DataPath("global"), "global.json", skips.Contains);
    }

    /// <summary>Runs the i32 tests.</summary>
    [TestMethod]
    public void SpecTest_i32() => SpecTestRunner.Run(DataPath("i32"), "i32.json");

    /// <summary>Runs the i64 tests.</summary>
    [TestMethod]
    public void SpecTest_i64() => SpecTestRunner.Run(DataPath("i64"), "i64.json");

    /// <summary>Runs the if tests.</summary>
    [TestMethod]
    public void SpecTest_if() => SpecTestRunner.Run(DataPath("if"), "if.json");

    /// <summary>Runs the imports tests.</summary>
    [TestMethod]
    public void SpecTest_imports()
    {
        // 1x ModuleLoadException: Exported table must be of index 0, found 1 (3).
        // 1x AssertFailedException: 21 tried to register null as a module method source.
        // 1x ImportException: Missing import for spectest::print_i64 (26).
        // 2x AssertFailedException: no method source (cascaded module-load failure) (85, 86).
        // 7x ImportException: Missing import for test::func* (116-122).
        // 2x AssertFailedException: imported-global value mismatch — spectest globals are stubbed at 666 (251, 252).
        // 3x ImportException: Missing import for test::global-* (254-256).
        // 12x ImportException: Missing import for test::table-10-* (389-400).
        // 6x AssertFailedException: expected ImportException not thrown (436, 440, 529, 533, 563, 567).
        // 1x AssertFailedException: "multiple memories" (imported + defined) not yet rejected — validation gap (489).
        // 3x ImportException: Missing import for test::memory-2-inf (501-503).
        // 1x ImportException: Missing import for grown-memory::memory (587).
        // 1x ImportException: Missing import for grown-imported-memory::memory (594).
        // 3x KeyNotFoundException: The given key '$Mgim*' was not present in the dictionary (592, 593, 599).
        var skips = new HashSet<uint>
        {
            3, 21, 26, 85, 86, 116, 117, 118, 119, 120, 121, 122, 251, 252, 254, 255, 256, 389, 390, 391,
            392, 393, 394, 395, 396, 397, 398, 399, 400, 436, 440, 489, 501, 502, 503, 529, 533, 563, 567,
            587, 592, 593, 594, 599
        };
        SpecTestRunner.Run(DataPath("imports"), "imports.json", skips.Contains);
    }

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
    public void SpecTest_labels() => SpecTestRunner.Run(DataPath("labels"), "labels.json");

    /// <summary>Runs the left to right tests.</summary>
    [TestMethod]
    public void SpecTest_left_to_right() => SpecTestRunner.Run(DataPath("left-to-right"), "left-to-right.json");

    /// <summary>Runs the linking tests.</summary>
    [TestMethod]
    public void SpecTest_linking()
    {
        // 3x AssertFailedException: cross-module global method-lookup failure (68, 75, 81).
        // 2x AssertFailedException: expected ImportException not thrown (87, 117).
        // 2x ImportException: Missing import for Mt::tab (191, 229).
        // 3x KeyNotFoundException: The given key '$Ot' was not present (207, 213, 219).
        // 3x AssertFailedException: cross-module table-call value mismatch (209, 210, 212).
        // 6x AssertFailedException: Object reference not set to an instance of an object.
        // 1x AssertFailedException: Expected ModuleLoadException or IndexOutOfRangeException, received KeyNotFoundException (227).
        // 1x AssertFailedException: Common Language Runtime detected an invalid program (241).
        // 3x AssertFailedException: "out of bounds table access" assert_trap has no test procedure set up (244, 267, 410).
        // 3x AssertFailedException: "out of bounds memory access" assert_trap has no test procedure set up (279, 360, 398).
        // 1x ModuleLoadException: Exported table must be of index 0, found 1 (291).
        // 1x KeyNotFoundException: The given key '$Mtable_ex' was not present (295).
        // 1x ImportException: Missing import for Mtable_ex::t-func (297).
        // 3x ImportException: Missing import for Mm::mem (340, 354, 367).
        // 2x AssertFailedException: cross-module memory value mismatch (349, 350).
        // 1x KeyNotFoundException: The given key '$Om' was not present (352).
        // 8x KeyNotFoundException: The given key '$Pm' was not present (375-382).
        // 3x AssertFailedException: cross-module memory value mismatch (406, 419, 452).
        // 1x AssertFailedException: memory access exceeded allocated memory (407).
        // 1x AssertFailedException: expected ModuleLoadException not thrown (436).
        var skips = new HashSet<uint>
        {
            68, 75, 81, 87, 117, 191, 207, 209, 210, 212, 213, 215, 216, 218, 219, 227, 229, 241, 244, 267,
            275, 279, 288, 291, 295, 297, 340, 349, 350, 352, 354, 360, 367, 375, 376, 377, 378, 379, 380,
            381, 382, 398, 406, 407, 410, 419, 436, 452, 453
        };
        SpecTestRunner.Run(DataPath("linking"), "linking.json", skips.Contains);
    }

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
    public void SpecTest_loop() => SpecTestRunner.Run(DataPath("loop"), "loop.json");

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
    // The call-stack-exhaustion lines are auto-skipped by the runner (uncatchable StackOverflowException);
    // running them is what previously caused the CLR malfunction this was ignored for.
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
    public void SpecTest_unreached_valid()
    {
        // 1x StackTypeInvalidException: RefIsNull requires the top stack item to be FuncRef, found Int32 (1).
        // 4x AssertFailedException: got AssertFailedException, expected UnreachableException (42-45).
        var skips = new HashSet<uint>
        {
            1, 42, 43, 44, 45
        };
        SpecTestRunner.Run(DataPath("unreached-valid"), "unreached-valid.json", skips.Contains);
    }

    /// <summary>Runs the unwind tests.</summary>
    [TestMethod]
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
    public void SpecTest_bulk() => SpecTestRunner.Run(DataPath("bulk"), "bulk.json");

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
    public void SpecTest_simd_address() => SpecTestRunner.Run(DataPath("simd", "simd_address"), "simd_address.json");

    /// <summary>Runs the simd_align tests.</summary>
    [TestMethod]
    public void SpecTest_simd_align() => SpecTestRunner.Run(DataPath("simd", "simd_align"), "simd_align.json");

    /// <summary>Runs the simd_bit_shift tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bit_shift() => SpecTestRunner.Run(DataPath("simd", "simd_bit_shift"), "simd_bit_shift.json");

    /// <summary>Runs the simd_bitwise tests.</summary>
    [TestMethod]
    public void SpecTest_simd_bitwise() => SpecTestRunner.Run(DataPath("simd", "simd_bitwise"), "simd_bitwise.json");

    /// <summary>Runs the simd_boolean tests.</summary>
    [TestMethod]
    public void SpecTest_simd_boolean() => SpecTestRunner.Run(DataPath("simd", "simd_boolean"), "simd_boolean.json");

    /// <summary>Runs the simd_const tests.</summary>
    [TestMethod]
    public void SpecTest_simd_const() => SpecTestRunner.Run(DataPath("simd", "simd_const"), "simd_const.json");

    /// <summary>Runs the simd_conversions tests.</summary>
    [TestMethod]
    public void SpecTest_simd_conversions() => SpecTestRunner.Run(DataPath("simd", "simd_conversions"), "simd_conversions.json");

    /// <summary>Runs the simd_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4() => SpecTestRunner.Run(DataPath("simd", "simd_f32x4"), "simd_f32x4.json");

    /// <summary>Runs the simd_f32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_arith() => SpecTestRunner.Run(DataPath("simd", "simd_f32x4_arith"), "simd_f32x4_arith.json");

    /// <summary>Runs the simd_f32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_f32x4_cmp"), "simd_f32x4_cmp.json");

    /// <summary>Runs the simd_f32x4_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_pmin_pmax() => SpecTestRunner.Run(DataPath("simd", "simd_f32x4_pmin_pmax"), "simd_f32x4_pmin_pmax.json");

    /// <summary>Runs the simd_f32x4_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f32x4_rounding() => SpecTestRunner.Run(DataPath("simd", "simd_f32x4_rounding"), "simd_f32x4_rounding.json");

    /// <summary>Runs the simd_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2() => SpecTestRunner.Run(DataPath("simd", "simd_f64x2"), "simd_f64x2.json");

    /// <summary>Runs the simd_f64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_arith() => SpecTestRunner.Run(DataPath("simd", "simd_f64x2_arith"), "simd_f64x2_arith.json");

    /// <summary>Runs the simd_f64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_f64x2_cmp"), "simd_f64x2_cmp.json");

    /// <summary>Runs the simd_f64x2_pmin_pmax tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_pmin_pmax() => SpecTestRunner.Run(DataPath("simd", "simd_f64x2_pmin_pmax"), "simd_f64x2_pmin_pmax.json");

    /// <summary>Runs the simd_f64x2_rounding tests.</summary>
    [TestMethod]
    public void SpecTest_simd_f64x2_rounding() => SpecTestRunner.Run(DataPath("simd", "simd_f64x2_rounding"), "simd_f64x2_rounding.json");

    /// <summary>Runs the simd_i16x8_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_arith"), "simd_i16x8_arith.json");

    /// <summary>Runs the simd_i16x8_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_arith2() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_arith2"), "simd_i16x8_arith2.json");

    /// <summary>Runs the simd_i16x8_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_cmp"), "simd_i16x8_cmp.json");

    /// <summary>Runs the simd_i16x8_extadd_pairwise_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extadd_pairwise_i8x16() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_extadd_pairwise_i8x16"), "simd_i16x8_extadd_pairwise_i8x16.json");

    /// <summary>Runs the simd_i16x8_extmul_i8x16 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_extmul_i8x16() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_extmul_i8x16"), "simd_i16x8_extmul_i8x16.json");

    /// <summary>Runs the simd_i16x8_q15mulr_sat_s tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_q15mulr_sat_s() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_q15mulr_sat_s"), "simd_i16x8_q15mulr_sat_s.json");

    /// <summary>Runs the simd_i16x8_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i16x8_sat_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i16x8_sat_arith"), "simd_i16x8_sat_arith.json");

    /// <summary>Runs the simd_i32x4_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_arith"), "simd_i32x4_arith.json");

    /// <summary>Runs the simd_i32x4_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_arith2() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_arith2"), "simd_i32x4_arith2.json");

    /// <summary>Runs the simd_i32x4_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_cmp"), "simd_i32x4_cmp.json");

    /// <summary>Runs the simd_i32x4_dot_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_dot_i16x8() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_dot_i16x8"), "simd_i32x4_dot_i16x8.json");

    /// <summary>Runs the simd_i32x4_extadd_pairwise_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extadd_pairwise_i16x8() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_extadd_pairwise_i16x8"), "simd_i32x4_extadd_pairwise_i16x8.json");

    /// <summary>Runs the simd_i32x4_extmul_i16x8 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_extmul_i16x8() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_extmul_i16x8"), "simd_i32x4_extmul_i16x8.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f32x4() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_trunc_sat_f32x4"), "simd_i32x4_trunc_sat_f32x4.json");

    /// <summary>Runs the simd_i32x4_trunc_sat_f64x2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i32x4_trunc_sat_f64x2() => SpecTestRunner.Run(DataPath("simd", "simd_i32x4_trunc_sat_f64x2"), "simd_i32x4_trunc_sat_f64x2.json");

    /// <summary>Runs the simd_i64x2_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i64x2_arith"), "simd_i64x2_arith.json");

    /// <summary>Runs the simd_i64x2_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_arith2() => SpecTestRunner.Run(DataPath("simd", "simd_i64x2_arith2"), "simd_i64x2_arith2.json");

    /// <summary>Runs the simd_i64x2_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_i64x2_cmp"), "simd_i64x2_cmp.json");

    /// <summary>Runs the simd_i64x2_extmul_i32x4 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i64x2_extmul_i32x4() => SpecTestRunner.Run(DataPath("simd", "simd_i64x2_extmul_i32x4"), "simd_i64x2_extmul_i32x4.json");

    /// <summary>Runs the simd_i8x16_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i8x16_arith"), "simd_i8x16_arith.json");

    /// <summary>Runs the simd_i8x16_arith2 tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_arith2() => SpecTestRunner.Run(DataPath("simd", "simd_i8x16_arith2"), "simd_i8x16_arith2.json");

    /// <summary>Runs the simd_i8x16_cmp tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_cmp() => SpecTestRunner.Run(DataPath("simd", "simd_i8x16_cmp"), "simd_i8x16_cmp.json");

    /// <summary>Runs the simd_i8x16_sat_arith tests.</summary>
    [TestMethod]
    public void SpecTest_simd_i8x16_sat_arith() => SpecTestRunner.Run(DataPath("simd", "simd_i8x16_sat_arith"), "simd_i8x16_sat_arith.json");

    /// <summary>Runs the simd_int_to_int_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_int_to_int_extend() => SpecTestRunner.Run(DataPath("simd", "simd_int_to_int_extend"), "simd_int_to_int_extend.json");

    /// <summary>Runs the simd_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_lane() => SpecTestRunner.Run(DataPath("simd", "simd_lane"), "simd_lane.json");

    /// <summary>Runs the simd_linking tests.</summary>
    [TestMethod]
    public void SpecTest_simd_linking() => SpecTestRunner.Run(DataPath("simd", "simd_linking"), "simd_linking.json");

    /// <summary>Runs the simd_load tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load() => SpecTestRunner.Run(DataPath("simd", "simd_load"), "simd_load.json");

    /// <summary>Runs the simd_load16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load16_lane() => SpecTestRunner.Run(DataPath("simd", "simd_load16_lane"), "simd_load16_lane.json");

    /// <summary>Runs the simd_load32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load32_lane() => SpecTestRunner.Run(DataPath("simd", "simd_load32_lane"), "simd_load32_lane.json");

    /// <summary>Runs the simd_load64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load64_lane() => SpecTestRunner.Run(DataPath("simd", "simd_load64_lane"), "simd_load64_lane.json");

    /// <summary>Runs the simd_load8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load8_lane() => SpecTestRunner.Run(DataPath("simd", "simd_load8_lane"), "simd_load8_lane.json");

    /// <summary>Runs the simd_load_extend tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_extend() => SpecTestRunner.Run(DataPath("simd", "simd_load_extend"), "simd_load_extend.json");

    /// <summary>Runs the simd_load_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_splat() => SpecTestRunner.Run(DataPath("simd", "simd_load_splat"), "simd_load_splat.json");

    /// <summary>Runs the simd_load_zero tests.</summary>
    [TestMethod]
    public void SpecTest_simd_load_zero() => SpecTestRunner.Run(DataPath("simd", "simd_load_zero"), "simd_load_zero.json");

    /// <summary>Runs the simd_splat tests.</summary>
    [TestMethod]
    public void SpecTest_simd_splat() => SpecTestRunner.Run(DataPath("simd", "simd_splat"), "simd_splat.json");

    /// <summary>Runs the simd_store tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store() => SpecTestRunner.Run(DataPath("simd", "simd_store"), "simd_store.json");

    /// <summary>Runs the simd_store16_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store16_lane() => SpecTestRunner.Run(DataPath("simd", "simd_store16_lane"), "simd_store16_lane.json");

    /// <summary>Runs the simd_store32_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store32_lane() => SpecTestRunner.Run(DataPath("simd", "simd_store32_lane"), "simd_store32_lane.json");

    /// <summary>Runs the simd_store64_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store64_lane() => SpecTestRunner.Run(DataPath("simd", "simd_store64_lane"), "simd_store64_lane.json");

    /// <summary>Runs the simd_store8_lane tests.</summary>
    [TestMethod]
    public void SpecTest_simd_store8_lane() => SpecTestRunner.Run(DataPath("simd", "simd_store8_lane"), "simd_store8_lane.json");
}
