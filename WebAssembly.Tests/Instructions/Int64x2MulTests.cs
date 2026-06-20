using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2Mul"/> instruction.</summary>
[TestClass]
public class Int64x2MulTests
{
    /// <summary>Export for Int64x2Mul test.</summary>
    public abstract class Int64x2MulExport
    {
        /// <summary>Computes the instruction result and returns the byte at the given offset.</summary>
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int64x2MulExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [3, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0] },
                new V128Const { Value = [4, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0] },
                new Int64x2Mul(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int64x2Mul produces correct results.</summary>
    [TestMethod]
    public void Int64x2Mul_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int64x2MulExport>();
        int[] expected = [12, 0, 0, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i} mismatch");
    }
}
