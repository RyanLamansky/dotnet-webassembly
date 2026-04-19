using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16ReplaceLane"/> instruction.</summary>
[TestClass]
public class Int8x16ReplaceLaneTests
{
    /// <summary>Export for Int8x16ReplaceLane test.</summary>
    public abstract class Int8x16ReplaceLaneExport
    {
        /// <summary>Returns the first byte of the result after replacing lane 0 with 99.</summary>
        public abstract int GetByte();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Memories.Add(new Memory(1, 1));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16ReplaceLaneExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16] },
                new Int32Constant(99),
                new Int8x16ReplaceLane { LaneIndex = 0 },
                new V128Store(),
                new Int32Constant(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16ReplaceLane produces correct results.</summary>
    [TestMethod]
    public void Int8x16ReplaceLane_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16ReplaceLaneExport>();
        Assert.AreEqual(99, compiled.Exports.GetByte());
    }
}
