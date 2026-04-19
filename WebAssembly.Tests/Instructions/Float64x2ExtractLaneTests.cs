using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2ExtractLane"/> instruction.</summary>
[TestClass]
public class Float64x2ExtractLaneTests
{
    /// <summary>Export for Float64x2ExtractLane test.</summary>
    public abstract class Float64x2ExtractLaneExport
    {
        /// <summary>Extracts an f64 lane.</summary>
        public abstract double Extract();
    }

    private static Module BuildModule()
    {
        // 1.0 bytes: [0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x3F]
        var oneBytes = BitConverter.GetBytes(1.0);
        var v = new byte[16];
        Array.Copy(oneBytes, 0, v, 0, 8); // lane 0 = 1.0

        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Float64] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float64x2ExtractLaneExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = v },
                new Float64x2ExtractLane { LaneIndex = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float64x2ExtractLane produces correct results.</summary>
    [TestMethod]
    public void Float64x2ExtractLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float64x2ExtractLaneExport>();
        Assert.AreEqual(1.0, compiled.Exports.Extract());
    }
}
