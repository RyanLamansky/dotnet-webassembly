using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefFunc"/> instruction.
/// </summary>
[TestClass]
public class RefFuncTests
{
    /// <summary>Export that returns a funcref.</summary>
    public abstract class RefFuncExport
    {
        /// <summary>Returns a funcref.</summary>
        public abstract object? Test();
    }

    /// <summary>
    /// Tests that ref.func compiles without error.
    /// The current implementation returns null as a placeholder until full function-reference storage is implemented.
    /// </summary>
    [TestMethod]
    public void RefFunc_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Returns = [WebAssemblyValueType.FuncRef],
        });
        module.Functions.Add(new Function { Type = 0 });
        module.Exports.Add(new Export { Name = nameof(RefFuncExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new RefFunc(0),
                new End()
            ]
        });

        var instance = module.ToInstance<RefFuncExport>();
        // Result is null until ref.func emits real function references.
        _ = instance.Exports.Test();
    }
}
