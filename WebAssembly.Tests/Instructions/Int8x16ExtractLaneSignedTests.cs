using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16ExtractLaneSigned"/> instruction.</summary>
[TestClass]
public class Int8x16ExtractLaneSignedTests
{
    /// <summary>Export for Int8x16ExtractLaneSigned test.</summary>
    public abstract class Int8x16ExtractLaneSignedExport
    {
        /// <summary>Extracts a signed i8 lane as i32.</summary>
        public abstract int Extract();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16ExtractLaneSignedExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // v = [0xFF==-1 signed, 2,3,...,16]
                new V128Const { Value = [0xFF,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16] },
                new Int8x16ExtractLaneSigned { LaneIndex = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16ExtractLaneSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16ExtractLaneSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16ExtractLaneSignedExport>();
        Assert.AreEqual(-1, compiled.Exports.Extract());
    }
}
