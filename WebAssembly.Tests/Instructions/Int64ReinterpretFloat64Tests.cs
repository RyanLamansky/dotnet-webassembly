using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="Int64ReinterpretFloat64"/> instruction.
/// </summary>
[TestClass]
public class Int64ReinterpretFloat64Tests
{
    [StructLayout(LayoutKind.Explicit)]
    struct Overlap64
    {
        [FieldOffset(0)]
        public long Int64;

        [FieldOffset(0)]
        public double Float64;
    }

    /// <summary>
    /// Tests compilation and execution of the <see cref="Int64ReinterpretFloat64"/> instruction.
    /// </summary>
    [TestMethod]
    public void Int64ReinterpretFloat64_Compiled()
    {
        var exports = ConversionTestBase<double, long>.CreateInstance(
            new LocalGet(0),
            new Int64ReinterpretFloat64(),
            new End());

        foreach (var value in Samples.Double)
            Assert.AreEqual(new Overlap64 { Float64 = value }.Int64, exports.Test(value));
    }
}
