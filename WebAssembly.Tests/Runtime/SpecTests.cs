using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebAssembly.Runtime;

/// <summary>
/// Runs the official specification's tests.
/// </summary>
[TestClass]
public class SpecTests
{
    /// <summary>
    /// Runs the address tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_address()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "address"), "address.json");
    }

    /// <summary>
    /// Runs the align tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_align()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "align"), "align.json");
    }

    /// <summary>
    /// Runs the binary tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_binary()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary"), "binary.json");
    }

    /// <summary>
    /// Runs the binary-leb128 tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_binary_leb128()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary-leb128"), "binary-leb128.json");
    }

    /// <summary>
    /// Runs the block tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_block()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "block"), "block.json");
    }

    /// <summary>
    /// Runs the br tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_br()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br"), "br.json");
    }

    /// <summary>
    /// Runs the br_if tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_br_if()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br_if"), "br_if.json");
    }

    /// <summary>
    /// Runs the br_table tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_br_table()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br_table"), "br_table.json");
    }

    /// <summary>
    /// Runs the break-drop tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_break_drop()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "break-drop"), "break-drop.json");
    }

    /// <summary>
    /// Runs the call tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_call()
    {
        var skips = new HashSet<uint>
            {
                // CLR JIT tail-call-optimizes self-/mutual-recursion into infinite loops;
                // EnsureSufficientExecutionStack never fires because the stack doesn't grow.
                272, // assert_exhaustion: runaway (tail-recursive — infinite loop)
                273, // assert_exhaustion: mutual-runaway (tail-recursive — infinite loop)
            };
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "call"), "call.json", skips.Contains);
    }

    /// <summary>
    /// Runs the call_indirect tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_call_indirect()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "call_indirect"), "call_indirect.json",
            line =>
            // CLR JIT tail-call-optimizes self-/mutual-recursion into infinite loops;
            // EnsureSufficientExecutionStack never fires because the stack doesn't grow.
            line == 556 || // assert_exhaustion: runaway (tail-recursive — infinite loop)
            line == 557    // assert_exhaustion: mutual-runaway (tail-recursive — infinite loop)
        );
    }

    /// <summary>
    /// Runs the const tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_const()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "const"), "const.json");
    }

    /// <summary>
    /// Runs the conversions tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_conversions()
    {
        if (!Environment.Is64BitProcess) // 32-bit JIT operates differently as of .NET Core 3.1.
        {
            var skips = new HashSet<uint> { 454, 455, 470, 471 };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "conversions"), "conversions.json", skips.Contains);
        }
        else
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "conversions"), "conversions.json");
    }

    /// <summary>
    /// Runs the custom tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_custom()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "custom"), "custom.json");
    }

    /// <summary>
    /// Runs the data tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_data()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "data"), "data.json");
    }

    /// <summary>
    /// Runs the elem tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_elem()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "elem"), "elem.json");
    }

    /// <summary>
    /// Runs the endianness tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_endianness()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "endianness"), "endianness.json");
    }

    /// <summary>
    /// Runs the exports tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_exports()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "exports"), "exports.json");
    }

    /// <summary>
    /// Runs the f32 tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f32()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f32"), "f32.json");
    }

    /// <summary>
    /// Runs the f32_bitwise tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f32_bitwise()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f32_bitwise"), "f32_bitwise.json");
    }

    /// <summary>
    /// Runs the f32_cmp tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f32_cmp()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f32_cmp"), "f32_cmp.json");
    }

    /// <summary>
    /// Runs the f64 tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f64()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f64"), "f64.json");
    }

    /// <summary>
    /// Runs the f64_bitwise tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f64_bitwise()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f64_bitwise"), "f64_bitwise.json");
    }

    /// <summary>
    /// Runs the f64_cmp tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_f64_cmp()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f64_cmp"), "f64_cmp.json");
    }

    /// <summary>
    /// Runs the fac tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_fac()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "fac"), "fac.json");
    }

    /// <summary>
    /// Runs the float_exprs tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_float_exprs() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_exprs"), "float_exprs.json");

    /// <summary>
    /// Runs the float_literals tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_float_literals() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_literals"), "float_literals.json");

    /// <summary>
    /// Runs the float_memory tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_float_memory() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_memory"), "float_memory.json");

    /// <summary>
    /// Runs the float_misc tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_float_misc()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_misc"), "float_misc.json");
    }

    /// <summary>
    /// Runs the forward tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_forward()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "forward"), "forward.json");
    }

    /// <summary>
    /// Runs the func tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_func()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "func"), "func.json");
    }

    /// <summary>
    /// Runs the func_ptrs tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_func_ptrs()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "func_ptrs"), "func_ptrs.json");
    }

    /// <summary>
    /// Runs the globals tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_globals()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "globals"), "globals.json");
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles -- Must match expectations of the target WASM.
    public abstract class IntegerMath<T>
    {
        public abstract T add(T x, T y);
        public abstract T sub(T x, T y);
        public abstract T mul(T x, T y);
        public abstract T div_s(T x, T y);
        public abstract T div_u(T x, T y);
        public abstract T rem_s(T x, T y);
        public abstract T rem_u(T x, T y);
        public abstract T and(T x, T y);
        public abstract T or(T x, T y);
        public abstract T xor(T x, T y);
        public abstract T shl(T x, T y);
        public abstract T shr_s(T x, T y);
        public abstract T shr_u(T x, T y);
        public abstract T rotl(T x, T y);
        public abstract T clz(T x);
        public abstract T ctz(T x);
        public abstract T popcnt(T x);
        public abstract int eqz(T x);
        public abstract int eq(T x, T y);
        public abstract int ne(T x, T y);
        public abstract int lt_s(T x, T y);
        public abstract int lt_u(T x, T y);
        public abstract int le_s(T x, T y);
        public abstract int le_u(T x, T y);
        public abstract int gt_s(T x, T y);
        public abstract int gt_u(T x, T y);
        public abstract int ge_s(T x, T y);
        public abstract int ge_u(T x, T y);
    }
