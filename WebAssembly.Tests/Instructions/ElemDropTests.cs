using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ElemDrop"/> instruction.
/// </summary>
[TestClass]
public class ElemDropTests
{
    /// <summary>Export with no return.</summary>
    public abstract class VoidExport
    {
        /// <summary>Runs the function.</summary>
        public abstract void Test();
    }

    /// <summary>
    /// Tests that elem.drop throws NotSupportedException at runtime (stub).
    /// </summary>
    [TestMethod]
    public void ElemDrop_ThrowsNotSupported()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(VoidExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new ElemDrop { SegmentIndex = 0 },
                new End(),
            ],
        });

        Assert.ThrowsException<ModuleLoadException>(() => module.ToInstance<VoidExport>());
    }
}
