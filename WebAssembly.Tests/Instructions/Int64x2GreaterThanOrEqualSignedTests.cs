using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int64x2GreaterThanOrEqualSigned"/> instruction.</summary>
[TestClass]
public class Int64x2GreaterThanOrEqualSignedTests
{
    /// <summary>Export for Int64x2GreaterThanOrEqualSigned test.</summary>
    public abstract class Int64x2GreaterThanOrEqualSignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int64x2GreaterThanOrEqualSignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int64x2GreaterThanOrEqualSigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int64x2GreaterThanOrEqualSigned produces correct results.</summary>
    [TestMethod]
    public void Int64x2GreaterThanOrEqualSigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int64x2GreaterThanOrEqualSignedExport>();
        Assert.AreEqual(255, compiled.Exports.GetByte(0));
    }
}
