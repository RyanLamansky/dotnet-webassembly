using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableInit"/> instruction.
/// </summary>
[TestClass]
public class TableInitTests
{
    /// <summary>Export with no return.</summary>
    public abstract class VoidExport
    {
        /// <summary>Runs the function.</summary>
        public abstract void Test();
    }

    /// <summary>
    /// Tests that table.init throws ModuleLoadException when the referenced element segment is not passive.
    /// </summary>
    [TestMethod]
    public void TableInit_NoPassiveSegment_ThrowsCompilerException()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Returns = [], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(VoidExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new Int32Constant(0),
                new Int32Constant(0),
                new TableInit { SegmentIndex = 0, TableIndex = 0 },
                new End(),
            ],
        });

        Assert.ThrowsException<ModuleLoadException>(() => module.ToInstance<VoidExport>());
    }
}
