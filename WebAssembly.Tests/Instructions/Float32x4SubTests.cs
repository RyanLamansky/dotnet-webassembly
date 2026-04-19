using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4Sub"/> instruction.</summary>
[TestClass]
public class Float32x4SubTests
{
    /// <summary>Export for Float32x4Sub test.</summary>
    public abstract class Float32x4SubExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float32x4SubExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0, 0, 160, 64, 0, 0, 160, 64, 0, 0, 160, 64, 0, 0, 160, 64] },
                new V128Const { Value = [0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64, 0, 0, 0, 64] },
                new Float32x4Sub(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float32x4Sub produces correct results.</summary>
    [TestMethod]
    public void Float32x4Sub_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float32x4SubExport>();
        int[] expected = [0, 0, 64, 64];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i} mismatch");
    }
}
