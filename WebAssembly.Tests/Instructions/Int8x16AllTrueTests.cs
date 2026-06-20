using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int8x16AllTrue"/> instruction.</summary>
[TestClass]
public class Int8x16AllTrueTests
{
    /// <summary>Export for Int8x16AllTrue test.</summary>
    public abstract class Int8x16AllTrueExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int8x16AllTrueExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1] },
                new Int8x16AllTrue(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int8x16AllTrue produces correct results.</summary>
    [TestMethod]
    public void Int8x16AllTrue_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int8x16AllTrueExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
