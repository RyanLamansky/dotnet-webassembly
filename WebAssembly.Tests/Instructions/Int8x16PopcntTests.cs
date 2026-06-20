using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Popcnt"/> instruction.</summary>
[TestClass]
public class Int8x16PopcntTests
{
    /// <summary>Export for Int8x16Popcnt test.</summary>
    public abstract class Int8x16PopcntExport
    {
        /// <summary>Returns the byte at the given offset of the result.</summary>
        public abstract int GetByte(int offset);
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16PopcntExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int8x16Popcnt(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16Popcnt produces correct results.</summary>
    [TestMethod]
    public void Int8x16Popcnt_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16PopcntExport>();
        Assert.AreEqual(8, compiled.Exports.GetByte(0));
    }
}
