using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="StructNewDefault"/> instruction.
/// </summary>
[TestClass]
public class StructNewDefaultTests
{
    /// <summary>Tests <see cref="StructNewDefault"/> opcode, immediate, and equality.</summary>
    [TestMethod]
    public void StructNewDefault_OpCodeAndEquality()
    {
        var instruction = new StructNewDefault { Type = 2 };
        Assert.AreEqual(OpCode.GCPrefix, instruction.OpCode);
        Assert.AreEqual(GCOpCode.StructNewDefault, instruction.GCOpCode);
        Assert.AreEqual(new StructNewDefault { Type = 2 }, instruction);
        Assert.AreNotEqual(new StructNewDefault { Type = 3 }, instruction);
        Assert.AreEqual(new StructNewDefault { Type = 2 }.GetHashCode(), instruction.GetHashCode());
    }

    /// <summary><c>struct.new_default</c> zero-initializes numeric fields.</summary>
    [TestMethod]
    public void StructNewDefault_DefaultsToZero()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Form = FunctionType.Struct, Fields = [new StructField { StorageType = WebAssemblyValueType.Int32 }] });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(CompilerTestBase0<int>.Test), 0));
        module.Codes.Add(new FunctionBody(
            new StructNewDefault { Type = 0 },
            new StructGet { Type = 0, Field = 0 },
            new End()));

        Assert.AreEqual(0, module.ToInstance<CompilerTestBase0<int>>().Exports.Test());
    }

    /// <summary><c>struct.new_default</c> on a struct with a non-nullable reference field is a validation error.</summary>
    [TestMethod]
    public void StructNewDefault_NonDefaultableField_Rejected()
    {
        var module = new Module();
        // A self-referential non-nullable (ref 0) field has no default value.
        module.Types.Add(new WebAssemblyType
        {
            Form = FunctionType.Struct,
            Fields = [new StructField { StorageType = WebAssemblyValueType.Reference, StorageRefType = new ReferenceType(0u, false) }],
        });
        module.Types.Add(new WebAssemblyType { Returns = [WebAssemblyValueType.AnyRef] });
        module.Functions.Add(new Function(1));
        module.Exports.Add(new Export(nameof(AnyExport.Test), 0));
        module.Codes.Add(new FunctionBody(
            new StructNewDefault { Type = 0 },
            new End()));

        Assert.ThrowsExactly<ModuleLoadException>(() => module.ToInstance<AnyExport>());
    }

    /// <summary>A zero-argument export returning an <c>anyref</c> (object).</summary>
    public abstract class AnyExport
    {
        /// <summary>The exported function.</summary>
        public abstract object Test();
    }
}
