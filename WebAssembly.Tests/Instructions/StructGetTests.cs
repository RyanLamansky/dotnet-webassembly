using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructGet"/> instruction.
/// </summary>
[TestClass]
public class StructGetTests
{
    /// <summary>Tests <see cref="StructGet"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void StructGet_OpCodeAndEquality()
    {
        var instruction = new StructGet { Type = 1, Field = 2 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructGet, instruction.GCOpCode);
        Assert.AreEqual(1u, instruction.Type);
        Assert.AreEqual(2u, instruction.Field);
        Assert.AreEqual(new StructGet { Type = 1, Field = 2 }, instruction);
        Assert.AreNotEqual(new StructGet { Type = 1, Field = 3 }, instruction);
        Assert.AreEqual(new StructGet { Type = 1, Field = 2 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary>Plain <c>struct.get</c> of a packed field is a validation error (requires <c>_s</c>/<c>_u</c>).</summary>
    [TestMethod]
    public void StructGet_PackedField_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { Packed = PackedType.I8 }] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));
        module.Codes.Add(new FunctionBody(
            new StructNewDefault { Type = 0 },
            new StructGet { Type = 0, Field = 0 },
            new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<CompilerTestBase0<int>>());
    }
}
