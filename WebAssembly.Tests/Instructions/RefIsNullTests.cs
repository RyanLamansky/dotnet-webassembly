using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="RefIsNull"/> instruction.
/// </summary>
[TestClass]
public class RefIsNullTests
{
    /// <summary>Tests the <see cref="RefIsNull"/> opcode and equality.</summary>
    [TestMethod]
    public void RefIsNull_OpCodeAndEquality()
    {
        var instruction = new RefIsNull();
        Assert.AreEqual(OpCode.RefIsNull, instruction.OpCode);
        Assert.AreEqual(new RefIsNull(), instruction);
    }

    /// <summary>Minimal export for the validation test.</summary>
    public abstract class RefIsNullExport
    {
        /// <summary>The exported function.</summary>
        public abstract int Test();
    }

    /// <summary>
    /// <c>ref.is_null</c> applied to a non-reference operand must be rejected during compilation rather
    /// than emitting a nonsensical comparison of a numeric value against null.
    /// </summary>
    [TestMethod]
    public void RefIsNull_NonReferenceOperand_Rejected()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType { Parameters = [], Returns = [WebAssemblyValueType.Int32] });
        module.Functions.Add(new Function());
        module.Exports.Add(new Export { Name = nameof(RefIsNullExport.Test) });
        module.Codes.Add(new FunctionBody
        {
            Code = [new Int32Constant(0), new RefIsNull(), new End()],
        });

        Assert.ThrowsExactly<StackTypeInvalidException>(() => module.ToInstance<RefIsNullExport>());
    }
}
