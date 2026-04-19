using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableGrow"/> instruction.
/// </summary>
[TestClass]
public class TableGrowTests
{
    /// <summary>Export that grows a table.</summary>
    public abstract class TableGrowExport
    {
        /// <summary>Grows the table by delta and returns the old size.</summary>
        public abstract int Test(int delta);
    }

    /// <summary>
    /// Tests that table.grow returns the previous size.
    /// </summary>
    [TestMethod]
    public void TableGrow_ReturnsPreviousSize()
    {
        var module = new Module();
        module.Tables.Add(new Table(2, 10));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableGrowExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // ref.null funcref (the init value — ignored by our stub Grow)
                new RefNull(WebAssemblyValueType.FuncRef),
                // delta = param0
                new LocalGet(0),
                new TableGrow { TableIndex = 0 },
                new End(),
            ],
        });

        var compiled = module.ToInstance<TableGrowExport>();
        // Initial size is 2; grow by 3 → returns 2
        Assert.AreEqual(2, compiled.Exports.Test(3));
    }
}
