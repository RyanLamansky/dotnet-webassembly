using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64TruncateFloat64Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64TruncateFloat64UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64TruncateFloat64Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64TruncateUnsignedFloat64_Compiled()
        {
            var exports = ConversionTestBase<double, long>.CreateInstance(
                new LocalGet(0),
                new Int64TruncateFloat64Unsigned(),
                new End());

            foreach (var value in new[] { 0, 1.5, -1.5, 123445678901234.0 })
                Assert.AreEqual((long)value, exports.Test(value));

            const double exceptional = 1234456789012345678901234567890.0;
            Assert.ThrowsException<System.OverflowException>(() => exports.Test(exceptional));
        }
    }
}