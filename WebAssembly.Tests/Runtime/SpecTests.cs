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
        public void SpecTest_binary()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary"), "binary.json", line => line == 723);
        }

        /// <summary>
        /// Runs the binary-leb128 tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_binary_leb128()
        {
            var skips = new HashSet<uint>
            {
                32, // ModuleLoadException: At offset 22: Operation is not valid due to the current state of the object.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "binary-leb128"), "binary-leb128.json", skips.Contains);
        }

        /// <summary>
        /// Runs the block tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_block()
        {
            // Most commmon(/exlusive?) issue: should have thrown an exception but did not.
            var skips = new HashSet<uint>
            {
                373,
                382,
                391,
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "block"), "block.json",
                line =>
                line < 306 ||
                (line >= 597 && line <= 861) || // This is imprecise due to so many failing.
                skips.Contains(line)
                );
        }

        /// <summary>
        /// Runs the br tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_br()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br"), "br.json",
                line =>
                line == 3 ||
                (line >= 334 && line <= 417) || // has no method source.
                (line >= 420 && line <= 433) || // should have thrown an exception but did not.
                (line >= 446 && line <= 455) // should have thrown an exception but did not.
            );
        }

        /// <summary>
        /// Runs the br_if tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_br_if()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br_if"), "br_if.json",
                line => line == 3 || // Stack empty.
                (line >= 372 && line <= 478) || // has no method source.
                (line >= 515 && line <= 521) || // should have thrown an exception but did not.
                (line >= 540 && line <= 560) || // should have thrown an exception but did not.
                (line >= 587 && line <= 593) || // should have thrown an exception but did not.
                line == 618 // should have thrown an exception but did not.
            );
        }

        /// <summary>
        /// Runs the br_table tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_br_table()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "br_table"), "br_table.json",
                line => line == 3 || // StackTooSmallException
                (line >= 1247 && line <= 1426) || // has no method source.
                line == 1429 || // should have thrown an exception but did not.
                line == 1443 || // should have thrown an exception but did not.
                line == 1457 || // should have thrown an exception but did not.
                (line >= 1481 && line <= 1487) || // should have thrown an exception but did not.
                line == 1502 || // should have thrown an exception but did not.
                line == 1521 // should have thrown an exception but did not.
            );
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
                272, // Infinite loop
                273, // Infinite loop
                289, // IndexOutOfRangeException (expected to fail, but a better exception needed)
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
                line == 556 || // Infinite loop
                (line >= 557 && line <= 589) || // No method source
                line >= 886 // should have thrown an exception but did not.
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
            var skips = new HashSet<uint> { 88, 89, 93, 133, 134, 139, 183, 187, 229, 234, 236 };
            if (!Environment.Is64BitProcess) // 32-bit JIT operates differently as of .NET Core 3.1.
                skips.UnionWith(new uint[] { 454, 455, 470, 471 });
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
                78, // MemoryAccessOutOfRangeException: Attempted to access 1 bytes of memory starting at offset 65536, which would have exceeded the allocated memory.
                83, // MemoryAccessOutOfRangeException: Attempted to access 1 bytes of memory starting at offset 65536, which would have exceeded the allocated memory.
                89, // MemoryAccessOutOfRangeException: Attempted to access 1 bytes of memory starting at offset 131072, which would have exceeded the allocated memory.
                94, // Attempted to access 1 bytes of memory starting at offset 0, which would have exceeded the allocated memory.
                103, // Attempted to access 1 bytes of memory starting at offset 0, which would have exceeded the allocated memory.
                108, // Attempted to access 1 bytes of memory starting at offset 65536, which would have exceeded the allocated memory.
                113, // Attempted to access 1 bytes of memory starting at offset 0, which would have exceeded the allocated memory.
                122, // Attempted to access 1 bytes of memory starting at offset 0, which would have exceeded the allocated memory.
                186, // No exception thrown. ModuleLoadException exception was expected.
                194, // No exception thrown. ModuleLoadException exception was expected.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "data"), "data.json", skips.Contains);
        }

        /// <summary>
        /// Runs the elem tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_elem()
        {
            var needTableImport = new HashSet<uint>
            { // Missing import for spectest::table. (or other table import error)
                28,
                43,
                91,
                101,
                116,
                122,
                128,
                134,
                186,
                203,
                229,
                237,
                318,
                357,
                370,
            };

            var initializerIssues = new HashSet<uint>
            { // Initializer expression support for the Element section is limited to a single Int32 constant followed by end.
                53,
                60,
            };

            var operationIssues = new HashSet<uint>
            { // Operation is not valid due to the current state of the object.
                97,
                106,
                111,
            };

            var exceptionExpected = new HashSet<uint>
            { // No exception thrown. ModuleLoadException exception was expected.
                143,
                152,
                161,
                178,
                195,
            };

            var failedLookUp = new HashSet<uint>
            { // Failed to look up method call-overwritten-element
                329,
            };

            var nullRef = new HashSet<uint>
            { // NullReferenceException
                353,
                366,
                379,
            };

            var notEqual = new HashSet<uint>
            {
                367, // Not equal: 68 and 65
                380, // Not equal: 69 and 65
                381, // Not equal: 70 and 66
            };

            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "elem"), "elem.json",
                line =>
                needTableImport.Contains(line) ||
                initializerIssues.Contains(line) ||
                operationIssues.Contains(line) ||
                exceptionExpected.Contains(line) ||
                failedLookUp.Contains(line) ||
                nullRef.Contains(line) ||
                notEqual.Contains(line)
            );
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
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "endianness"), "endianness.json", line =>
                skips.Contains(line) || (!Environment.Is64BitProcess && line >= 194)
            );
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
        public void SpecTest_fac()
        {
            var skips = new HashSet<uint>
            {
                89, // Infinite loop
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "fac"), "fac.json", skips.Contains);
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
            if (!Environment.Is64BitProcess)
            {
                skips.UnionWith(new uint[]
                {
                    141, // Not equal: 9219994337134247936 and 9222246136947933184
                    143, // Not equal: 9218888453225749180 and 9221140253039434428
                    144, // Not equal: 9219717281780008969 and 9221969081593694217
                    145, // Not equal: -3751748707474619 and -1499948893789371
                });
            }
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
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "func"), "func.json",
                line =>
                line <= 284 // WASM broken by StackSizeIncorrectException
            );
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
        [Ignore("StackSizeIncorrectException")]
        public void SpecTest_globals()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "globals"), "globals.json");
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
            var skips = new HashSet<uint> { 106 };
            if (!Environment.Is64BitProcess)
            {
                skips.UnionWith(Enumerable.Range(166, 47).Select(i => (uint)i)); // As of .NET Core 3.1, 32-bit mode can't bit shift by a 64-bit amount.
                skips.UnionWith(Enumerable.Range(242, 8).Select(i => (uint)i)); // As of .NET Core 3.1, 32-bit mode can't bit shift by a 64-bit amount.
            }

            SpecTestRunner.Run<IntegerMath<long>>(Path.Combine("Runtime", "SpecTestData", "i64"), "i64.json", skips.Contains);
        }

        /// <summary>
        /// Runs the if tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException")]
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
            var skip = new HashSet<uint>
            {
                271, // Requires table import support
                283, // Requires table import support
                284, // Requires table import support
                285, // Requires table import support
                286, // Requires table import support
                287, // Requires table import support
                290, // Requires table import support
                302, // Requires table import support
                303, // Requires table import support
                304, // Requires table import support
                305, // Requires table import support
                306, // Requires table import support
                310, // Requires table import support
                314, // Requires table import support
                318, // Requires table import support
                322, // Requires table import support
                323, // Requires table import support
                324, // Requires table import support
                325, // Requires table import support
                326, // Requires table import support
                327, // Requires table import support
                328, // Requires table import support
                329, // Requires table import support
                330, // Requires table import support
                331, // Requires table import support
                332, // Requires table import support
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "imports"), "imports.json", line => skip.Contains(line) || line >= 381);
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
        [Ignore("StackSizeIncorrectException")]
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
            HashSet<uint>? skips = null;
            if (!Environment.Is64BitProcess)
            {
                skips = new HashSet<uint>
                {
                    205, // Common Language Runtime detected an invalid program.
                    206, // Common Language Runtime detected an invalid program.
                    207, // Common Language Runtime detected an invalid program.
                };
            }
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "left-to-right"), "left-to-right.json", skips != null ? (Func<uint, bool>)skips.Contains : null);
        }

        /// <summary>
        /// Runs the linking tests.
        /// </summary>
        [TestMethod]
        [Ignore("Missing import for Mf::call.")]
        public void SpecTest_linking()
        {
            // TODO: SpecTestRunner needs to route "register" input to imports to complete this test.
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "linking"), "linking.json");
        }

        /// <summary>
        /// Runs the load tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException")]
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
            var skip = new HashSet<uint>
            {
                194, // should have thrown an exception but did not.
                203, // should have thrown an exception but did not.
                212, // should have thrown an exception but did not.
                230, // should have thrown an exception but did not.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "local_set"), "local_set.json", skip.Contains);
        }

        /// <summary>
        /// Runs the local_tee tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException")]
        public void SpecTest_local_tee()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "local_tee"), "local_tee.json");
        }

        /// <summary>
        /// Runs the loop tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException")]
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
            var skip = new HashSet<uint>
            {
                9, // No exception thrown. ModuleLoadException exception was expected.
                11, // Segment size of 0 is not currently supported.
                12, // failed to look up method memsize
                13, // Segment size of 0 is not currently supported.
                14, // failed to look up method memsize
                49, // No exception thrown. ModuleLoadException exception was expected.
                53, // No exception thrown. ModuleLoadException exception was expected.
                57, // No exception thrown. ModuleLoadException exception was expected.
                61, // No exception thrown. ModuleLoadException exception was expected.
                65, // No exception thrown. ModuleLoadException exception was expected.
                69, // No exception thrown. ModuleLoadException exception was expected.
                73, // No exception thrown. ModuleLoadException exception was expected.
                166, // Common Language Runtime detected an invalid program.
            };
            if (!Environment.Is64BitProcess)
                skip.UnionWith(Enumerable.Range(187, 26).Select(i => (uint)i)); // Common Language Runtime detected an invalid program.
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory"), "memory.json", skip.Contains);
        }

        /// <summary>
        /// Runs the memory_grow tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_memory_grow()
        {
            var skips = new HashSet<uint>
            {
                47, // Not equal: -1 and 0
                101, // StackSizeIncorrectException
            };

            skips.UnionWith(Enumerable.Range(259, (355 + 1) - 259).Select(i => (uint)i)); //Caused by 101 skip

            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "memory_grow"), "memory_grow.json", skips.Contains);
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
        [Ignore("StackTooSmallException")]
        public void SpecTest_return()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "return"), "return.json");
        }

        /// <summary>
        /// Runs the select tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackTooSmallException")]
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
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "stack"), "stack.json", line => line == 137);
        }

        /// <summary>
        /// Runs the store tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_store()
        {
            var skips = new HashSet<uint>
            {
                168, // should have thrown an exception but did not.
                178, // should have thrown an exception but did not.
                188, // should have thrown an exception but did not.
                198, // should have thrown an exception but did not.
                248, // should have thrown an exception but did not.
                258, // should have thrown an exception but did not.
                268, // should have thrown an exception but did not.
                278, // should have thrown an exception but did not.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "store"), "store.json", skips.Contains);
        }

        /// <summary>
        /// Runs the switch tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_switch()
        {
            var skips = new HashSet<uint>
            {
                130, // Common Language Runtime detected an invalid program.
                131, // Common Language Runtime detected an invalid program.
                132, // Common Language Runtime detected an invalid program.
                133, // Common Language Runtime detected an invalid program.
                134, // Common Language Runtime detected an invalid program.
                135, // Common Language Runtime detected an invalid program.
                136, // Common Language Runtime detected an invalid program.
                138, // JIT Compiler encountered an internal limitation.
                139, // JIT Compiler encountered an internal limitation.
                140, // JIT Compiler encountered an internal limitation.
                141, // JIT Compiler encountered an internal limitation.
                142, // JIT Compiler encountered an internal limitation.
                143, // JIT Compiler encountered an internal limitation.
                144, // JIT Compiler encountered an internal limitation.
                145, // JIT Compiler encountered an internal limitation.
                146, // JIT Compiler encountered an internal limitation.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "switch"), "switch.json", skips.Contains);
        }

        /// <summary>
        /// Runs the traps tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_traps()
        {
            var skips = new HashSet<uint>
            {
                83, // threw an unexpected exception of type InvalidProgramException.
                91, // threw an unexpected exception of type InvalidProgramException.
            };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "traps"), "traps.json", skips.Contains);
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
        [Ignore("StackSizeIncorrectException")]
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
            var skips = new HashSet<uint>
            {
                490, // should have thrown an exception but did not.
                585, // should have thrown an exception but did not.
                604, // should have thrown an exception but did not.
                676, // should have thrown an exception but did not.
                690, // should have thrown an exception but did not.
            };

            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "unreached-invalid"), "unreached-invalid.json", skips.Contains);
        }

        /// <summary>
        /// Runs the unwind tests.
        /// </summary>
        [TestMethod]
        [Ignore("StackSizeIncorrectException")]
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
    }
}
