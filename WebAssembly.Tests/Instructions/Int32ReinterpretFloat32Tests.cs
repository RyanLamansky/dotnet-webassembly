using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int32ReinterpretFloat32"/> instruction.
/// </summary>
[TestClass]
public class Int32ReinterpretFloat32Tests
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
    /// Tests compilation and execution of the <see cref="Int32ReinterpretFloat32"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int32ReinterpretFloat32_Compiled()
    {
        var exports = ConversionTestBase<float, int>.CreateInstance(
            new LocalGet(0),
            new Int32ReinterpretFloat32(),
            new End());

        foreach (var value in Samples.Single)
            Assert.AreEqual(new Overlap32 { Float32 = value }.Int32, exports.Test(value));
    }
}
