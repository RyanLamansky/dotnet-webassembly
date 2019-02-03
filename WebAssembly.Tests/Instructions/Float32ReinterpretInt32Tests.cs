using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Tests the <see cref="Float32ReinterpretInt32"/> instruction.
    /// </summary>
    [TestClass]
    public class Float32ReinterpretInt32Tests
    {
        [StructLayout(LayoutKind.Explicit)]
        struct Overlap32
        {
            [FieldOffset(0)]
            public int Int32;

            [FieldOffset(0)]
            public float Float32;
        }

        /// <summary>
        /// Tests compilation and execution of the <see cref="Float32ReinterpretInt32"/> instruction.
        /// </summary>
        [TestMethod]
        public void Float32ReinterpretInt32_Compiled()
        {
            var exports = ConversionTestBase<int, float>.CreateInstance(
                new GetLocal(0),
                new Float32ReinterpretInt32(),
                new End());

            foreach (var value in Samples.Int32)
                Assert.AreEqual(new Overlap32 { Int32 = value }.Float32, exports.Test(value));
        }
    }
}