#pragma warning restore

    /// <summary>
    /// Runs the i32 tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_i32() =>
        SpecTestRunner.Run<IntegerMath<int>>(Path.Combine("Runtime", "SpecTestData", "i32"), "i32.json");

    /// <summary>
    /// Runs the i64 tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_i64() =>
        SpecTestRunner.Run<IntegerMath<long>>(Path.Combine("Runtime", "SpecTestData", "i64"), "i64.json");

    /// <summary>
    /// Runs the if tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_if()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "if"), "if.json");
    }

    /// <summary>
    /// Runs the imports tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_imports()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "imports"), "imports.json");
    }

    /// <summary>
    /// Runs the int_exprs tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_int_exprs()
    {
        HashSet<uint>? skips = null;
        if (!Environment.Is64BitProcess)
        {
            skips = new HashSet<uint>
                {
                    58, 59, 77, 78,
                };
        }
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "int_exprs"), "int_exprs.json", skips != null ? (Func<uint, bool>)skips.Contains : null);
    }

    /// <summary>
    /// Runs the int_literals tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_int_literals()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "int_literals"), "int_literals.json");
    }

    /// <summary>
    /// Runs the labels tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_labels()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "labels"), "labels.json");
    }

    /// <summary>
    /// Runs the left-to-right tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_left_to_right()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "left-to-right"), "left-to-right.json");
    }

    /// <summary>
    /// Runs the linking tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_linking()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "linking"), "linking.json");
    }

    /// <summary>
    /// Runs the load tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_load()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "load"), "load.json");
    }

    /// <summary>
    /// Runs the local_get tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_local_get()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "local_get"), "local_get.json");
    }

    /// <summary>
    /// Runs the local_set tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_local_set()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "local_set"), "local_set.json");
    }

    /// <summary>
    /// Runs the local_tee tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_local_tee()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "local_tee"), "local_tee.json");
    }

    /// <summary>
    /// Runs the loop tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_loop()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "loop"), "loop.json");
    }

    /// <summary>
    /// Runs the memory tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_memory()
    {
        HashSet<uint>? skip = null;
        if (!Environment.Is64BitProcess)
        {
            skip = new HashSet<uint>();
            skip.UnionWith(Enumerable.Range(187, 26).Select(i => (uint)i)); // Common Language Runtime detected an invalid program.
        }
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory"), "memory.json", skip != null ? (Func<uint, bool>)skip.Contains : null);
    }

    /// <summary>
    /// Runs the memory_grow tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_memory_grow()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory_grow"), "memory_grow.json");
    }

    /// <summary>
    /// Runs the memory_redundancy tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_memory_redundancy()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory_redundancy"), "memory_redundancy.json");
    }

    /// <summary>
    /// Runs the memory_size tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_memory_size()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory_size"), "memory_size.json");
    }

    /// <summary>
    /// Runs the names tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_names()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "names"), "names.json");
    }

    /// <summary>
    /// Runs the nop tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_nop()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "nop"), "nop.json");
    }

    /// <summary>
    /// Runs the return tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_return()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "return"), "return.json");
    }

    /// <summary>
    /// Runs the select tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_select()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "select"), "select.json");
    }

    /// <summary>
    /// Runs the skip-stack-guard-page tests.
    /// </summary>
    [TestMethod]
    [Ignore("Causes CLR malfunction.")]
    public void SpecTest_skip_stack_guard_page()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "skip-stack-guard-page"), "skip-stack-guard-page.json");
    }

    /// <summary>
    /// Runs the stack tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_stack()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "stack"), "stack.json");
    }

    /// <summary>
    /// Runs the store tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_store()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "store"), "store.json");
    }

    /// <summary>
    /// Runs the switch tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_switch()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "switch"), "switch.json");
    }

    /// <summary>
    /// Runs the traps tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_traps()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "traps"), "traps.json");
    }

    /// <summary>
    /// Runs the type tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_type()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "type"), "type.json");
    }

    /// <summary>
    /// Runs the unreachable tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_unreachable()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "unreachable"), "unreachable.json");
    }

    /// <summary>
    /// Runs the unreached-invalid tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_unreached_invalid()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "unreached-invalid"), "unreached-invalid.json");
    }

    /// <summary>
    /// Runs the unwind tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_unwind()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "unwind"), "unwind.json");
    }

    /// <summary>
    /// Runs the utf8-custom-section-id tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_utf8_custom_section_id()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "utf8-custom-section-id"), "utf8-custom-section-id.json");
    }

    /// <summary>
    /// Runs the utf8-import-field tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_utf8_import_field()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "utf8-import-field"), "utf8-import-field.json");
    }

    /// <summary>
    /// Runs the utf8-import-module tests.
    /// </summary>
    [TestMethod]
    public void SpecTest_utf8_import_module()
    {
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "utf8-import-module"), "utf8-import-module.json");
    }

    /// <summary>Runs the bulk memory tests.</summary>
    [TestMethod]
    public void SpecTest_bulk() =>
        SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "bulk"), "bulk.json");

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
