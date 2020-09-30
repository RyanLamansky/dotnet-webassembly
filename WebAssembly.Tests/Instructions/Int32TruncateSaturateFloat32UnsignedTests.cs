using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32TruncateSaturateFloat32Unsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32TruncateSaturateFloat32UnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32TruncateSaturateFloat32Unsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32TruncateSaturatedUnsignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, int>.CreateInstance(
                new LocalGet(0),
                new Int32TruncateSaturateFloat32Unsigned(),
                new End());

            // Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0.0f));
            Assert.AreEqual(0, exports.Test(-0.0f));
            Assert.AreEqual(0, exports.Test(float.Epsilon));
            Assert.AreEqual(0, exports.Test(-float.Epsilon));
            Assert.AreEqual(1, exports.Test(1.0f));
            Assert.AreEqual(1, exports.Test(BitConverter.Int32BitsToSingle(0x3f8ccccd)));
            Assert.AreEqual(1, exports.Test(1.5f));
            Assert.AreEqual(1, exports.Test(1.9f));
            Assert.AreEqual(2, exports.Test(2.0f));
            Assert.AreEqual(-2147483648, exports.Test(2147483648f));
            Assert.AreEqual(-256, exports.Test(4294967040.0f));
            Assert.AreEqual(0, exports.Test(BitConverter.Int32BitsToSingle(unchecked((int)0xbf666666))));
            Assert.AreEqual(0, exports.Test(BitConverter.Int32BitsToSingle(unchecked((int)0xbf7fffff))));
            Assert.AreEqual(unchecked((int)0xffffffff), exports.Test(4294967296.0f));
            Assert.AreEqual(0x00000000, exports.Test(-1.0f));
            Assert.AreEqual(unchecked((int)0xffffffff), exports.Test(float.PositiveInfinity));
            Assert.AreEqual(0x00000000, exports.Test(float.NegativeInfinity));
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