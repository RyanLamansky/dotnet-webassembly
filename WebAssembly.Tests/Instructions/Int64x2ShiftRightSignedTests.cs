using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2ShiftRightSigned"/> instruction.</summary>
[TestClass]
public class Int64x2ShiftRightSignedTests
{
    /// <summary>Export for Int64x2ShiftRightSigned test.</summary>
    public abstract class Int64x2ShiftRightSignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int64x2ShiftRightSignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int32Constant(1),
                new Int64x2ShiftRightSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int64x2ShiftRightSigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2ShiftRightSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int64x2ShiftRightSignedExport>();
        Assert.AreEqual(0, compiled.Exports.GetByte(0));
    }
}
