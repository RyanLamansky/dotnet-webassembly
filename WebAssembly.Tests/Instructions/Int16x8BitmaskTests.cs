using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8Bitmask"/> instruction.</summary>
[TestClass]
public class Int16x8BitmaskTests
{
    /// <summary>Export for Int16x8Bitmask test.</summary>
    public abstract class Int16x8BitmaskExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8BitmaskExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new Int16x8Bitmask(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8Bitmask produces correct results.</summary>
    [TestMethod]
    public void Int16x8Bitmask_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8BitmaskExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
