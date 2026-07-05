using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly;

/// <summary>
/// Tests the additive 64-bit surface added to <see cref="ResizableLimits"/> for WebAssembly 3.0 memory64 / table64:
/// the original <c>uint</c> <see cref="ResizableLimits.Minimum"/>/<see cref="ResizableLimits.Maximum"/> remain a
/// narrowing view over the same value as the new full-width <see cref="ResizableLimits.Minimum64"/>/
/// <see cref="ResizableLimits.Maximum64"/>, with <see cref="ResizableLimits.Is64Bit"/> selecting the address type.
/// </summary>
[TestClass]
public class ResizableLimitsTests
{
    /// <summary>The 32-bit and 64-bit views read back the same value regardless of which one was written.</summary>
    [TestMethod]
    public void ResizableLimits_UintAndUlongViews_ShareOneValue()
    {
        var limits = new ResizableLimits(3, 7);
        Assert.AreEqual(3u, limits.Minimum);
        Assert.AreEqual(7u, limits.Maximum);
        Assert.AreEqual(3UL, limits.Minimum64);
        Assert.AreEqual(7UL, (object?)limits.Maximum64);
        Assert.IsFalse(limits.Is64Bit);

        // Writing through the 64-bit view is visible (narrowed) through the 32-bit view.
        limits.Minimum64 = 10;
        limits.Maximum64 = 20;
        Assert.AreEqual(10u, limits.Minimum);
        Assert.AreEqual(20u, limits.Maximum);

        // Writing through the 32-bit view is visible through the 64-bit view.
        limits.Minimum = 11;
        limits.Maximum = 21;
        Assert.AreEqual(11UL, limits.Minimum64);
        Assert.AreEqual(21UL, (object?)limits.Maximum64);
    }

    /// <summary>The full-width view holds values beyond <see cref="uint.MaxValue"/> for a memory64 / table64 limit.</summary>
    [TestMethod]
    public void ResizableLimits_FullWidth_ExceedsUint()
    {
        const ulong big = (1UL << 40) + 5;
        var limits = new ResizableLimits(big, big + 1, is64Bit: true);

        Assert.IsTrue(limits.Is64Bit);
        Assert.AreEqual(big, limits.Minimum64);
        Assert.AreEqual(big + 1, (object?)limits.Maximum64);
    }

    /// <summary>A null maximum is preserved across both views.</summary>
    [TestMethod]
    public void ResizableLimits_NullMaximum_Preserved()
    {
        var limits = new ResizableLimits(1);
        Assert.IsNull((object?)limits.Maximum);
        Assert.IsNull((object?)limits.Maximum64);

        limits.Maximum64 = 9;
        Assert.AreEqual(9u, limits.Maximum);
        limits.Maximum = null;
        Assert.IsNull((object?)limits.Maximum64);
    }
}
