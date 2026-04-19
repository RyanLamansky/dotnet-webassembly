using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableCopy"/> instruction.
/// </summary>
[TestClass]
public class TableCopyTests
{
    /// <summary>Export with no return.</summary>
    public abstract class VoidExport
    {
        /// <summary>Runs the function.</summary>
        public abstract void Test();
    }

    /// <summary>
    /// Tests that table.copy throws NotSupportedException at runtime (stub).
    /// </summary>
    [TestMethod]
    public void TableCopy_ThrowsNotSupported()
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
                new TableCopy { DstTableIndex = 0, SrcTableIndex = 0 },
                new End(),
            ],
        });

        Assert.ThrowsException<ModuleLoadException>(() => module.ToInstance<VoidExport>());
    }
}
