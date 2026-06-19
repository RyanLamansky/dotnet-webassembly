using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Splat"/> instruction.</summary>
[TestClass]
public class Int64x2SplatTests
{
    /// <summary>Export for Int64x2Splat test.</summary>
    public abstract class Int64x2SplatExport
    {
        /// <summary>Splats a value and returns the first byte of the result.</summary>
        public abstract int GetFirstByte(long value);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int64],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int64x2SplatExport.GetFirstByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Int64x2Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int64x2Splat produces correct results.</summary>
    [TestMethod]
    public void Int64x2Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int64x2SplatExport>();
        Assert.AreEqual(0xAB, compiled.Exports.GetFirstByte(0xABL));
        Assert.AreEqual(0, compiled.Exports.GetFirstByte(0L));
    }
}
