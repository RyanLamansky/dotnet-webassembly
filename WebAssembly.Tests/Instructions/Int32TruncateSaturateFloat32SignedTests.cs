using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32TruncateSaturateFloat32Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32TruncateSaturateFloat32SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32TruncateSaturateFloat32Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32TruncateSaturatedSignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, int>.CreateInstance(
                new LocalGet(0),
                new Int32TruncateSaturateFloat32Signed(),
                new End());

            // Test cases from https://github.com/WebAssembly/spec/blob/7526564b56c30250b66504fe795e9c1e88a938af/test/core/conversions.wast
            Assert.AreEqual(0, exports.Test(0.0f));
            Assert.AreEqual(0, exports.Test(-0.0f));
            Assert.AreEqual(0, exports.Test(float.Epsilon));
            Assert.AreEqual(0, exports.Test(-float.Epsilon));
            Assert.AreEqual(1, exports.Test(1.0f));
            Assert.AreEqual(1, exports.Test(BitConverter.Int32BitsToSingle(0x3f8ccccd)));
            Assert.AreEqual(1, exports.Test(1.5f));
            Assert.AreEqual(-1, exports.Test(-1.0f));
            Assert.AreEqual(-1, exports.Test(BitConverter.Int32BitsToSingle(unchecked((int)0xbf8ccccd))));
            Assert.AreEqual(-1, exports.Test(-1.5f));
            Assert.AreEqual(-1, exports.Test(-1.9f));
            Assert.AreEqual(-2, exports.Test(-2.0f));
            Assert.AreEqual(2147483520, exports.Test(2147483520.0f));
            Assert.AreEqual(-2147483648, exports.Test(-2147483648.0f));
            Assert.AreEqual(0x7fffffff, exports.Test(2147483648.0f));
            Assert.AreEqual(unchecked((int)0x80000000), exports.Test(-2147483904.0f));
            Assert.AreEqual(0x7fffffff, exports.Test(float.PositiveInfinity));
            Assert.AreEqual(unchecked((int)0x80000000), exports.Test(float.NegativeInfinity));
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