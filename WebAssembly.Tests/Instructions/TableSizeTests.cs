using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableSize"/> instruction.
/// </summary>
[TestClass]
public class TableSizeTests
{
    /// <summary>Export that returns the table size.</summary>
    public abstract class TableSizeExport
    {
        /// <summary>Returns the current table size.</summary>
        public abstract int Test();
    }

    /// <summary>
    /// Tests that table.size returns the initial table element count.
    /// </summary>
    [TestMethod]
    public void TableSize_ReturnsInitialSize()
    {
        var module = new Module();
        module.Tables.Add(new Table(5, 10));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSizeExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new TableSize { TableIndex = 0 },
                new End(),
            ],
        });

        var compiled = module.ToInstance<TableSizeExport>();
        Assert.AreEqual(5, compiled.Exports.Test());
    }
}
