using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;
using System;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float64x2ReplaceLane"/> instruction.</summary>
[TestClass]
public class Float64x2ReplaceLaneTests
{
    /// <summary>Export for Float64x2ReplaceLane test.</summary>
    public abstract class Float64x2ReplaceLaneExport
    {
        /// <summary>Returns the first byte of lane 0 after replacing it with 1.0.</summary>
        public abstract int GetByte();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float64x2ReplaceLaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] },
                new Float64Constant(1.0),
                new Float64x2ReplaceLane { LaneIndex = 0 },
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float64x2ReplaceLane produces correct results.</summary>
    [TestMethod]
    public void Float64x2ReplaceLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float64x2ReplaceLaneExport>();
        var expected = (int)BitConverter.GetBytes(1.0)[0];
        Assert.AreEqual(expected, compiled.Exports.GetByte());
    }
}
