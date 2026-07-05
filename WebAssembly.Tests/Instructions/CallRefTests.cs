using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAssembly.Runtime;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="CallRef"/> instruction.
/// </summary>
[TestClass]
public class CallRefTests
{
    // func0 doubles its argument; func1 (exported "Test") obtains a typed reference to func0 and calls it via call_ref.
    private static Module CreateModule()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function(0));
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase<int>.Test), 1));

        var declared = new Element { Kind = ElementKind.DeclarativeFunctionIndices };
        declared.Elements.Add(0);
        module.Elements.Add(declared);

        // func0: n * 2
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new Int32Constant(2), new Int32Multiply(), new End()));

        // func1: call_ref (ref.func 0)(n)
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new RefFunc(0), new CallRef(0), new End()));

        return module;
    }

    /// <summary>
    /// Tests compilation and execution of <see cref="CallRef"/> through a typed function reference.
    /// </summary>
    [TestMethod]
    public void CallRef_Compiled()
    {
        var exports = CreateModule().ToInstance<CompilerTestBase<int>>().Exports;
        Assert.AreEqual(0, exports.Test(0));
        Assert.AreEqual(10, exports.Test(5));
        Assert.AreEqual(-4, exports.Test(-2));
    }

    /// <summary>
    /// A <c>call_ref</c> on a null reference traps (surfacing as a <see cref="System.NullReferenceException"/>).
    /// </summary>
    [TestMethod]
    public void CallRef_Null_Traps()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType
        {
            Parameters = [WebAssemblyValueType.Int32],
            Returns = [WebAssemblyValueType.Int32],
        });
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export(nameof(CompilerTestBase<int>.Test), 0));

        // call_ref (ref.null 0)(n) — the null reference traps at the invoke.
        module.Codes.Add(new FunctionBody(
            new LocalGet(0), new RefNull { TypeIndex = 0 }, new CallRef(0), new End()));

        var exports = module.ToInstance<CompilerTestBase<int>>().Exports;
        Assert.ThrowsExactly<System.NullReferenceException>(() => exports.Test(1));
    }

    /// <summary>
    /// Tests the <see cref="CallRef.Equals(Instruction)"/> and <see cref="CallRef.GetHashCode()"/> methods.
    /// </summary>
    [TestMethod]
    public void CallRef_Equals()
    {
        TestUtility.CreateInstances<CallRef>(out var a, out var b);

        a.Type = 1;
        TestUtility.AreNotEqual(a, b);
        b.Type = 1;
        TestUtility.AreEqual(a, b);
    }
}
