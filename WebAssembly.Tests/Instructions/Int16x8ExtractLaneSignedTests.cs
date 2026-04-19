using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8ExtractLaneSigned"/> instruction.</summary>
[TestClass]
public class Int16x8ExtractLaneSignedTests
{
    /// <summary>Export for Int16x8ExtractLaneSigned test.</summary>
    public abstract class Int16x8ExtractLaneSignedExport
    {
        /// <summary>Extracts a signed i16 lane as i32.</summary>
        public abstract int Extract();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8ExtractLaneSignedExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // lane 0 = 0xFF00 as little-endian bytes [0x00, 0xFF] = -256 signed
                new V128Const { Value = [0x00,0xFF,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Int16x8ExtractLaneSigned { LaneIndex = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8ExtractLaneSigned produces correct results.</summary>
    [TestMethod]
    public void Int16x8ExtractLaneSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8ExtractLaneSignedExport>();
        Assert.AreEqual(-256, compiled.Exports.Extract());
    }
}
