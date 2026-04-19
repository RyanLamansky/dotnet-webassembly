using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="MemoryFill"/> instruction.
/// </summary>
[TestClass]
public class MemoryFillTests
{
    /// <summary>Export that fills memory and reads back a byte.</summary>
    public abstract class MemoryFillExport
    {
        /// <summary>Fills len bytes at dst with val, then returns byte at readOffset.</summary>
        public abstract int Test(int dst, int val, int len, int readOffset);
    }

    /// <summary>
    /// Tests that memory.fill writes the correct byte value.
    /// </summary>
    [TestMethod]
    public void MemoryFill_FillsBytesCorrectly()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(MemoryFillExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // memory.fill(dst=param0, val=param1, len=param2)
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryFill(),
                // load byte at param3
                new LocalGet(3),
                new Int32Load8Unsigned { Offset = 0 },
                new End(),
            ],
        });

        var compiled = module.ToInstance<MemoryFillExport>();
        // Fill 4 bytes at address 10 with value 0xAB; read byte at 10
        Assert.AreEqual(0xAB, compiled.Exports.Test(10, 0xAB, 4, 10));
        // Read byte at 13 (last filled)
        Assert.AreEqual(0xAB, compiled.Exports.Test(10, 0xAB, 4, 13));
        // Address 14 should still be 0 (not filled)
        Assert.AreEqual(0, compiled.Exports.Test(10, 0xAB, 4, 14));
    }
}
