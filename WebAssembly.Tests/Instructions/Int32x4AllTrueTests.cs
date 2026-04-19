using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int32x4AllTrue"/> instruction.</summary>
[TestClass]
public class Int32x4AllTrueTests
{
    /// <summary>Export for Int32x4AllTrue test.</summary>
    public abstract class Int32x4AllTrueExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int32x4AllTrueExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0] },
                new Int32x4AllTrue(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int32x4AllTrue produces correct results.</summary>
    [TestMethod]
    public void Int32x4AllTrue_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int32x4AllTrueExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
