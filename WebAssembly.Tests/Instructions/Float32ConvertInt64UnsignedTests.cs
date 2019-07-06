using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32ConvertInt64Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32ConvertInt64UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32ConvertInt64Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32ConvertUnsignedInt64_Compiled()
        {
            var exports = ConversionTestBase<long, float>.CreateInstance(
                new LocalGet(0),
                new Float32ConvertInt64Unsigned(),
                new End());

            foreach (var value in Samples.UInt64)
                Assert.AreEqual(value, exports.Test((long)value));
        }
    }
}