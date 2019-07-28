using Microsoft.VisualStudio.TestTools.UnitTesting;
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
