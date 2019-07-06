using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int64TruncateFloat32Signed"/> instruction.
    /// </summary>
    [TestClass]
    public class Int64TruncateFloat32SignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int64TruncateFloat32Signed"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int64TruncateSignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, long>.CreateInstance(
                new LocalGet(0),
                new Int64TruncateFloat32Signed(),
                new End());

            foreach (var value in new[] { 0, 1.5f, -1.5f, 123445678901234f })
                Assert.AreEqual((long)value, exports.Test(value));

            const float exceptional = 1234456789012345678901234567890f;
            Assert.ThrowsException<System.OverflowException>(() => exports.Test(exceptional));
        }
    }
}