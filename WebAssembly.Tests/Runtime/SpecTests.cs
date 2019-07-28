using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebAssembly.Runtime
{
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
            var skips = new HashSet<uint> { 391, 395, 433, 437, 475, 479, 487, 495, 570, 574, 576, 580, 582, 586, 588, 589 };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "address"), "address.json", skips.Contains);
        }

        /// <summary>
        /// Runs the align tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_align()
        {
            Func<uint, bool> skip = line => line <= 454 || (line >= 807 && line <=811) || (line >= 828 && line <= 850);
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "align"), "align.json", skip);
        }

        /// <summary>
        /// Runs the binary tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
        public void SpecTest_binary()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary"), "binary.json");
        }

        /// <summary>
        /// Runs the binary-leb128 tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile, data size limit exceeded.")]
        public void SpecTest_binary_leb128()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary-leb128"), "binary-leb128.json");
        }

        /// <summary>
        /// Runs the block tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile, at least one issue related to End not cleaning up waste.")]
        public void SpecTest_block()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "block"), "block.json");
        }

        /// <summary>
        /// Runs the br tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
        public void SpecTest_br()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br"), "br.json");
        }

        /// <summary>
        /// Runs the br_if tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
        public void SpecTest_br_if()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br_if"), "br_if.json");
        }

        /// <summary>
        /// Runs the br_table tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
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
        [Ignore("Fails to compile, at least one issue related to End not cleaning up waste.")]
        public void SpecTest_call()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "call"), "call.json");
        }

        /// <summary>
        /// Runs the call_indirect tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
        public void SpecTest_call_indirect()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "call_indirect"), "call_indirect.json");
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
            var skips = new HashSet<uint> { 88, 89, 93, 133, 134, 139, 183, 187, 229, 234, 236 };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "conversions"), "conversions.json", skips.Contains);
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
            var skips = new HashSet<uint>
            {
                5, // 0-size data section
                27, // compiler wants a memory section
                40, // compiler wants a memory section
                50, // Missing import for spectest::global_i32
                55, // compiler wants a memory section
                61, // Missing import for spectest::global_i32
                66, // compiler wants a memory section
                78, // MemoryAccessOutOfRangeException: Attempted to access 1 bytes of memory starting at offset 65536, which would have exceeded the allocated memory.
                83, // compiler wants a memory section
                89, // MemoryAccessOutOfRangeException: Attempted to access 1 bytes of memory starting at offset 131072, which would have exceeded the allocated memory.
                94, // 0-size data section
                98, // compiler wants a memory section
                103, // 0-size data section
                108, // 0-size data section
                113, // 0-size data section
                117, // compiler wants a memory section
                122, // 0-size data section
                127, // compiler wants a memory section
                132, // compiler wants a memory section
                137, // compiler wants a memory section
                143, // compiler wants a memory section
                149, // compiler wants a memory section
                154, // compiler wants a memory section
                162, // No exception thrown. ModuleLoadException exception was expected.
                170, // No exception thrown. ModuleLoadException exception was expected.
                178, // No exception thrown. ModuleLoadException exception was expected.
                211, // No exception thrown. ModuleLoadException exception was expected.
                220, // No exception thrown. ModuleLoadException exception was expected.
                235, // No exception thrown. ModuleLoadException exception was expected.
                243, // No exception thrown. ModuleLoadException exception was expected.
                251, // No exception thrown. ModuleLoadException exception was expected.
                266, // No exception thrown. ModuleLoadException exception was expected.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "data"), "data.json", skips.Contains);
        }

        /// <summary>
        /// Runs the elem tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile")]
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
            var skips = new HashSet<uint>
            {
                168, // Common Language Runtime detected an invalid program.
                169, // Common Language Runtime detected an invalid program.
                170, // Common Language Runtime detected an invalid program.
                171, // Common Language Runtime detected an invalid program.
                178, // Common Language Runtime detected an invalid program.
                179, // Common Language Runtime detected an invalid program.
                180, // Common Language Runtime detected an invalid program.
                181, // Common Language Runtime detected an invalid program.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "endianness"), "endianness.json", skips.Contains);
        }

        /// <summary>
        /// Runs the exports tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_exports()
        {
            var skips = new HashSet<uint>
            {
                33, // Exception expected but not thrown.
                37, // Exception expected but not thrown.
                41, // Exception expected but not thrown.
                45, // Exception expected but not thrown.
                49, // Exception expected but not thrown.
                82, // Exception expected but not thrown.
                86, // Exception expected but not thrown.
                90, // Exception expected but not thrown.
                94, // Exception expected but not thrown.
                98, // Exception expected but not thrown.
                130, // Exception expected but not thrown.
                139, // Exception expected but not thrown.
                143, // Exception expected but not thrown.
                147, // Exception expected but not thrown.
                179, // Exception expected but not thrown.
                188, // Exception expected but not thrown.
                192, // Exception expected but not thrown.
                196, // Exception expected but not thrown.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "exports"), "exports.json", skips.Contains);
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
        [Ignore("Fails to compile, at least one issue related to End not cleaning up waste.")]
        public void SpecTest_fac()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "fac"), "fac.json");
        }

        /// <summary>
        /// Runs the float_exprs tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_float_exprs()
        {
            var skips = new HashSet<uint>
            {
                511, // Arithmetic operation resulted in an overflow.
                519, // Arithmetic operation resulted in an overflow.
                823, // Common Language Runtime detected an invalid program.
                824, // Common Language Runtime detected an invalid program.
                825, // Common Language Runtime detected an invalid program.
                826, // Common Language Runtime detected an invalid program.
                827, // Common Language Runtime detected an invalid program.
                828, // Common Language Runtime detected an invalid program.
                829, // Common Language Runtime detected an invalid program.
                830, // Common Language Runtime detected an invalid program.
                831, // Common Language Runtime detected an invalid program.
                929, // StackSizeIncorrectException
                1055, // StackSizeIncorrectException
                1430, // Common Language Runtime detected an invalid program.
                1431, // Common Language Runtime detected an invalid program.
                1432, // Common Language Runtime detected an invalid program.
                1433, // Common Language Runtime detected an invalid program.
                1434, // Common Language Runtime detected an invalid program.
                1581, // Common Language Runtime detected an invalid program.
                1582, // Common Language Runtime detected an invalid program.
                2349, // Not equal: 2143289344 and 2139095040
                2351, // Not equal: 2143289344 and 2139095040
                2353, // Not equal: 2143289344 and 2139095040
                2355, // Not equal: 9221120237041090560 and 9218868437227405312
                2357, // Not equal: 9221120237041090560 and 9218868437227405312
                2359, // Not equal: 9221120237041090560 and 9218868437227405312
            };

            skips.UnionWith(Enumerable.Range(973, (1004 + 1) - 973).Select(i => (uint)i)); //Caused by 929 skip
            skips.UnionWith(Enumerable.Range(1099, (1130 + 1) - 1099).Select(i => (uint)i)); //Caused by 1055 skip

            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_exprs"), "float_exprs.json", skips.Contains);
        }

        /// <summary>
        /// Runs the float_literals tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_float_literals()
        {
            var skips = new HashSet<uint>
            {
                109, // Not equal: 2141192192 and 2145386496
                111, // Not equal: 2139169605 and 2143363909
                112, // Not equal: 2142257232 and 2146451536
                113, // Not equal: -5587746 and -1393442
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_literals"), "float_literals.json", skips.Contains);
        }

        /// <summary>
        /// Runs the float_memory tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_float_memory()
        {
            var skips = new HashSet<uint>
            {
                21, // Not equal: 2141192192 and 2145386496
                40, // Common Language Runtime detected an invalid program.
                41, // Common Language Runtime detected an invalid program.
                43, // Common Language Runtime detected an invalid program.
                44, // Common Language Runtime detected an invalid program.
                46, // Common Language Runtime detected an invalid program.
                47, // Common Language Runtime detected an invalid program.
                49, // Common Language Runtime detected an invalid program.
                50, // Common Language Runtime detected an invalid program.
                52, // Common Language Runtime detected an invalid program.
                53, // Common Language Runtime detected an invalid program.
                73, // Not equal: 2141192192 and 2145386496
                92, // Common Language Runtime detected an invalid program.
                93, // Common Language Runtime detected an invalid program.
                95, // Common Language Runtime detected an invalid program.
                96, // Common Language Runtime detected an invalid program.
                98, // Common Language Runtime detected an invalid program.
                99, // Common Language Runtime detected an invalid program.
                101, // Common Language Runtime detected an invalid program.
                102, // Common Language Runtime detected an invalid program.
                104, // Common Language Runtime detected an invalid program.
                105, // Common Language Runtime detected an invalid program.
                144, // Common Language Runtime detected an invalid program.
                145, // Common Language Runtime detected an invalid program.
                147, // Common Language Runtime detected an invalid program.
                148, // Common Language Runtime detected an invalid program.
                150, // Common Language Runtime detected an invalid program.
                151, // Common Language Runtime detected an invalid program.
                153, // Common Language Runtime detected an invalid program.
                154, // Common Language Runtime detected an invalid program.
                156, // Common Language Runtime detected an invalid program.
                157, // Common Language Runtime detected an invalid program.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "float_memory"), "float_memory.json", skips.Contains);
        }

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
        [Ignore("StackSizeIncorrectException due to insufficient End logic.")]
        public void SpecTest_forward()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "forward"), "forward.json");
        }

        /// <summary>
        /// Runs the func tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException due to insufficient End logic.")]
        public void SpecTest_func()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "func"), "func.json");
        }

        /// <summary>
        /// Runs the func_ptrs tests.
        /// </summary>
        [TestMethod]
        [Ignore("Missing import for spectest::print_i32.")]
        public void SpecTest_func_ptrs()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "func_ptrs"), "func_ptrs.json");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
        public void SpecTest_i32()
        {
            SpecTestRunner.Run<IntegerMath<int>>(Path.Combine("Runtime", "SpecTestData", "i32"), "i32.json", new HashSet<uint> { 106 }.Contains);
        }

        /// <summary>
        /// Runs the i64 tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_i64()
        {
            SpecTestRunner.Run<IntegerMath<long>>(Path.Combine("Runtime", "SpecTestData", "i64"), "i64.json", new HashSet<uint> { 106 }.Contains);
        }

        /// <summary>
        /// Runs the unwind tests.
        /// </summary>
        [TestMethod]
        [Ignore("Fails to compile.")]
        public void SpecTest_unwind()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "unwind"), "unwind.json");
        }
    }
}
