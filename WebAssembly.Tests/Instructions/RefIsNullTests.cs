using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefIsNull"/> instruction.
/// </summary>
[TestClass]
public class RefIsNullTests
{
    /// <summary>Export that tests whether a funcref is null.</summary>
    public abstract class RefIsNullExport
    {
        /// <summary>Returns 1 if the input funcref is null, 0 otherwise.</summary>
        public abstract int Test();
    }

    /// <summary>
    /// Tests that ref.is_null on a null ref returns 1.
    /// </summary>
    [TestMethod]
    public void RefIsNull_NullRef_Returns1()
    {
        var exports = AssemblyBuilder.CreateInstance<RefIsNullExport>(
            nameof(RefIsNullExport.Test),
            [WebAssemblyValueType.Int32],
            [],
            new RefNull(WebAssemblyValueType.FuncRef),
            new RefIsNull(),
            new End());

        Assert.AreEqual(1, exports.Test());
    }
}
