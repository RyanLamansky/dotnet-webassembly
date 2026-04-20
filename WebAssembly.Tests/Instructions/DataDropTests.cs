using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="DataDrop"/> instruction.
/// </summary>
[TestClass]
public class DataDropTests
{
    /// <summary>Export that drops a segment then tries to init from it.</summary>
    public abstract class DataDropExport
    {
        /// <summary>Drops segment 0.</summary>
        public abstract void Drop();

        /// <summary>Tries to memory.init from segment 0; expected to trap after drop.</summary>
        public abstract void Init(int dst, int srcOffset, int len);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));

        var seg = new Data { Kind = 1 };
        seg.RawData.Add(42);
        module.Data.Add(seg);

        // Type 0: () → void  (Drop)
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [] });
        // Type 1: (i32, i32, i32) → void  (Init)
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32, WebAssemblyValueType.Int32, WebAssemblyValueType.Int32],
            Returns = [],
        });

        module.Functions.Add(new Function { Type = 0 });
        module.Functions.Add(new Function { Type = 1 });

        module.Exports.Add(new Export { Name = nameof(DataDropExport.Drop), Index = 0 });
        module.Exports.Add(new Export { Name = nameof(DataDropExport.Init), Index = 1 });

        module.Codes.Add(new FunctionBody
        {
            Code = [new DataDrop { SegmentIndex = 0 }, new End()],
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new LocalGet(1),
                new LocalGet(2),
                new MemoryInit { SegmentIndex = 0, MemIdx = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>
    /// Tests that data.drop compiles and runs without error.
    /// </summary>
    [TestMethod]
    public void DataDrop_Compiles_AndRuns()
    {
        var compiled = BuildModule().ToInstance<DataDropExport>();
        compiled.Exports.Drop(); // should not throw
    }

    /// <summary>
    /// Tests that memory.init after data.drop traps with MemoryAccessOutOfRangeException.
    /// </summary>
    [TestMethod]
    public void DataDrop_MemoryInitAfterDrop_Traps()
    {
        var compiled = BuildModule().ToInstance<DataDropExport>();
        compiled.Exports.Drop();
        Assert.ThrowsException<WebAssembly.Runtime.MemoryAccessOutOfRangeException>(
            () => compiled.Exports.Init(0, 0, 1));
    }
}
