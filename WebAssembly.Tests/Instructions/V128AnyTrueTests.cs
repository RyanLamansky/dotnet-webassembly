using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>Tests the <see cref="V128AnyTrue"/> instruction.</summary>
[TestClass]
public class V128AnyTrueTests
{
    /// <summary>Export for V128AnyTrue test.</summary>
    public abstract class V128AnyTrueExport
    {
        /// <summary>Returns the i32 result.</summary>
        public abstract int GetResult();
    }

    private static Module BuildModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new WebAssembly.Export { Name = nameof(V128AnyTrueExport.GetResult) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new V128Const { Value = [1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0] },
                new V128AnyTrue(),
                new End(),
            ],
        });
        return module;
    }

    /// <summary>Verifies V128AnyTrue produces correct results.</summary>
    [TestMethod]
    public void V128AnyTrue_IsCorrect()
    {
        var compiled = BuildModule().ToInstance<V128AnyTrueExport>();
        Assert.AreEqual(1, compiled.Exports.GetResult());
    }
}
