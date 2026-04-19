using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4ReplaceLane"/> instruction.</summary>
[TestClass]
public class Float32x4ReplaceLaneTests
{
    /// <summary>Export for Float32x4ReplaceLane test.</summary>
    public abstract class Float32x4ReplaceLaneExport
    {
        /// <summary>Returns the first byte of lane 0 after replacing it with 1.0f.</summary>
        public abstract int GetByte();
    }

    private static Module BuildModule()
    {
        // 1.0f IEEE 754 little-endian = [0x00, 0x00, 0x80, 0x3F]
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float32x4ReplaceLaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Float32Constant(1.0f),
                new Float32x4ReplaceLane { LaneIndex = 0 },
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float32x4ReplaceLane produces correct results.</summary>
    [TestMethod]
    public void Float32x4ReplaceLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float32x4ReplaceLaneExport>();
        var expected = (int)BitConverter.GetBytes(1.0f)[0];
        Assert.AreEqual(expected, compiled.Exports.GetByte());
    }
}
