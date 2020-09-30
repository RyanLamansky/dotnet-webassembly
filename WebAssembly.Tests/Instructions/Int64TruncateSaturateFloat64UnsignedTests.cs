using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64TruncateSaturateFloat64Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64TruncateSaturateFloat64UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64TruncateSaturateFloat64Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64TruncateSaturatedUnsignedFloat64_Compiled()
        {
            var exports = ConversionTestBase<double, long>.CreateInstance(
                new LocalGet(0),
                new Int64TruncateSaturateFloat64Unsigned(),
                new End());

            // Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0.0));
            Assert.AreEqual(0, exports.Test(-0.0));
            Assert.AreEqual(0, exports.Test(double.Epsilon));
            Assert.AreEqual(0, exports.Test(-double.Epsilon));
            Assert.AreEqual(1, exports.Test(1.0));
            Assert.AreEqual(1, exports.Test(BitConverter.Int64BitsToDouble(0x3ff199999999999a)));
            Assert.AreEqual(1, exports.Test(1.5));
            Assert.AreEqual(0xffffffff, exports.Test(4294967295));
            Assert.AreEqual(0x100000000, exports.Test(4294967296));
            Assert.AreEqual(-2048, exports.Test(18446744073709549568.0));
            Assert.AreEqual(0, exports.Test(BitConverter.Int64BitsToDouble(unchecked((long)0xbfeccccccccccccd))));
            Assert.AreEqual(0, exports.Test(BitConverter.Int64BitsToDouble(unchecked((long)0xbfefffffffffffff))));
            Assert.AreEqual(100000000, exports.Test(1e8));
            Assert.AreEqual(10000000000000000, exports.Test(1e16));
            Assert.AreEqual(-9223372036854775808, exports.Test(9223372036854775808));
            Assert.AreEqual(unchecked((long)0xffffffffffffffff), exports.Test(18446744073709551616.0));
            Assert.AreEqual(0x0000000000000000, exports.Test(-1.0));
            Assert.AreEqual(unchecked((long)0xffffffffffffffff), exports.Test(double.PositiveInfinity));
            Assert.AreEqual(0x0000000000000000, exports.Test(double.NegativeInfinity));
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