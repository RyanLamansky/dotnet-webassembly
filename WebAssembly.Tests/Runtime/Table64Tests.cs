using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Tests the WebAssembly 3.0 table64 feature: i64 index operands for <c>table.get</c>/<c>table.set</c>/
/// <c>table.size</c>/<c>table.grow</c>/<c>table.fill</c>, honest trapping for out-of-range i64 indices, and the
/// reader/writer of 64-bit table limits.
/// </summary>
[TestClass]
public class Table64Tests
{
    /// <summary>Exposes a 64-bit externref table through an i64-indexed surface.</summary>
    public abstract class Table64Export
    {
        /// <summary>Returns the current table size as an i64.</summary>
        public abstract long Size();

        /// <summary>Grows the table by <paramref name="delta"/> (with a null initializer) and returns the previous size as an i64.</summary>
        public abstract long Grow(long delta);

        /// <summary>Writes <paramref name="value"/> to the table at the given i64 index.</summary>
        public abstract void Set(long index, object? value);

        /// <summary>Reads the table entry at the given i64 index.</summary>
        public abstract object? Get(long index);
    }

    private static Instance<Table64Export> CreateInstance()
    {
        var module = new Module();
        // A 64-bit externref table of two entries, growable to four.
        module.Tables.Add(new Table(2, 4) { ElementType = ElementType.ExternRef, Is64Bit = true });

        // Type 0: ()->i64; type 1: (i64)->i64; type 2: (i64, externref)->(); type 3: (i64)->externref.
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int64] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.Int64] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64, WebAssemblyValueType.ExternRef] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int64], Returns = [WebAssemblyValueType.ExternRef] });

        void AddFunction(string name, uint typeIndex, params Instruction[] code)
        {
            module.Functions.Add(new Function { Type = typeIndex });
            module.Exports.Add(new Export { Name = name, Index = (uint)(module.Functions.Count - 1) });
            module.Codes.Add(new FunctionBody { Code = code });
        }

        AddFunction(nameof(Table64Export.Size), 0,
            new TableSize(), new End());

        AddFunction(nameof(Table64Export.Grow), 1,
            new RefNull(WebAssemblyValueType.ExternRef), new LocalGet(0), new TableGrow(), new End());

        AddFunction(nameof(Table64Export.Set), 2,
            new LocalGet(0), new LocalGet(1), new TableSet(), new End());

        AddFunction(nameof(Table64Export.Get), 3,
            new LocalGet(0), new TableGet(), new End());

        return module.ToInstance<Table64Export>();
    }

    /// <summary>A store and load through i64 indices round-trips, and <c>table.size</c> returns an i64.</summary>
    [TestMethod]
    public void Table64_SetGet_RoundTripWithI64Index()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        Assert.AreEqual(2L, exports.Size());

        var reference = new object();
        Assert.IsNull(exports.Get(1));
        exports.Set(1, reference);
        Assert.AreSame(reference, exports.Get(1));
    }

    /// <summary><c>table.grow</c> returns the previous size as an i64, and a grow beyond the maximum returns -1.</summary>
    [TestMethod]
    public void Table64_Grow_ReturnsI64AndFailsWithMinusOne()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // Grow from two to four (the maximum) succeeds and returns the previous size.
        Assert.AreEqual(2L, exports.Grow(2));
        Assert.AreEqual(4L, exports.Size());

        // A further grow exceeds the maximum and returns -1 without changing the size.
        Assert.AreEqual(-1L, exports.Grow(1));
        Assert.AreEqual(4L, exports.Size());
    }

    /// <summary>An i64 index at or beyond the table length traps rather than wrapping to a valid slot.</summary>
    [TestMethod]
    public void Table64_OutOfRangeIndex_Traps()
    {
        using var instance = CreateInstance();
        var exports = instance.Exports;

        // 0x1_0000_0000 truncated to 32 bits would be 0 (a valid index); at full width it is far out of range.
        Assert.ThrowsExactly<TableAccessOutOfRangeException>(() => exports.Get(0x1_0000_0000L));
    }

    /// <summary>A 64-bit table serializes its limits with the i64 flag bit set and round-trips faithfully.</summary>
    [TestMethod]
    public void Table64_Limits_RoundTripWithFlagBit()
    {
        var module = new Module();
        module.Tables.Add(new Table(2, 5) { Is64Bit = true });

        using var stream = new MemoryStream();
        module.WriteToBinary(stream);
        stream.Position = 0;

        var roundTripped = Module.ReadFromBinary(stream);
        var table = roundTripped.Tables[0];
        Assert.IsTrue(table.Is64Bit);
        Assert.AreEqual(2u, table.ResizableLimits.Minimum);
        Assert.AreEqual(5u, table.ResizableLimits.Maximum);

        // The limits flag byte is 0x05: bit 0 (has-maximum) | bit 2 (i64), following the funcref element type 0x70.
        var bytes = stream.ToArray();
        var found = false;
        for (var i = 0; i + 4 <= bytes.Length; i++)
            if (bytes[i] == 0x70 && bytes[i + 1] == 0x05 && bytes[i + 2] == 0x02 && bytes[i + 3] == 0x05)
                found = true;
        Assert.IsTrue(found, "Expected table type 0x70 followed by limits 0x05, 0x02, 0x05.");
    }
}
