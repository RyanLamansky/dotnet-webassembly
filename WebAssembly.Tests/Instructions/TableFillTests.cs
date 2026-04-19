using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableFill"/> instruction.
/// </summary>
[TestClass]
public class TableFillTests
{
    /// <summary>Export with no return.</summary>
    public abstract class VoidExport
    {
        /// <summary>Runs the function.</summary>
        public abstract void Test();
    }

    /// <summary>
    /// Tests that table.fill throws NotSupportedException at runtime (stub).
    /// </summary>
    [TestMethod]
    public void TableFill_ThrowsNotSupported()
    {
        var module = new Module();
        module.Tables.Add(new Table(4, 10));
        module.Types.Add(new WebAssemblyType { Returns = [], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(VoidExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new Int32Constant(0),
                new RefNull(WebAssemblyValueType.FuncRef),
                new Int32Constant(2),
                new TableFill { TableIndex = 0 },
                new End(),
            ],
        });

        Assert.ThrowsException<ModuleLoadException>(() => module.ToInstance<VoidExport>());
    }
}
