using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ReplaceLane"/> instruction.</summary>
[TestClass]
public class Int32x4ReplaceLaneTests
{
    /// <summary>Export for Int32x4ReplaceLane test.</summary>
    public abstract class Int32x4ReplaceLaneExport
    {
        /// <summary>Returns the first byte of lane 0 after replacing it with 0x05060708.</summary>
        public abstract int GetByte();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4ReplaceLaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Int32Constant(0x05060708),  // low byte = 0x08
                new Int32x4ReplaceLane { LaneIndex = 0 },
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4ReplaceLane produces correct results.</summary>
    [TestMethod]
    public void Int32x4ReplaceLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4ReplaceLaneExport>();
        Assert.AreEqual(0x08, compiled.Exports.GetByte());
    }
}
