using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4Splat"/> instruction.</summary>
[TestClass]
public class Int32x4SplatTests
{
    /// <summary>Export for Int32x4Splat test.</summary>
    public abstract class Int32x4SplatExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4SplatExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Int32x4Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4Splat produces correct results.</summary>
    [TestMethod]
    public void Int32x4Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4SplatExport>();
        // splat(0x01020304) → byte 0 = 0x04
        Assert.AreEqual(0x04, compiled.Exports.GetByte(0x01020304));
        Assert.AreEqual(0, compiled.Exports.GetByte(0));
    }
}
