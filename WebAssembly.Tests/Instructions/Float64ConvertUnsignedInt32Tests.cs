using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64ConvertUnsignedInt32"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64ConvertUnsignedInt32Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64ConvertUnsignedInt32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64ConvertUnsignedInt32_Compiled()
        {
            var exports = ConversionTestBase<int, double>.CreateInstance(
                new GetLocal(0),
                new Float64ConvertUnsignedInt32(),
                new End());

            foreach (var value in Samples.UInt32)
                Assert.AreEqual(value, exports.Test((int)value));
        }
    }
}