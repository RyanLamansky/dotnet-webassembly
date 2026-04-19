using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ReplaceLane"/> instruction.</summary>
[TestClass]
public class Int16x8ReplaceLaneTests
{
    /// <summary>Export for Int16x8ReplaceLane test.</summary>
    public abstract class Int16x8ReplaceLaneExport
    {
        /// <summary>Returns the first byte of the result after replacing lane 0 with 0x0102.</summary>
        public abstract int GetByte();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8ReplaceLaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Int32Constant(0x0102),  // low byte = 0x02
                new Int16x8ReplaceLane { LaneIndex = 0 },
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8ReplaceLane produces correct results.</summary>
    [TestMethod]
    public void Int16x8ReplaceLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8ReplaceLaneExport>();
        Assert.AreEqual(0x02, compiled.Exports.GetByte());
    }
}
