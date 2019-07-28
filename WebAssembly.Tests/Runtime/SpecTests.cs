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
        //[Ignore("Fails to compile, at least one issue related to End not cleaning up waste.")]
        public void SpecTest_conversions()
        {
            var skips = new HashSet<uint> { 88, 89, 93, 133, 134, 139, 183, 187, 229, 234, 236 };
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "conversions"), "conversions.json", skips.Contains);
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
        /// Runs the f64 tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_f64()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f64"), "f64.json");
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
        /// Runs the f64_bitwise tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_f64_bitwise()
        {
            SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "f64_bitwise"), "f64_bitwise.json");
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
