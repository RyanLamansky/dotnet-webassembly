using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Splat"/> instruction.</summary>
[TestClass]
public class Int16x8SplatTests
{
    /// <summary>Export for Int16x8Splat test.</summary>
    public abstract class Int16x8SplatExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8SplatExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new LocalGet(0),
                new Int16x8Splat(),
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8Splat produces correct results.</summary>
    [TestMethod]
    public void Int16x8Splat_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8SplatExport>();
        // splat(0x0102) → low byte = 0x02
        Assert.AreEqual(0x02, compiled.Exports.GetByte(0x0102));
        Assert.AreEqual(0, compiled.Exports.GetByte(0));
    }
}
