using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Splat"/> instruction.</summary>
[TestClass]
public class Float32x4SplatTests
{
    /// <summary>Export for Float32x4Splat test.</summary>
    public abstract class Float32x4SplatExport
    {
        /// <summary>Splats a value and returns the first byte of the result.</summary>
        public abstract int GetFirstByte(float value);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Float32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float32x4SplatExport.GetFirstByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Float32x4Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float32x4Splat produces correct results.</summary>
    [TestMethod]
    public void Float32x4Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float32x4SplatExport>();
        // 1.0f in IEEE 754 little-endian = [0x00, 0x00, 0x80, 0x3F]
        var bits = BitConverter.GetBytes(1.0f);
        Assert.AreEqual((int)bits[0], compiled.Exports.GetFirstByte(1.0f));
    }
}
