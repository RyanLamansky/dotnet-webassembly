using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Splat"/> instruction.</summary>
[TestClass]
public class Int8x16SplatTests
{
    /// <summary>Export for Int8x16Splat test.</summary>
    public abstract class Int8x16SplatExport
    {
        /// <summary>Splats a value and returns the first byte of the result.</summary>
        public abstract int GetByte(int value);
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16SplatExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Int8x16Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16Splat produces correct results.</summary>
    [TestMethod]
    public void Int8x16Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16SplatExport>();
        Assert.AreEqual(42, compiled.Exports.GetByte(42));
        Assert.AreEqual(0, compiled.Exports.GetByte(0));
        Assert.AreEqual(255, compiled.Exports.GetByte(255));
    }
}
