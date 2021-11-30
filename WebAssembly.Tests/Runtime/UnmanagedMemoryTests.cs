using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the <see cref="UnmanagedMemory"/> class.
/// </summary>
[TestClass]
public class UnmanagedMemoryTests
{
    /// <summary>
    /// Tests that memory can be grown from a starting point of zero.
    /// </summary>
    [TestMethod]
    public void UnmanagedMemory_GrowFromZero()
    {
        Assert.AreEqual(0u, new UnmanagedMemory(0, 1).Grow(1));
    }

    /// <summary>
    /// Tests that a disposed instance can't be revived by the Grow function.
    /// </summary>
    [TestMethod]
    public void UnamangedMemory_BlockGrowWhenDisposed()
    {
        var memory = new UnmanagedMemory(0, 1);
        memory.Dispose();
        Assert.ThrowsException<ObjectDisposedException>(() => memory.Grow(1));
    }
}
