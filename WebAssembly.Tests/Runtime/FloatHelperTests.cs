using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the <see cref="FloatHelper"/> bit-exact float operations.
/// </summary>
[TestClass]
public class FloatHelperTests
{
    // Canonical qNaN bit patterns per the WebAssembly specification.
    // https://webassembly.github.io/spec/core/syntax/values.html#floating-point
    private const uint CanonicalFloat32NaN = 0x7FC00000u;
    private const ulong CanonicalFloat64NaN = 0x7FF8000000000000UL;

    /// <summary>
    /// Verifies that <see cref="FloatHelper.UInt32BitsToFloat"/> and <see cref="FloatHelper.FloatToUInt32Bits"/>
    /// round-trip ordinary values exactly.
    /// </summary>
    [TestMethod]
    public void FloatHelper_Float32_RoundTripsOrdinaryValues()
    {
        foreach (var value in new[] { 0f, -0f, 1f, -1f, float.MaxValue, float.MinValue, float.Epsilon, float.PositiveInfinity, float.NegativeInfinity })
        {
            var bits = FloatHelper.FloatToUInt32Bits(value);
            Assert.AreEqual(value, FloatHelper.UInt32BitsToFloat(bits));
        }

        // Well-known encodings from the IEEE 754 single-precision format.
        Assert.AreEqual(0x3F800000u, FloatHelper.FloatToUInt32Bits(1f));
        Assert.AreEqual(0x80000000u, FloatHelper.FloatToUInt32Bits(-0f));
        Assert.AreEqual(1f, FloatHelper.UInt32BitsToFloat(0x3F800000u));
    }

    /// <summary>
    /// Verifies that <see cref="FloatHelper.UInt64BitsToDouble"/> and <see cref="FloatHelper.DoubleToUInt64Bits"/>
    /// round-trip ordinary values exactly.
    /// </summary>
    [TestMethod]
    public void FloatHelper_Float64_RoundTripsOrdinaryValues()
    {
        foreach (var value in new[] { 0d, -0d, 1d, -1d, double.MaxValue, double.MinValue, double.Epsilon, double.PositiveInfinity, double.NegativeInfinity })
        {
            var bits = FloatHelper.DoubleToUInt64Bits(value);
            Assert.AreEqual(value, FloatHelper.UInt64BitsToDouble(bits));
        }

        Assert.AreEqual(0x3FF0000000000000UL, FloatHelper.DoubleToUInt64Bits(1d));
        Assert.AreEqual(0x8000000000000000UL, FloatHelper.DoubleToUInt64Bits(-0d));
        Assert.AreEqual(1d, FloatHelper.UInt64BitsToDouble(0x3FF0000000000000UL));
    }

    /// <summary>
    /// The whole point of these helpers is to bypass CLR NaN canonicalization, so an arbitrary NaN payload
    /// (here a signaling NaN) must survive a bits-&gt;value-&gt;bits round trip unchanged.
    /// </summary>
    [TestMethod]
    public void FloatHelper_Float32_PreservesNaNPayload()
    {
        // Signaling NaN: exponent all ones, payload MSB clear, some payload bit set.
        const uint signalingNaN = 0x7FA00000u;
        var value = FloatHelper.UInt32BitsToFloat(signalingNaN);
        Assert.IsTrue(float.IsNaN(value));
        Assert.AreEqual(signalingNaN, FloatHelper.FloatToUInt32Bits(value));

        // A NaN with a distinctive payload also survives intact.
        const uint arbitraryNaN = 0x7F812345u;
        Assert.AreEqual(arbitraryNaN, FloatHelper.FloatToUInt32Bits(FloatHelper.UInt32BitsToFloat(arbitraryNaN)));
    }

    /// <summary>
    /// The double-precision counterpart of <see cref="FloatHelper_Float32_PreservesNaNPayload"/>.
    /// </summary>
    [TestMethod]
    public void FloatHelper_Float64_PreservesNaNPayload()
    {
        const ulong signalingNaN = 0x7FF4000000000000UL;
        var value = FloatHelper.UInt64BitsToDouble(signalingNaN);
        Assert.IsTrue(double.IsNaN(value));
        Assert.AreEqual(signalingNaN, FloatHelper.DoubleToUInt64Bits(value));

        const ulong arbitraryNaN = 0x7FF0000123456789UL;
        Assert.AreEqual(arbitraryNaN, FloatHelper.DoubleToUInt64Bits(FloatHelper.UInt64BitsToDouble(arbitraryNaN)));
    }

    /// <summary>
    /// <see cref="FloatHelper.CanonicalizeFloat32"/> must replace any NaN with the spec's canonical qNaN
    /// while leaving every non-NaN value untouched.
    /// </summary>
    [TestMethod]
    public void FloatHelper_CanonicalizeFloat32()
    {
        // Any NaN, regardless of payload or sign, becomes the canonical qNaN.
        foreach (var nan in new[] { 0x7FA00000u, 0x7F812345u, 0xFFC00000u, CanonicalFloat32NaN })
            Assert.AreEqual(CanonicalFloat32NaN, FloatHelper.FloatToUInt32Bits(FloatHelper.CanonicalizeFloat32(FloatHelper.UInt32BitsToFloat(nan))));

        // Non-NaN values pass through unchanged, preserving even the sign of zero and infinities.
        foreach (var value in new[] { 0f, -0f, 1f, -1f, float.MaxValue, float.PositiveInfinity, float.NegativeInfinity })
            Assert.AreEqual(FloatHelper.FloatToUInt32Bits(value), FloatHelper.FloatToUInt32Bits(FloatHelper.CanonicalizeFloat32(value)));
    }

    /// <summary>
    /// The double-precision counterpart of <see cref="FloatHelper_CanonicalizeFloat32"/>.
    /// </summary>
    [TestMethod]
    public void FloatHelper_CanonicalizeFloat64()
    {
        foreach (var nan in new[] { 0x7FF4000000000000UL, 0x7FF0000123456789UL, 0xFFF8000000000000UL, CanonicalFloat64NaN })
            Assert.AreEqual(CanonicalFloat64NaN, FloatHelper.DoubleToUInt64Bits(FloatHelper.CanonicalizeFloat64(FloatHelper.UInt64BitsToDouble(nan))));

        foreach (var value in new[] { 0d, -0d, 1d, -1d, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity })
            Assert.AreEqual(FloatHelper.DoubleToUInt64Bits(value), FloatHelper.DoubleToUInt64Bits(FloatHelper.CanonicalizeFloat64(value)));
    }
}
