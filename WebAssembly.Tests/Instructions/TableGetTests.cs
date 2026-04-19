using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="TableGet"/> instruction.
/// </summary>
[TestClass]
public class TableGetTests
{
    /// <summary>Export that reads a table element by index.</summary>
    public abstract class TableGetExport
    {
        /// <summary>Returns the table element at the given index.</summary>
        public abstract object? Test(int index);
    }

    /// <summary>
    /// Tests that table.get returns null for an uninitialized slot.
    /// </summary>
    [TestMethod]
    public void TableGet_UninitializedSlot_ReturnsNull()
    {
        var module = new Module();
        module.Tables.Add(new Table(4, 10));
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.FuncRef],
        });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableGetExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code =
            [
                new LocalGet(0),
                new TableGet(0),
                new End(),
            ],
        });

        var compiled = module.ToInstance<TableGetExport>();
        Assert.IsNull(compiled.Exports.Test(0));
        Assert.IsNull(compiled.Exports.Test(3));
    }
}
