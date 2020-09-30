using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64TruncateSaturateFloat32Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64TruncateSaturateFloat32SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64TruncateSaturateFloat32Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64TruncateSaturatedSignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, long>.CreateInstance(
                new LocalGet(0),
                new Int64TruncateSaturateFloat32Signed(),
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
            Assert.AreEqual(4294967296, exports.Test(4294967296f));
            Assert.AreEqual(-4294967296, exports.Test(-4294967296f));
            Assert.AreEqual(9223371487098961920, exports.Test(9223371487098961920.0f));
            Assert.AreEqual(-9223372036854775808, exports.Test(-9223372036854775808.0f));
            Assert.AreEqual(0x7fffffffffffffff, exports.Test(9223372036854775808.0f));
            Assert.AreEqual(unchecked((long)0x8000000000000000), exports.Test(-9223373136366403584.0f));
            Assert.AreEqual(0x7fffffffffffffff, exports.Test(float.PositiveInfinity));
            Assert.AreEqual(unchecked((long)0x8000000000000000), exports.Test(float.NegativeInfinity));
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