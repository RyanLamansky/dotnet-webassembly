using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float64ConvertUnsignedInt64"/> instruction.
    /// </summary>
    [TestClass]
    public class Float64ConvertUnsignedInt64Tests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Float64ConvertUnsignedInt64"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float64ConvertUnsignedInt64_Compiled()
        {
            var exports = ConversionTestBase<long, double>.CreateInstance(
                new GetLocal(0),
                new Float64ConvertUnsignedInt64(),
                new End());

            foreach (var value in Samples.UInt64)
                Assert.AreEqual(value, exports.Test((long)value));
        }
    }
}