using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4ExtractLane"/> instruction.</summary>
[TestClass]
public class Float32x4ExtractLaneTests
{
    /// <summary>Export for Float32x4ExtractLane test.</summary>
    public abstract class Float32x4ExtractLaneExport
    {
        /// <summary>Extracts an f32 lane.</summary>
        public abstract float Extract();
    }

    private static Module BuildModule()
    {
        // 1.0f bytes: [0x00,0x00,0x80,0x3F]
        var oneBytes = BitConverter.GetBytes(1.0f);
        var v = new byte[16];
        Array.Copy(oneBytes, 0, v, 0, 4); // lane 0 = 1.0f

        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Float32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float32x4ExtractLaneExport.Extract) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = v },
                new Float32x4ExtractLane { LaneIndex = 0 },
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float32x4ExtractLane produces correct results.</summary>
    [TestMethod]
    public void Float32x4ExtractLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float32x4ExtractLaneExport>();
        Assert.AreEqual(1.0f, compiled.Exports.Extract());
    }
}
