using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableSet"/> instruction.
/// </summary>
[TestClass]
public class TableSetTests
{
    /// <summary>Export that writes then reads a table element.</summary>
    public abstract class TableSetExport
    {
        /// <summary>Writes null to the given index and reads it back (verifies no exception).</summary>
        public abstract object? Test(int index);
    }

    /// <summary>
    /// Tests that table.set compiles and executes without error when storing null.
    /// </summary>
    [TestMethod]
    public void TableSet_StoreNull_NoException()
    {
        var module = new Module();
        module.Tables.Add(new Table(4, 10));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.FuncRef],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSetExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                // table.set(index=param0, ref=null)
                new LocalGet(0),
                new RefNull(WebAssemblyValueType.FuncRef),
                new TableSet(0),
                // table.get(index=param0) → return null
                new LocalGet(0),
                new TableGet(0),
                new End(),
            ],
        });

        var compiled = module.ToInstance<TableSetExport>();
        Assert.IsNull(compiled.Exports.Test(1));
    }
}
