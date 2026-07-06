using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructSet"/> instruction.
/// </summary>
[TestClass]
public class StructSetTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="StructSet"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void StructSet_OpCodeAndEquality()
    {
        var instruction = new StructSet { Type = 0, Field = 1 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructSet, instruction.GCOpCode);
        Assert.AreEqual(new StructSet { Type = 0, Field = 1 }, instruction);
        Assert.AreNotEqual(new StructSet { Type = 0, Field = 2 }, instruction);
        Assert.AreEqual(new StructSet { Type = 0, Field = 1 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>struct.set</c> writes a mutable field that <c>struct.get</c> reads back.</summary>
    [TestMethod]
    public void StructSet_WritesMutableField()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32, IsMutable = true }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));

        // Local 0 is the argument; local 1 (a nullable ref to the struct) holds the fresh struct reference.
        module.Codes.Add(new FunctionBody
        {
            Locals = [new Local { Count = 1, RefType = new ReferenceType(0u, true) }],
            Code =
            [
                new StructNewDefault { Type = 0 },
                new LocalTee(1),
                new LocalGet(0),
                new StructSet { Type = 0, Field = 0 },
                new LocalGet(1),
                new StructGet { Type = 0, Field = 0 },
                new End(),
            ],
        });

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(99, exports.Test(99));
    }

    /// <summary><c>struct.set</c> of an immutable field is a validation error.</summary>
    [TestMethod]
    public void StructSet_ImmutableField_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32 }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export("Test", 0));
        module.Codes.Add(new FunctionBody(
            new StructNewDefault { Type = 0 },
            new LocalGet(0),
            new StructSet { Type = 0, Field = 0 },
            new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<StructSetImmutableExport>());
    }
}

/// <summary>A single-argument, void export for the immutable-field rejection test.</summary>
public abstract class StructSetImmutableExport
{
    /// <summary>The exported function.</summary>
    public abstract void Test(int value);
}
