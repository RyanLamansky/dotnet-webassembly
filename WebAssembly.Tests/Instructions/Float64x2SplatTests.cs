using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2Splat"/> instruction.</summary>
[TestClass]
public class Float64x2SplatTests
{
    /// <summary>Export for Float64x2Splat test.</summary>
    public abstract class Float64x2SplatExport
    {
        /// <summary>Splats a value and returns the first byte of the result.</summary>
        public abstract int GetFirstByte(double value);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Float64],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float64x2SplatExport.GetFirstByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Float64x2Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float64x2Splat produces correct results.</summary>
    [TestMethod]
    public void Float64x2Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float64x2SplatExport>();
        // 1.0 in IEEE 754 double little-endian = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F]
        var bits = BitConverter.GetBytes(1.0);
        Assert.AreEqual((int)bits[0], compiled.Exports.GetFirstByte(1.0));
    }
}
