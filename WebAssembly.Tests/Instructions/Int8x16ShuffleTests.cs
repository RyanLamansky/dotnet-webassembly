using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Shuffle"/> instruction.</summary>
[TestClass]
public class Int8x16ShuffleTests
{
    /// <summary>Export for Int8x16Shuffle test.</summary>
    public abstract class Int8x16ShuffleExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16ShuffleExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                // a = [1,2,...,16], b = [17,18,...,32]
                new V128Const { Value = [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16] },
                new V128Const { Value = [17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32] },
                // indices: [0,16,1,17,...] picks a[0]=1, b[0]=17, a[1]=2, b[1]=18 ...
                new Int8x16Shuffle { Indices = [0,16,1,17, 2,18,3,19, 4,20,5,21, 6,22,7,23] },
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16Shuffle produces correct results.</summary>
    [TestMethod]
    public void Int8x16Shuffle_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16ShuffleExport>();
        // result[0] = a[0] = 1, result[1] = b[0] = 17
        Assert.AreEqual(1, compiled.Exports.GetByte(0), "Byte 0 mismatch");
        Assert.AreEqual(17, compiled.Exports.GetByte(1), "Byte 1 mismatch");
    }
}
