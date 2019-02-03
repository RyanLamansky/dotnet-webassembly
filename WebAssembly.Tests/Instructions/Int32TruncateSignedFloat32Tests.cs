using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32TruncateSignedFloat32"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32TruncateSignedFloat32Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32TruncateSignedFloat32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32TruncateSignedFloat32_Compiled()
        {
            var exports = ConversionTestBase<float, int>.CreateInstance(
                new GetLocal(0),
                new Int32TruncateSignedFloat32(),
                new End());

            foreach (var value in new[] { 0, 1.5f, -1.5f })
                Assert.AreEqual((int)value, exports.Test(value));

            const float exceptional = 123445678901234f;
            Assert.ThrowsException<System.OverflowException>(() => exports.Test(exceptional));
        }
    }
}