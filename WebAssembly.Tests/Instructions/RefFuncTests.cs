using System;
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
        public abstract Delegate? Test();
    }

    /// <summary>
    /// Tests that ref.func returns a callable function reference.
    /// </summary>
    [TestMethod]
    public void RefFunc_Compiled()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Returns = [WebAssemblyValueType.FuncRef],
        });
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function { Type = 0 });
        module.Functions.Add(new Function { Type = 1 });
        module.Exports.Add(new Export { Name = nameof(RefFuncExport.Test) });
        module.Exports.Add(new Export("target", 1));
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new RefFunc(1),
                new End()
            ]
        });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new End()
            ]
        });

        var instance = module.ToInstance<RefFuncExport>();
        var functionReference = instance.Exports.Test();

        Assert.IsNotNull(functionReference);
        Assert.AreEqual(7, functionReference.DynamicInvoke(7));
    }
}
