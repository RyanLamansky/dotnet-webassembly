using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly;

/// <summary>
/// Tests parsing of WASM 2.0 element segment kinds.
/// </summary>
[TestClass]
public class ElementSegmentTests
{
    /// <summary>Export that returns the table size.</summary>
    public abstract class TableSizeExport
    {
        /// <summary>Returns the current table size.</summary>
        public abstract int Size();
    }

    /// <summary>
    /// Tests that a module with a kind-0 element segment (active, table 0) round-trips through Module.
    /// </summary>
    [TestMethod]
    public void ElementSegment_Kind0_RoundTrips()
    {
        var module = new Module();
        module.Tables.Add(new Table(4, null));
        module.Types.Add(new WebAssemblyType { Returns = [], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSizeExport.Size) });
        module.Codes.Add(new FunctionBody
        {
            Code = [new Instructions.TableSize(), new Instructions.End()],
        });
        // Kind 0 element segment, placing func 0 at offset 0
        module.Elements.Add(new Element(0, 0u));

        using var ms = new MemoryStream();
        module.WriteToBinary(ms);
        ms.Position = 0;
        var roundTripped = Module.ReadFromBinary(ms);

        Assert.AreEqual(1, roundTripped.Elements.Count);
        Assert.AreEqual(0u, roundTripped.Elements[0].Kind);
        Assert.AreEqual(1, roundTripped.Elements[0].Elements.Count);
    }

    /// <summary>
    /// Tests that a module with kind-1 (passive) element segment parses without error.
    /// </summary>
    [TestMethod]
    public void ElementSegment_Kind1_ParsesWithoutError()
    {
        var element = new Element { Kind = 1 };
        element.Elements.Add(0u);

        var module = new Module();
        module.Tables.Add(new Table(4, null));
        module.Types.Add(new WebAssemblyType { Returns = [], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSizeExport.Size) });
        module.Codes.Add(new FunctionBody
        {
            Code = [new Instructions.TableSize(), new Instructions.End()],
        });
        module.Elements.Add(element);

        using var ms = new MemoryStream();
        module.WriteToBinary(ms);
        ms.Position = 0;
        var roundTripped = Module.ReadFromBinary(ms);

        Assert.AreEqual(1, roundTripped.Elements.Count);
        Assert.AreEqual(1u, roundTripped.Elements[0].Kind);
    }

    /// <summary>
    /// Tests that a module with kind-1 element segment compiles (segment is skipped at runtime).
    /// </summary>
    [TestMethod]
    public void ElementSegment_Kind1_CompilesAndRuns()
    {
        var element = new Element { Kind = 1 };
        element.Elements.Add(0u);

        var module = new Module();
        module.Tables.Add(new Table(4, null));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSizeExport.Size) });
        module.Codes.Add(new FunctionBody
        {
            Code = [new Instructions.TableSize(), new Instructions.End()],
        });
        module.Elements.Add(element);

        var compiled = module.ToInstance<TableSizeExport>();
        Assert.AreEqual(4, compiled.Exports.Size());
    }

    /// <summary>
    /// Tests that a module with kind-3 (declarative) element segment compiles without error.
    /// </summary>
    [TestMethod]
    public void ElementSegment_Kind3_CompilesAndRuns()
    {
        var element = new Element { Kind = 3 };
        element.Elements.Add(0u);

        var module = new Module();
        module.Tables.Add(new Table(4, null));
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32], Parameters = [] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(TableSizeExport.Size) });
        module.Codes.Add(new FunctionBody
        {
            Code = [new Instructions.TableSize(), new Instructions.End()],
        });
        module.Elements.Add(element);

        var compiled = module.ToInstance<TableSizeExport>();
        Assert.AreEqual(4, compiled.Exports.Size());
    }
}
