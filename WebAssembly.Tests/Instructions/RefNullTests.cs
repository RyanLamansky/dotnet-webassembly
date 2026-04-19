using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefNull"/> instruction.
/// </summary>
[TestClass]
public class RefNullTests
{
    /// <summary>Export that returns a nullable funcref.</summary>
    public abstract class FuncRefNullExport
    {
        /// <summary>Returns a null funcref.</summary>
        public abstract object? Test();
    }

    /// <summary>
    /// Tests that ref.null funcref compiles and returns null.
    /// </summary>
    [TestMethod]
    public void RefNull_FuncRef_Compiled()
    {
        var exports = AssemblyBuilder.CreateInstance<FuncRefNullExport>(
            nameof(FuncRefNullExport.Test),
            [WebAssemblyValueType.FuncRef],
            [],
            new RefNull(WebAssemblyValueType.FuncRef),
            new End());

        Assert.IsNull(exports.Test());
    }

    /// <summary>
    /// Tests that ref.null externref compiles and returns null.
    /// </summary>
    [TestMethod]
    public void RefNull_ExternRef_Compiled()
    {
        var exports = AssemblyBuilder.CreateInstance<FuncRefNullExport>(
            nameof(FuncRefNullExport.Test),
            [WebAssemblyValueType.ExternRef],
            [],
            new RefNull(WebAssemblyValueType.ExternRef),
            new End());

        Assert.IsNull(exports.Test());
    }
}
