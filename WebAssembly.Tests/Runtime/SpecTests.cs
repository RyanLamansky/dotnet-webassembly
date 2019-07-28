using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

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
