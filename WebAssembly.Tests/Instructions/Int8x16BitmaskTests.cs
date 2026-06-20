using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16Bitmask"/> instruction.</summary>
[TestClass]
public class Int8x16BitmaskTests
{
    /// <summary>Export for Int8x16Bitmask test.</summary>
    public abstract class Int8x16BitmaskExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16BitmaskExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int8x16Bitmask(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16Bitmask produces correct results.</summary>
    [TestMethod]
    public void Int8x16Bitmask_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16BitmaskExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
