using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4MinSigned"/> instruction.</summary>
[TestClass]
public class Int32x4MinSignedTests
{
    /// <summary>Export for Int32x4MinSigned test.</summary>
    public abstract class Int32x4MinSignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4MinSignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [255, 255, 255, 127, 255, 255, 255, 127, 255, 255, 255, 127, 255, 255, 255, 127] },
                new V128Const { Value = [0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128] },
                new Int32x4MinSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4MinSigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4MinSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4MinSignedExport>();
        int[] expected = [0, 0, 0, 128];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i} mismatch");
    }
}
