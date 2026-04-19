using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Float32x4ConvertInt32x4Signed"/> instruction.</summary>
[TestClass]
public class Float32x4ConvertInt32x4SignedTests
{
    /// <summary>Export for Float32x4ConvertInt32x4Signed test.</summary>
    public abstract class Float32x4ConvertInt32x4SignedExport
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
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Float32x4ConvertInt32x4SignedExport.GetByte) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new V128Const { Value = [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0] },
                new Float32x4ConvertInt32x4Signed(),
                new V128Store(),
                new LocalGet(0),
                new Int32Load8Unsigned(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Float32x4ConvertInt32x4Signed produces correct results.</summary>
    [TestMethod]
    public void Float32x4ConvertInt32x4Signed_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Float32x4ConvertInt32x4SignedExport>();
        Assert.AreEqual(0, compiled.Exports.GetByte(0));
    }
}
