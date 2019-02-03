using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64ConvertSignedInt32"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64ConvertSignedInt32Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64ConvertSignedInt32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64ConvertSignedInt32_Compiled()
        {
            var exports = ConversionTestBase<int, double>.CreateInstance(
                new GetLocal(0),
                new Float64ConvertSignedInt32(),
                new End());

            foreach (var value in Samples.Int32)
                Assert.AreEqual(value, exports.Test(value));
        }
    }
}