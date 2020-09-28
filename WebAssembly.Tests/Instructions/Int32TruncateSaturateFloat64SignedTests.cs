using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32TruncateSaturateFloat64Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32TruncateSaturateFloat64SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32TruncateSaturateFloat64Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32TruncateSaturatedSignedFloat64_Compiled()
        {
            var exports = ConversionTestBase<double, int>.CreateInstance(
                new LocalGet(0),
                new Int32TruncateSaturateFloat64Signed(),
                new End());

            // Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0.0));
            Assert.AreEqual(0, exports.Test(-0.0));
            Assert.AreEqual(0, exports.Test(double.Epsilon));
            Assert.AreEqual(0, exports.Test(-double.Epsilon));
            Assert.AreEqual(1, exports.Test(1.0));
            Assert.AreEqual(1, exports.Test(BitConverter.Int64BitsToDouble(0x3ff199999999999a)));
            Assert.AreEqual(1, exports.Test(1.5));
            Assert.AreEqual(-1, exports.Test(-1.0));
            Assert.AreEqual(-1, exports.Test(BitConverter.Int64BitsToDouble(unchecked((long)0xbff199999999999a))));
            Assert.AreEqual(-1, exports.Test(-1.5));
            Assert.AreEqual(-1, exports.Test(-1.9));
            Assert.AreEqual(-2, exports.Test(-2.0));
            Assert.AreEqual(2147483647, exports.Test(2147483647.0));
            Assert.AreEqual(-2147483648, exports.Test(-2147483648.0));
            Assert.AreEqual(0x7fffffff, exports.Test(2147483648.0));
            Assert.AreEqual(unchecked((int)0x80000000), exports.Test(-2147483649.0));
            Assert.AreEqual(0x7fffffff, exports.Test(double.PositiveInfinity));
            Assert.AreEqual(unchecked((int)0x80000000), exports.Test(double.NegativeInfinity));
            Assert.AreEqual(0, exports.Test(double.NaN));
            Assert.AreEqual(0, exports.Test(AddPayload(double.NaN, 0x4000000000000)));
            Assert.AreEqual(0, exports.Test(-double.NaN));
            Assert.AreEqual(0, exports.Test(AddPayload(-double.NaN, 0x4000000000000)));
        }

        private static double AddPayload(double doubleValue, long payload)
        {
            var doubleValueAsInt = BitConverter.DoubleToInt64Bits(doubleValue);
            doubleValueAsInt |= payload;
            return BitConverter.Int64BitsToDouble(doubleValueAsInt);
        }
    }
}