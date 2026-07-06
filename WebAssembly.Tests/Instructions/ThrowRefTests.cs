using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace WebAssembly.Instructions;

/// <summary>
/// Tests the <see cref="ThrowRef"/> instruction.
/// </summary>
[TestClass]
public class ThrowRefTests
{
    /// <summary>Tests <see cref="ThrowRef"/> opcode and equality.</summary>
    [TestMethod]
    public void ThrowRef_Equality()
    {
        var instruction = new ThrowRef();
        Assert.AreEqual(OpCode.ThrowRef, instruction.OpCode);
        Assert.AreEqual(new ThrowRef(), instruction);
        Assert.AreEqual(new ThrowRef().GetHashCode(), instruction.GetHashCode());
    }

    /// <summary>Exposes a function that re-throws a null exnref.</summary>
    public abstract class ThrowRefExports
    {
        /// <summary>Runs the re-throwing function.</summary>
        public abstract void Run();
    }

    /// <summary><c>throw_ref</c> of a null exnref traps with a null reference.</summary>
    [TestMethod]
    public void ThrowRef_Null_Traps()
    {
        var module = new Module();
        module.Types.Add(new WebAssemblyType());     // 0: () -> ()
        module.Functions.Add(new Function(0));
        module.Exports.Add(new Export("Run", 0, ExternalKind.Function));
        module.Codes.Add(new FunctionBody
        {
            Code = [new RefNull(WebAssemblyHeapType.Exn), new ThrowRef(), new End()],
        });

        var exports = module.ToInstance<ThrowRefExports>().Exports;

        try
        {
            exports.Run();
        }
        catch (TargetInvocationException x) when (x.InnerException is NullReferenceException)
        {
            return;
        }
        catch (NullReferenceException)
        {
            return;
        }

        throw new AssertFailedException("throw_ref of null did not trap with a NullReferenceException.");
    }
}
