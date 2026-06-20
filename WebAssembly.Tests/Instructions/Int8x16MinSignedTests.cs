using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16MinSigned"/> instruction.</summary>
[TestClass]
public class Int8x16MinSignedTests
{
    /// <summary>Export for Int8x16MinSigned test.</summary>
    public abstract class Int8x16MinSignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16MinSignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127] },
                new V128Const { Value = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128] },
                new Int8x16MinSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16MinSigned produces correct results.</summary>
    [TestMethod]
    public void Int8x16MinSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16MinSignedExport>();
        int[] expected = [128, 128, 128, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i} mismatch");
    }
}
