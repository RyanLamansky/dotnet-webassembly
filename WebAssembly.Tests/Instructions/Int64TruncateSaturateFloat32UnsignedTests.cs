using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64TruncateSaturateFloat32Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64TruncateSaturateFloat32UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64TruncateSaturateFloat32Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64TruncateSaturatedUnsignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, long>.CreateInstance(
                new LocalGet(0),
                new Int64TruncateSaturateFloat32Unsigned(),
                new End());

            // Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0.0f));
            Assert.AreEqual(0, exports.Test(-0.0f));
            Assert.AreEqual(0, exports.Test(float.Epsilon));
            Assert.AreEqual(0, exports.Test(-float.Epsilon));
            Assert.AreEqual(1, exports.Test(1.0f));
            Assert.AreEqual(1, exports.Test(BitConverter.Int32BitsToSingle(0x3f8ccccd)));
            Assert.AreEqual(1, exports.Test(1.5f));
            Assert.AreEqual(4294967296, exports.Test(4294967296f));
            Assert.AreEqual(-1099511627776, exports.Test(18446742974197923840.0f));
            Assert.AreEqual(0, exports.Test(BitConverter.Int32BitsToSingle(unchecked((int)0xbf666666))));
            Assert.AreEqual(0, exports.Test(BitConverter.Int32BitsToSingle(unchecked((int)0xbf7fffff))));
            Assert.AreEqual(unchecked((long)0xffffffffffffffff), exports.Test(18446744073709551616.0f));
            Assert.AreEqual(0x0000000000000000, exports.Test(-1.0f));
            Assert.AreEqual(unchecked((long)0xffffffffffffffff), exports.Test(float.PositiveInfinity));
            Assert.AreEqual(0x0000000000000000, exports.Test(float.NegativeInfinity));
            Assert.AreEqual(0, exports.Test(float.NaN));
            Assert.AreEqual(0, exports.Test(AddPayload(float.NaN, 0x200000)));
            Assert.AreEqual(0, exports.Test(-float.NaN));
            Assert.AreEqual(0, exports.Test(AddPayload(-float.NaN, 0x200000)));
        }

        private static float AddPayload(float floatValue, int payload)
        {
            var floatValueAsInt = BitConverter.SingleToInt32Bits(floatValue);
            floatValueAsInt |= payload;
            return BitConverter.Int32BitsToSingle(floatValueAsInt);
        }
    }
}