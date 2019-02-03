using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32ConvertSignedInt32"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32ConvertSignedInt32Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32ConvertSignedInt32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32ConvertSignedInt32_Compiled()
        {
            var exports = ConversionTestBase<int, float>.CreateInstance(
                new GetLocal(0),
                new Float32ConvertSignedInt32(),
                new End());

            foreach (var value in Samples.Int32)
                Assert.AreEqual(value, exports.Test(value));
        }
    }
}