using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests that <see cref="Compile"/> rejects an import whose supplied value is type-incompatible with the
/// module's declaration: a global's value type or mutability, or a memory's or table's limits.
/// </summary>
[TestClass]
public class ImportValidationTests
{
    static Module GlobalModule(WebAssemblyValueType contentType, bool mutable)
    {
        var module = new Module();
        module.Imports.Add(new Import.Global { Module = "M", Field = "F", ContentType = contentType, IsMutable = mutable });
        return module;
    }

    static Module MemoryModule(uint minimum, uint? maximum)
    {
        var module = new Module();
        module.Imports.Add(new Import.Memory("M", "F", new Memory(minimum, maximum)));
        return module;
    }

    static Module TableModule(uint minimum, uint? maximum)
    {
        var module = new Module();
        module.Imports.Add(new Import.Table("M", "F", minimum, maximum));
        return module;
    }

    static ImportDictionary With(RuntimeImport import) => new() { { "M", "F", import } };

    /// <summary>A global supplied with the wrong value type is rejected.</summary>
    [TestMethod]
    public void GlobalImport_ValueTypeMismatch_Throws()
    {
        var module = GlobalModule(WebAssemblyValueType.Int32, mutable: false);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new GlobalImport(() => 0L)))); // i64 supplied for an i32 import
    }

    /// <summary>A global supplied with the wrong mutability is rejected.</summary>
    [TestMethod]
    public void GlobalImport_MutabilityMismatch_Throws()
    {
        var module = GlobalModule(WebAssemblyValueType.Int32, mutable: true);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new GlobalImport(() => 0)))); // immutable supplied for a mutable import
    }

    /// <summary>A global matching both value type and mutability is accepted.</summary>
    [TestMethod]
    public void GlobalImport_Compatible_Succeeds()
    {
        var module = GlobalModule(WebAssemblyValueType.Int32, mutable: false);
        Assert.IsNotNull(module.ToInstance<object>(With(new GlobalImport(() => 0))));
    }

    /// <summary>A memory smaller than the declared minimum is rejected.</summary>
    [TestMethod]
    public void MemoryImport_BelowMinimum_Throws()
    {
        var module = MemoryModule(2, null);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new MemoryImport(() => new UnmanagedMemory(1, null)))));
    }

    /// <summary>A memory whose maximum exceeds the declared maximum is rejected.</summary>
    [TestMethod]
    public void MemoryImport_AboveMaximum_Throws()
    {
        var module = MemoryModule(1, 1);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new MemoryImport(() => new UnmanagedMemory(1, 5)))));
    }

    /// <summary>A memory within the declared limits is accepted.</summary>
    [TestMethod]
    public void MemoryImport_Compatible_Succeeds()
    {
        var module = MemoryModule(1, 2);
        Assert.IsNotNull(module.ToInstance<object>(With(new MemoryImport(() => new UnmanagedMemory(1, 2)))));
    }

    /// <summary>A table smaller than the declared minimum is rejected.</summary>
    [TestMethod]
    public void TableImport_BelowMinimum_Throws()
    {
        var module = TableModule(2, null);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new FunctionTable(1))));
    }

    /// <summary>A table whose maximum exceeds the declared maximum is rejected.</summary>
    [TestMethod]
    public void TableImport_AboveMaximum_Throws()
    {
        var module = TableModule(1, 1);
        Assert.ThrowsException<ImportException>(
            () => module.ToInstance<object>(With(new FunctionTable(1, 5))));
    }

    /// <summary>A table within the declared limits is accepted.</summary>
    [TestMethod]
    public void TableImport_Compatible_Succeeds()
    {
        var module = TableModule(1, 2);
        Assert.IsNotNull(module.ToInstance<object>(With(new FunctionTable(1, 2))));
    }
}
