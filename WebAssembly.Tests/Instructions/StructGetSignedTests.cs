using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructGetSigned"/> instruction.
/// </summary>
[TestClass]
public class StructGetSignedTests
{
    /// <summary>A one-argument, one-result export.</summary>
    public abstract class Sample
    {
        /// <summary>The exported function.</summary>
        public abstract int Test(int value);
    }

    /// <summary>Tests <see cref="StructGetSigned"/> opcode, immediates, and equality.</summary>
    [TestMethod]
    public void StructGetSigned_OpCodeAndEquality()
    {
        var instruction = new StructGetSigned { Type = 1, Field = 0 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructGetSigned, instruction.GCOpCode);
        Assert.AreEqual(new StructGetSigned { Type = 1, Field = 0 }, instruction);
        Assert.AreNotEqual(new StructGetSigned { Type = 2, Field = 0 }, instruction);
        Assert.AreEqual(new StructGetSigned { Type = 1, Field = 0 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>struct.get_s</c> sign-extends a packed i8 field (0xFF becomes -1).</summary>
    [TestMethod]
    public void StructGetSigned_SignExtendsPackedI8()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { Packed = PackedType.I8, IsMutable = true }] });
        module.Types.Add(new WebAssemblyType { Parameters = [WebAssemblyValueType.Int32], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(Sample.Test), 0));
        module.Codes.Add(new FunctionBody(
            new LocalGet(0),
            new StructNew { Type = 0 },
            new StructGetSigned { Type = 0, Field = 0 },
            new End()));

        var exports = module.ToInstance<Sample>().Exports;
        Assert.AreEqual(-1, exports.Test(0xFF));
        Assert.AreEqual(-2, exports.Test(0xFE));
        Assert.AreEqual(1, exports.Test(1));
    }

    /// <summary><c>struct.get_s</c> of a non-packed field is a validation error.</summary>
    [TestMethod]
    public void StructGetSigned_NonPackedField_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32 }] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));
        module.Codes.Add(new FunctionBody(
            new StructNewDefault { Type = 0 },
            new StructGetSigned { Type = 0, Field = 0 },
            new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<CompilerTestBase0<int>>());
    }
}
