using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ExtractLaneUnsigned"/> instruction.</summary>
[TestClass]
public class Int16x8ExtractLaneUnsignedTests
{
    /// <summary>Export for Int16x8ExtractLaneUnsigned test.</summary>
    public abstract class Int16x8ExtractLaneUnsignedExport
    {
        /// <summary>Extracts an unsigned i16 lane as i32.</summary>
        public abstract int Extract();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8ExtractLaneUnsignedExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // lane 0 = 0xFF00 as little-endian bytes [0x00, 0xFF] = 65280 unsigned
                new V128Const { Value = [0x00,0xFF,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Int16x8ExtractLaneUnsigned { LaneIndex = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8ExtractLaneUnsigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8ExtractLaneUnsigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8ExtractLaneUnsignedExport>();
        Assert.AreEqual(65280, compiled.Exports.Extract());
    }
}
