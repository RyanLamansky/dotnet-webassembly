using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Sub"/> instruction.</summary>
[TestClass]
public class Int16x8SubTests
{
    /// <summary>Export for Int16x8Sub test.</summary>
    public abstract class Int16x8SubExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8SubExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0, 5, 0] },
                new V128Const { Value = [3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0, 3, 0] },
                new Int16x8Sub(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8Sub produces correct results.</summary>
    [TestMethod]
    public void Int16x8Sub_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8SubExport>();
        int[] expected = [2, 0, 2, 0];
        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], compiled.Exports.GetByte(i), $"Byte {i} mismatch");
    }
}
