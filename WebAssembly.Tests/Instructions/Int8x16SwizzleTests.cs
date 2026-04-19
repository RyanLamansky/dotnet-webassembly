using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Swizzle"/> instruction.</summary>
[TestClass]
public class Int8x16SwizzleTests
{
    /// <summary>Export for Int8x16Swizzle test.</summary>
    public abstract class Int8x16SwizzleExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16SwizzleExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                // a = [10,20,30,40, 50,60,70,80, 90,100,110,120, 130,140,150,160]
                new V128Const { Value = [10,20,30,40, 50,60,70,80, 90,100,110,120, 130,140,150,160] },
                // indices = [0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0] → all first byte (10)
                new V128Const { Value = [0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0] },
                new Int8x16Swizzle(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16Swizzle produces correct results.</summary>
    [TestMethod]
    public void Int8x16Swizzle_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16SwizzleExport>();
        Assert.AreEqual(10, compiled.Exports.GetByte(0), "Byte 0 mismatch");
    }
}
