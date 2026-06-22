using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WebAssembly.Instructions;

namespace WebAssembly.Runtime;

/// <summary>
/// Verifies that stack-validation failures on prefixed instructions name the specific operation rather than the
/// generic <see cref="OpCode.MiscellaneousOperationPrefix"/> / <see cref="OpCode.SimdOperationPrefix"/> byte.
/// </summary>
[TestClass]
public class PrefixedOpCodeDiagnosticTests
{
    /// <summary>An export surface with no members; only the (invalid) function body is compiled.</summary>
    public abstract class Empty
    {
    }

    private static T CompileExpecting<T>(params Instruction[] code)
        where T : OpCodeCompilationException
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [] });
        module.Functions.Add(new Function());
        module.Codes.Add(new FunctionBody { Code = code });

        using var memory = new MemoryStream();
        module.WriteToBinary(memory);
        memory.Position = 0;
        return Assert.ThrowsExactly<T>(() => Compile.FromBinary<Empty>(memory)(new ImportDictionary()));
    }

    /// <summary>
    /// A miscellaneous (0xFC) instruction with too few operands reports its specific opcode, not the prefix.
    /// </summary>
    [TestMethod]
    public void Miscellaneous_StackTooSmall_NamesSpecificOpCode()
    {
        var exception = CompileExpecting<StackTooSmallException>(
            new Int32TruncateSaturateFloat32Signed(), new End());

        Assert.AreEqual(MiscellaneousOpCode.Int32TruncateSaturateFloat32Signed, exception.MiscellaneousOpCode);
        Assert.IsNull(exception.SimdOpCode);
        StringAssert.StartsWith(exception.Message, $"{MiscellaneousOpCode.Int32TruncateSaturateFloat32Signed} ");
        Assert.IsFalse(exception.Message.Contains(nameof(OpCode.MiscellaneousOperationPrefix)), exception.Message);
    }

    /// <summary>
    /// A miscellaneous (0xFC) instruction given the wrong operand type reports its specific opcode.
    /// </summary>
    [TestMethod]
    public void Miscellaneous_StackTypeInvalid_NamesSpecificOpCode()
    {
        var exception = CompileExpecting<StackTypeInvalidException>(
            new Int32Constant(0), new Int32TruncateSaturateFloat32Signed(), new End());

        Assert.AreEqual(MiscellaneousOpCode.Int32TruncateSaturateFloat32Signed, exception.MiscellaneousOpCode);
        Assert.AreEqual(WebAssemblyValueType.Float32, exception.Expected);
        Assert.AreEqual(WebAssemblyValueType.Int32, exception.Actual);
        StringAssert.StartsWith(exception.Message, $"{MiscellaneousOpCode.Int32TruncateSaturateFloat32Signed} ");
    }

    /// <summary>
    /// A SIMD (0xFD) instruction with too few operands reports its specific opcode, not the prefix.
    /// </summary>
    [TestMethod]
    public void Simd_StackTooSmall_NamesSpecificOpCode()
    {
        var exception = CompileExpecting<StackTooSmallException>(new V128And(), new End());

        Assert.AreEqual(SimdOpCode.V128And, exception.SimdOpCode);
        Assert.IsNull(exception.MiscellaneousOpCode);
        StringAssert.StartsWith(exception.Message, $"{SimdOpCode.V128And} ");
        Assert.IsFalse(exception.Message.Contains(nameof(OpCode.SimdOperationPrefix)), exception.Message);
    }

    /// <summary>
    /// A SIMD (0xFD) instruction given the wrong operand type reports its specific opcode.
    /// </summary>
    [TestMethod]
    public void Simd_StackTypeInvalid_NamesSpecificOpCode()
    {
        var exception = CompileExpecting<StackTypeInvalidException>(
            new Int32Constant(0), new V128And(), new End());

        Assert.AreEqual(SimdOpCode.V128And, exception.SimdOpCode);
        Assert.AreEqual(WebAssemblyValueType.V128, exception.Expected);
        Assert.AreEqual(WebAssemblyValueType.Int32, exception.Actual);
        StringAssert.StartsWith(exception.Message, $"{SimdOpCode.V128And} ");
    }
}
