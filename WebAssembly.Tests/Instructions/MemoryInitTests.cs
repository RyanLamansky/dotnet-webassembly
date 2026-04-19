using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemoryInit"/> instruction.
/// </summary>
[TestClass]
public class MemoryInitTests
{
    /// <summary>Export that uses memory.init and reads a byte back.</summary>
    public abstract class MemoryInitExport
    {
        /// <summary>Copies len bytes from segment at srcOffset to memory at dst; returns byte at readOffset.</summary>
        public abstract int Test(int dst, int srcOffset, int len, int readOffset);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));

        // Passive data segment (kind 1) containing bytes [10, 20, 30, 40]
        var seg = new Data { Kind = 1 };
        seg.RawData.Add(10);
        seg.RawData.Add(20);
        seg.RawData.Add(30);
        seg.RawData.Add(40);
        module.Data.Add(seg);

        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryInitExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0), // dst
                new LocalGet(1), // srcOffset
                new LocalGet(2), // len
                new MemoryInit { SegmentIndex = 0, MemIdx = 0 },
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>
    /// Tests that memory.init copies bytes from a passive segment into memory.
    /// </summary>
    [TestMethod]
    public void MemoryInit_CopiesBytesFromPassiveSegment()
    {
        var compiled = BuildModule().ToInstance<MemoryInitExport>();

        // Copy all 4 bytes starting at segment offset 0 into memory at address 100
        Assert.AreEqual(10, compiled.Exports.Test(100, 0, 4, 100));
        Assert.AreEqual(20, compiled.Exports.Test(100, 0, 4, 101));
        Assert.AreEqual(30, compiled.Exports.Test(100, 0, 4, 102));
        Assert.AreEqual(40, compiled.Exports.Test(100, 0, 4, 103));
    }

    /// <summary>
    /// Tests that memory.init with a srcOffset copies the correct sub-range.
    /// </summary>
    [TestMethod]
    public void MemoryInit_RespectsSourceOffset()
    {
        var compiled = BuildModule().ToInstance<MemoryInitExport>();

        // Copy 2 bytes starting at segment offset 1 ([20, 30]) into memory at 200
        Assert.AreEqual(20, compiled.Exports.Test(200, 1, 2, 200));
        Assert.AreEqual(30, compiled.Exports.Test(200, 1, 2, 201));
    }
}
