using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Int32LessThanUnsigned"/> instruction.
    /// </summary>
    [TestClass]
    public class Int32LessThanUnsignedTests
    {
        /// <summary>
        /// Tests compilation and execution of the <see cref="Int32LessThanUnsigned"/> instruction.
        /// </summary>
        [TestMethod]
        public void Int32LessThanUnsigned_Compiled()
        {
            var exports = ComparisonTestBase<int>.CreateInstance(
                new LocalGet(0),
                new LocalGet(1),
                new Int32LessThanUnsigned(),
                new End());

            var values = new uint[]
            {
                0,
                1,
                0x00,
                0x0F,
                0xF0,
                0xFF,
                byte.MaxValue,
                ushort.MaxValue,
                int.MaxValue,
                uint.MaxValue,
            };

            foreach (var comparand in values)
            {
                foreach (var value in values)
                    Assert.AreEqual(comparand < value, exports.Test((int)comparand, (int)value) != 0);

                foreach (var value in values)
                    Assert.AreEqual(value < comparand, exports.Test((int)value, (int)comparand) != 0);
            }
        }
    }
}