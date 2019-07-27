using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace WebAssembly.Runtime
{
    /// <summary>
    /// Runs the official specification's tests.
    /// </summary>
    [TestClass]
    public class SpecTests
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public abstract class FloatingPointMath<T>
        {
            public abstract T add(T x, T y);
            public abstract T sub(T x, T y);
            public abstract T mul(T x, T y);
            public abstract T div(T x, T y);
            public abstract T sqrt(T x);
            public abstract T min(T x, T y);
            public abstract T max(T x, T y);
            public abstract T ceil(T x);
            public abstract T floor(T x);
            public abstract T trunc(T x);
            public abstract T nearest(T x);
        }
#pragma warning restore

        /// <summary>
        /// Runs the f32 tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_f32()
        {
            SpecTestRunner.Run<FloatingPointMath<float>>(Path.Combine("Runtime", "SpecTestData", "f32"), "f32.json");
        }

        /// <summary>
        /// Runs the f64 tests.
        /// </summary>
        [TestMethod]
        public void SpecTest_f64()
        {
            SpecTestRunner.Run<FloatingPointMath<double>>(Path.Combine("Runtime", "SpecTestData", "f64"), "f64.json");
        }
    }
}
