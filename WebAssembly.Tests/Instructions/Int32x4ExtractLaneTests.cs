using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ExtractLane"/> instruction.</summary>
[TestClass]
public class Int32x4ExtractLaneTests
{
    /// <summary>Export for Int32x4ExtractLane test.</summary>
    public abstract class Int32x4ExtractLaneExport
    {
        /// <summary>Extracts an i32 lane.</summary>
        public abstract int Extract();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4ExtractLaneExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // lane 1 = bytes [4..7] = [0x01,0x00,0x00,0x00] = 1
                new V128Const { Value = [0,0,0,0, 1,0,0,0, 0,0,0,0, 0,0,0,0] },
                new Int32x4ExtractLane { LaneIndex = 1 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4ExtractLane produces correct results.</summary>
    [TestMethod]
    public void Int32x4ExtractLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4ExtractLaneExport>();
        Assert.AreEqual(1, compiled.Exports.Extract());
    }
}
