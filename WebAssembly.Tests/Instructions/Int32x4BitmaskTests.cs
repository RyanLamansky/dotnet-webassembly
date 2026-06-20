using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4Bitmask"/> instruction.</summary>
[TestClass]
public class Int32x4BitmaskTests
{
    /// <summary>Export for Int32x4Bitmask test.</summary>
    public abstract class Int32x4BitmaskExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4BitmaskExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int32x4Bitmask(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4Bitmask produces correct results.</summary>
    [TestMethod]
    public void Int32x4Bitmask_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4BitmaskExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
