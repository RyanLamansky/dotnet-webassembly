using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="Int16x8AllTrue"/> instruction.</summary>
[TestClass]
public class Int16x8AllTrueTests
{
    /// <summary>Export for Int16x8AllTrue test.</summary>
    public abstract class Int16x8AllTrueExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(Int16x8AllTrueExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0] },
                new Int16x8AllTrue(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies Int16x8AllTrue produces correct results.</summary>
    [TestMethod]
    public void Int16x8AllTrue_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<Int16x8AllTrueExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
