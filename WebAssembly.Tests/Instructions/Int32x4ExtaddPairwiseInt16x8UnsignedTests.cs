using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4ExtaddPairwiseInt16x8Unsigned"/> instruction.</summary>
[TestClass]
public class Int32x4ExtaddPairwiseInt16x8UnsignedTests
{
    /// <summary>Export for Int32x4ExtaddPairwiseInt16x8Unsigned test.</summary>
    public abstract class Int32x4ExtaddPairwiseInt16x8UnsignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4ExtaddPairwiseInt16x8UnsignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int32x4ExtaddPairwiseInt16x8Unsigned(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4ExtaddPairwiseInt16x8Unsigned produces correct results.</summary>
    [TestMethod]
    public void Int32x4ExtaddPairwiseInt16x8Unsigned_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4ExtaddPairwiseInt16x8UnsignedExport>();
        Assert.AreEqual(3, compiled.Exports.GetByte(0));
    }
}
